using DiagnosticsHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace WebSocketTransportHelper
{
    public class CommModule : IDisposable
    {

        [DllImport("kernel32.dll")]
        static extern void OutputDebugString(string lpOutputString);


        const int TIMEOUT = 30000;
        const int MAX_BUFFER_LENGTH = 100;

        MessageWebSocket socket;
        public ControlChannelTrigger channel;
        DataReader readPacket;
        public string serverUri;
        DataWriter writePacket;

        public CommModule()
        {
        }

        public void Dispose()
        {
            Reset();
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            lock (this)
            {
                if (readPacket != null)
                {
                    try
                    {
                        readPacket.DetachStream();
                        readPacket = null;
                    }
                    catch (Exception exp)
                    {
                        Diag.DebugPrint("Could not detach DataReader: " + exp.Message);
                    }
                }

                if (writePacket != null)
                {
                    try
                    {
                        writePacket.DetachStream();
                        writePacket = null;
                    }
                    catch (Exception exp)
                    {
                        Diag.DebugPrint("Could not detach DataWriter: " + exp.Message);
                    }
                }

                if (socket != null)
                {
                    socket.Dispose();
                    socket = null;
                }

                if (channel != null)
                {
                    if (((IDictionary<string, object>)CoreApplication.Properties).ContainsKey(channel.ControlChannelTriggerId))
                    {
                        CoreApplication.Properties.Remove(channel.ControlChannelTriggerId);
                    }

                    // Call the Dispose() method on the controlchanneltrigger object to release any 
                    // OS maintained resources for this channel object. 
                    channel.Dispose();
                    channel = null;
                }
                Diag.DebugPrint("CommModule has been reset.");
            }
        }

        private bool RegisterWithControlChannelTrigger(string serverUri)
        {

            // Make sure the objects are created in a system thread that are guaranteed
            // to run in an MTA. Any objects that are required for use within background
            // tasks must not be affinitized to the ASTA.
            //
            // To simplify consistency issues for the commModule instance, 
            // demonstrate the core registration path to use async await 
            // but wait for the entire operation to complete before returning from this method.
            // The transport setup routine can be triggered by user control, by network state change
            // or by keepalive task and a typical app must be resilient against all of 

            //Diag.DebugPrint("About to call RegisterWithCCTHelper");

            OutputDebugString("edetocCCTSample_Tracing:   Entering RegisterWithControlChannelTrigger");

            Task<bool> registerTask = RegisterWithCCTHelper(serverUri);

            // edetoc
            registerTask.Wait();

            OutputDebugString("edetocCCTSample_Tracing:   Leaving RegisterWithControlChannelTrigger");

            return registerTask.Result;

        }

        async Task<bool> RegisterWithCCTHelper(string serverUri)
        {
            bool result = false;
            socket = new MessageWebSocket();
            socket.MessageReceived += Socket_MessageReceived;

            // Specify the keepalive interval expected by the server for this app
            // in order of minutes.
            const int serverKeepAliveInterval = 30;

            // Specify the channelId string to differentiate this
            // channel instance from any other channel instance.
            // When background task fires, the channel object is provided
            // as context and the channel id can be used to adapt the behavior
            // of the app as required.
            const string channelId = "ControlChannelWebSocketUWP";

            // IMPORTANT: Note that this is a websocket sample, therefore the 
            // keepalive task class is provided by Windows for websockets. 
            // For websockets, the system does the keepalive on behalf of the
            // app but the app still needs to specify this well known keepalive task.
            // This should be done here in the background registration as well 
            // as in the package manifest.
            const string WebSocketKeepAliveTask = "Windows.Networking.Sockets.WebSocketKeepAlive";

            // Try creating the controlchanneltrigger if this has not been already 
            // created and stored in the property bag.
            //Diag.DebugPrint("RegisterWithCCTHelper Starting...");
            ControlChannelTriggerStatus status;
            //Diag.DebugPrint("About to create ControlChannelTrigger ...");

            // Create the ControlChannelTrigger object and request a hardware slot for this app.
            // If the app is not on LockScreen, then the ControlChannelTrigger constructor will 
            // fail right away.

            OutputDebugString("edetocCCTSample_Tracing:   Entering RegisterWithCCTHelper");
            try
            {
                channel = new ControlChannelTrigger(channelId, serverKeepAliveInterval,
                                   ControlChannelTriggerResourceType.RequestHardwareSlot);
            }
            catch (UnauthorizedAccessException exp)
            {
                //Diag.DebugPrint("Please add the app to the lock screen." + exp.Message);
                OutputDebugString("edetocCCTSample_Tracing:   Failed to create ControlChannelTrigger" + exp.Message);

                return result;
            }

            OutputDebugString("edetocCCTSample_Tracing:   ControlChannelTrigger created with success");

            //Diag.DebugPrint("ControlChannelTrigger creation OK");

            Uri serverUriInstance;
            try
            {
                serverUriInstance = new Uri(serverUri);
                OutputDebugString("edetocCCTSample_Tracing:   Uri is " + serverUriInstance.ToString());
            }
            catch (Exception exp)
            {
                OutputDebugString("edetocCCTSample_Tracing: error create URI: " + exp.Message);
                //Diag.DebugPrint("Error creating URI: " + exp.Message);
                return result;
            }


            // Register the apps background task with the trigger for keepalive.
            var keepAliveBuilder = new BackgroundTaskBuilder();
            keepAliveBuilder.Name = "KeepaliveTaskFor" + channelId;
            keepAliveBuilder.TaskEntryPoint = WebSocketKeepAliveTask;
            keepAliveBuilder.SetTrigger(channel.KeepAliveTrigger);
            keepAliveBuilder.Register();
            OutputDebugString("edetocCCTSample_Tracing:   WebSocketKeepAliveTask registered");
            //Diag.DebugPrint("edetoc - BG keepAliveBuilder register OK");

            // Register the apps background task with the trigger for push notification task.
            var pushNotifyBuilder = new BackgroundTaskBuilder();
            pushNotifyBuilder.Name = "PushNotificationTaskFor" + channelId;
            pushNotifyBuilder.TaskEntryPoint = "BackgroundTaskHelper.PushNotifyTask";
            pushNotifyBuilder.SetTrigger(channel.PushNotificationTrigger);
            pushNotifyBuilder.Register();
            //Diag.DebugPrint("edetoc - BG pushNotifyBuilder register OK");
            OutputDebugString("edetocCCTSample_Tracing:   PushNotifyTask registered");

            // Tie the transport method to the ControlChannelTrigger object to push enable it.
            // Note that if the transport's TCP connection is broken at a later point of time,
            // the ControlChannelTrigger object can be reused to plug in a new transport by
            // calling UsingTransport API again.
            //Diag.DebugPrint("Calling UsingTransport() ...");
            try
            {
                OutputDebugString("edetocCCTSample_Tracing:   Before UsingTransport");

                channel.UsingTransport(socket);

                OutputDebugString("edetocCCTSample_Tracing:   After UsingTransport");

                // Connect the socket
                //
                // If connect fails or times out it will throw exception.
                OutputDebugString(string.Format ("edetocCCTSample_Tracing:  {0} Before ConnectAsync", DateTime.Now.ToString()));
                
                await socket.ConnectAsync(serverUriInstance);
                
                OutputDebugString("edetocCCTSample_Tracing:   After ConnectASync");

                //Diag.DebugPrint("Socket connected");

                // Call WaitForPushEnabled API to make sure the TCP connection has 
                // been established, which will mean that the OS will have allocated 
                // any hardware slot for this TCP connection.
                //
                // In this sample, the ControlChannelTrigger object was created by 
                // explicitly requesting a hardware slot.
                //
                // On Non-AOAC systems, if app requests hardware slot as above, 
                // the system will fallback to a software slot automatically.
                //
                // On AOAC systems, if no hardware slot is available, then app 
                // can request a software slot [by re-creating the ControlChannelTrigger object].
                status = channel.WaitForPushEnabled();

                //Diag.DebugPrint("WaitForPushEnabled() completed with status: " + status);
                if (status != ControlChannelTriggerStatus.HardwareSlotAllocated
                    && status != ControlChannelTriggerStatus.SoftwareSlotAllocated)
                {
                    OutputDebugString("edetocCCTSample_Tracing:   Neither hardware nor software slot could be allocated");
                    throw new Exception(string.Format("Neither hardware nor software slot could be allocated. ChannelStatus is {0}", status.ToString()));
                }

                OutputDebugString("edetocCCTSample_Tracing:  WaitForPushEnabled OK"); 

                // Store the objects created in the property bag for later use.
                // NOTE: make sure these objects are free threaded. STA/Both objects can 
                // cause deadlocks when foreground threads are suspended.
                CoreApplication.Properties.Remove(channel.ControlChannelTriggerId);

                var appContext = new ApplicationContext(this, socket, channel, channel.ControlChannelTriggerId);
                ((IDictionary<string, object>)CoreApplication.Properties).Add(channel.ControlChannelTriggerId, appContext);
                result = true;
                //Diag.DebugPrint("RegisterWithCCTHelper Completed.");
            }
            catch (Exception exp)
            {
                //Diag.DebugPrint("RegisterWithCCTHelper Task failed with: " + exp.Message);
                OutputDebugString("edetocCCTSample_Tracing:   RegisterWithCCTHelper  failed with: " + exp.Message);

                // Exceptions may be thrown for example if the application has not 
                // registered the background task class id for using real time communications 
                // broker in the package appx manifest.
            }
            return result;
        }

        private void Socket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            OutputDebugString("edetocCCTSample_Tracing: Socket_MessageReceived fired");
            using (DataReader reader = args.GetDataReader())
            {
                if (((IDictionary<string, object>)CoreApplication.Properties).ContainsKey(channel.ControlChannelTriggerId))
                {
                    OutputDebugString("edetocCCTSample_Tracing: In Socket_MessageReceived . Channel ID found");

                    var appContext = ((IDictionary<string, object>)CoreApplication.Properties)[channel.ControlChannelTriggerId] as ApplicationContext;
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    var message = reader.ReadString(reader.UnconsumedBufferLength);
                    OutputDebugString("edetocCCTSample_Tracing:   message is: " + message);
                    appContext.RecievedMessage.Enqueue(message);
                }
            }
        }

        public bool SetupTransport(string serviceUri)
        {
            OutputDebugString("edetocCCTSample_Tracing:   Entering SetupTransport");

            bool result = false;
            lock (this)
            {
                // Save these to help reconnect later.
                serverUri = serviceUri;


                //Diag.DebugPrint("edetoc - About to call RegisterWithControlChannelTrigger");

                // Set up the ControlChannelTrigger with the stream socket.
                result = RegisterWithControlChannelTrigger(serverUri);
                
                if (result == false)
                {
                    Diag.DebugPrint("Failed to sign on and connect");
                    if (socket != null)
                    {
                        socket.Dispose();
                        socket = null;
                        readPacket = null;
                    }
                    if (channel != null)
                    {
                        channel.Dispose();
                        channel = null;
                    }
                }
                else
                {
                    //Diag.DebugPrint("edetoc - RegisterWithControlChannelTrigger OK");
                }
            }

            OutputDebugString("edetocCCTSample_Tracing:   Leaving SetupTransport");

            return result;
        }
    }

    public class ApplicationContext
    {
        public ApplicationContext(CommModule commInstance, MessageWebSocket webSocket, ControlChannelTrigger channel, string id)
        {
            WebSocketHandle = webSocket;
            Channel = channel;
            ChannelId = id;
            CommInstance = commInstance;
            RecievedMessage = new ConcurrentQueue<string>();
        }

        public ConcurrentQueue<string> RecievedMessage;

        public MessageWebSocket WebSocketHandle { get; set; }
        public ControlChannelTrigger Channel { get; set; }
        public string ChannelId { get; set; }
        public CommModule CommInstance { get; set; }
    }
}


namespace DiagnosticsHelper
{
    public static class Diag
    {
        public static CoreDispatcher coreDispatcher;
        public static TextBlock debugOutputTextBlock;
        public static async void DebugPrint(string msg)
        {
            Debug.WriteLine(msg);
            if (coreDispatcher != null)
            {
                await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        debugOutputTextBlock.Text =
                            debugOutputTextBlock.Text +
                                DateTime.Now.ToString(@"M/d/yyyy hh:mm:ss tt") + " " + msg + "\r\n";
                    });
            }
        }
    }
}