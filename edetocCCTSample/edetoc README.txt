This sample is a derived and simplified version of the ControlCHannelTrigger sample Windows 8.1 sample available here:
https://code.msdn.microsoft.com/windowsapps/ControlChannelTrigger-91f6bed8

It targets UWP 15063 (creators update)

The server part implementing the webSocket server is based on the .ps1 powershell script provided on the above URL
I have just modified the file EchoWebSocket.ashx provided in sample (see version attached to this solution) to do the following:
- the server sends a websocket message to the client 5 seconds after connection is established (this leaves enough time for the client to suspend)
- the server sends a recurring Websocket message every 10 seconds as long as the connection is opened.

Tracing has been added in the various .cs files (like app.xaml.cs, backgroudTasks.cs, CommModule.cs) using OutputDebugString (via p/invoke). 
this allows tracing of the app when it is not launched from VS (from the Start menu for instance)
The tool DebugView from SysInternals can be used to see the tracing output (make sure to check the "Capture Global Win32" option in the Capture menu).

The Scenario illustrated in this sample is :
You launch the App and then you minimize it after a few seconds
when the app is minimized the EnteredBackground (*) event is fired. 
In the event's handler, the app registers the BG tasks ( one with the ControlChannelTrigger, the other one to maintain a keep alive of the connection) and creates an underlying web socket that will be used for the communication with the websocket server when the app is suspended.
then the app goes to suspended state
5 seconds later the ControlChannelTrigger fires (server's response came in), which triggers the execution of the BG PushNotifyTask.
the task retrieves the message received and display its content to the user thanks to a toast notification.

(*) : see https://docs.microsoft.com/en-us/windows/uwp/launch-resume/app-lifecycle and https://docs.microsoft.com/en-us/uwp/api/Windows.ApplicationModel.Core.CoreApplication#Windows_ApplicationModel_Core_CoreApplication_EnteredBackground

Important  note:
The PushNotifyTask BG task executes in-proc (same process as the app), so the app process will run (we can see it running in task manager) for the duration the BG task executes, then it goes to suspended state again when the BG task has finished its work