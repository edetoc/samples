using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;

namespace BGTaskRuntimeComponent
{
    public sealed class SampleBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            StringBuilder sbToastContent = new StringBuilder();

            using (var connection = new AppServiceConnection())
            {

                //Set up a new app service connection
                connection.AppServiceName = "com.microsoft.randomnumbergenerator";
                connection.PackageFamilyName = "Microsoft.SDKSamples.AppServicesProvider.CS_8wekyb3d8bbwe";
                AppServiceConnectionStatus status = await connection.OpenAsync();

                var bCanContinue = false;

                switch (status)
                {
                    case AppServiceConnectionStatus.Success:
                        // The new connection opened successfully
                        sbToastContent.AppendLine("Connection established.");
                        bCanContinue = true;
                        break;

                    case AppServiceConnectionStatus.AppNotInstalled:
                        sbToastContent.AppendLine("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.");
                        break;

                    case AppServiceConnectionStatus.AppUnavailable:
                        sbToastContent.AppendLine("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.");
                        break;

                    case AppServiceConnectionStatus.AppServiceUnavailable:
                        sbToastContent.AppendLine("The app AppServicesProvider is installed but it does not provide the app service" );
                        break;

                    default:
                    case AppServiceConnectionStatus.Unknown:
                        sbToastContent.AppendLine("An unknown error occurred while we were trying to open an AppServiceConnection.");
                        break;
                }

                if (bCanContinue)
                {
                    //Set up the inputs and send a message to the service
                    var inputs = new ValueSet();
                    inputs.Add("minvalue", 0);
                    inputs.Add("maxvalue", 10);
                    AppServiceResponse response = await connection.SendMessageAsync(inputs);

                    //If the service responded with success display the result and walk away
                    if (response.Status == AppServiceResponseStatus.Success &&
                        response.Message.ContainsKey("result"))
                    {
                        var resultText = response.Message["result"].ToString();
                        if (!string.IsNullOrEmpty(resultText))
                        {
                            //Result.Text = resultText;
                            sbToastContent.AppendLine(string.Format("App service responded with value {0}", resultText));
                        }
                        else
                        {
                            sbToastContent.AppendLine("App service did not respond with a result");
                        }

                        //return;
                    }
                    else
                    {
                        //Something went wrong while sending a message. Let display
                        //a meaningful error message
                        switch (response.Status)
                        {
                            case AppServiceResponseStatus.Failure:
                                sbToastContent.AppendLine("The service failed to acknowledge the message we sent it. It may have been terminated or it's RequestReceived handler might not be handling incoming messages correctly.");
                                break;

                            case AppServiceResponseStatus.ResourceLimitsExceeded:
                                sbToastContent.AppendLine("The service exceeded the resources allocated to it and had to be terminated.");
                                break;

                            default:
                            case AppServiceResponseStatus.Unknown:
                                sbToastContent.AppendLine("An unknown error occurred while we were trying to send a message to the service.");
                                break;
                        }
                    }
                   
                }
                else
                {
                    sbToastContent.AppendLine("App Service is unavailable");
                }

               
            }

            // Send local Toast notification

            PopToast(sbToastContent.ToString());

            _deferral.Complete();
        }


        private void PopToast(string text)
        {
            // create and send local notificatio  with new app version
            var xmlString = string.Format(@"
                            <toast>  
                                <visual>
                                    <binding template='ToastGeneric'>                                        
                                        <text>BG task result: {0}</text>
                                    </binding>      
                                </visual>
                            </toast>",
                            text);

            var longTime = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("longtime");
            DateTimeOffset expiryTime = DateTime.Now.AddSeconds(8);
            string expiryTimeString = longTime.Format(expiryTime);

            XmlDocument toastXml = new XmlDocument();
            toastXml.LoadXml(xmlString);

            // Set the duration on the toast
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            // Create the actual toast object using this toast specification.
            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = expiryTime;

            // Send the toast
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

      
    }
}
