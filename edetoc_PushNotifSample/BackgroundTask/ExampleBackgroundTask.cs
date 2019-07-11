using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;
using BackgroundTask.Helpers;

namespace BackgroundTask
{
    public sealed class ExampleBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();          

            // Get the raw notification content
            RawNotification notification = (RawNotification)taskInstance.TriggerDetails;
            string content = notification.Content;

            //// Create sample file; replace if exists.
            //Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            //Windows.Storage.StorageFile sampleFile =   await storageFolder.CreateFileAsync("sample.txt",
            //        Windows.Storage.CreationCollisionOption.ReplaceExisting);

            // Display a toast
            ToastHelper.PopToast("Push Notification received", content);

            _deferral.Complete();
        }


  
    }
}
