﻿using DiagnosticsHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WebSocketTransportHelper;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Data.Xml.Dom;
using Windows.Networking.Sockets;
using Windows.UI.Notifications;

namespace BackgroundTaskHelper
{
  
    public sealed class PushNotifyTask : IBackgroundTask
    {
        [DllImport("kernel32.dll")]
        static extern void OutputDebugString(string lpOutputString);

        void InvokeSimpleToast(string messageReceived)
        {
            // GetTemplateContent returns a Windows.Data.Xml.Dom.XmlDocument object containing
            // the toast XML
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // You can use the methods from the XML document to specify all of the
            // required parameters for the toast
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements.Item(0).AppendChild(toastXml.CreateTextNode("Push notification message:"));
            stringElements.Item(1).AppendChild(toastXml.CreateTextNode(messageReceived));

            // Audio tags are not included by default, so must be added to the
            // XML document
            string audioSrc = "ms-winsoundevent:Notification.IM";
            XmlElement audioElement = toastXml.CreateElement("audio");
            audioElement.SetAttribute("src", audioSrc);

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            toastNode.AppendChild(audioElement);

            // Create a toast from the Xml, then create a ToastNotifier object to show
            // the toast
            ToastNotification toast = new ToastNotification(toastXml);

            OutputDebugString("edetocCCTSample_Tracing:  PushNotifyTask Show Toast");

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void Run(Windows.ApplicationModel.Background.IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
            {
                //Diag.DebugPrint("PushNotifyTask: taskInstance was null");
                OutputDebugString("edetocCCTSample_Tracing:  " + "PushNotifyTask: taskInstance was null");
                return;
            }

            OutputDebugString("edetocCCTSample_Tracing:  " + "PushNotifyTask " + taskInstance.Task.Name + " Starting...");

            //Diag.DebugPrint("PushNotifyTask " + taskInstance.Task.Name + " Starting...");

           // Use the ControlChannelTriggerEventDetails object to derive the context for this background task.
           // The context happens to be the channelId that apps can use to differentiate between
           // various instances of the channel..
           var channelEventArgs = taskInstance.TriggerDetails as IControlChannelTriggerEventDetails;

            ControlChannelTrigger channel = channelEventArgs.ControlChannelTrigger;
            if (channel == null)
            {
                //Diag.DebugPrint("Channel object may have been deleted.");
                OutputDebugString("edetocCCTSample_Tracing:  " + "PushNotifyTask " + "Channel object may have been deleted.");
                return;
            }

            string channelId = channel.ControlChannelTriggerId;

            if (((IDictionary<string, object>)CoreApplication.Properties).ContainsKey(channelId))
            {
                OutputDebugString("edetocCCTSample_Tracing:  " + "PushNotifyTask " + "ChannelId found");

                try
                {
                    string messageReceived = "PushNotification Received";
                    var appContext = ((IDictionary<string, object>)CoreApplication.Properties)[channelId] as ApplicationContext;

                    // Process any messages that have been enqueued by the receive completion handler.
                    bool result = appContext.RecievedMessage.TryDequeue(out messageReceived);
                    if (result)
                    {
                        //Diag.DebugPrint("Message: " + messageReceived);
                        OutputDebugString("edetocCCTSample_Tracing:  " + "PushNotifyTask " + "Message: " + messageReceived);
                        InvokeSimpleToast(messageReceived);
                    }
                    else
                    {
                        //Diag.DebugPrint("There was no message for this push notification: ");
                        OutputDebugString("edetocCCTSample_Tracing:  " + "PushNotifyTask " + "There was no message for this push notification");
                    }
                }
                catch (Exception exp)
                {
                    OutputDebugString("edetocCCTSample_Tracing:  " + "PushNotifyTask failed with: " + exp.Message);
                    //Diag.DebugPrint("PushNotifyTask failed with: " + exp.Message);
                }
            }
            else
            {
                OutputDebugString("edetocCCTSample_Tracing:  " + "PushNotifyTask " + "Cannot find AppContext key " + channelId);
                //Diag.DebugPrint("Cannot find AppContext key " + channelId);
            }

            OutputDebugString("edetocCCTSample_Tracing:  " + "PushNotifyTask " + taskInstance.Task.Name + " finished.");
            //Diag.DebugPrint("PushNotifyTask " + taskInstance.Task.Name + " finished.");
        }
    }


    // This class illustrates one way to set up a RTC enabled transport when 
    // a system event (such as network state change) occurs.
    public sealed class NetworkChangeTask : IBackgroundTask
    {

        [DllImport("kernel32.dll")]
        static extern void OutputDebugString(string lpOutputString);

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
            {
                Diag.DebugPrint("NetworkChangeTask: taskInstance was null");
                return;
            }

            OutputDebugString("edetocCCTSample_Tracing:  NetworkChangeTask runs!");

            // In this example, the channel name has been hardcoded to lookup the property bag
            // for any previous contexts. The channel name may be used in more sophisticated ways
            // in case an app has multiple controlchanneltrigger objects.
           string channelId = "channelOne";
            if (((IDictionary<string, object>)CoreApplication.Properties).ContainsKey(channelId))
            {
                try
                {
                    var appContext = ((IDictionary<string, object>)CoreApplication.Properties)[channelId] as ApplicationContext;
                    if (appContext != null && appContext.CommInstance != null)
                    {
                        CommModule commInstance = appContext.CommInstance;

                        // Clear any existing channels, sockets etc.
                        commInstance.Reset();

                        // Create RTC enabled transport
                        commInstance.SetupTransport(commInstance.serverUri);
                    }
                }
                catch (Exception exp)
                {
                    Diag.DebugPrint("Registering with RTC broker failed with: " + exp.Message);
                }
            }
            else
            {
                Diag.DebugPrint("Cannot find AppContext key channelOne");
            }

            Diag.DebugPrint("Systemtask - " + taskInstance.Task.Name + " finished.");
        }
    }

}
