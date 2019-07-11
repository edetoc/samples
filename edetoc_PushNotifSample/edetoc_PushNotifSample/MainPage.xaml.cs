using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace edetoc_PushNotifSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private static readonly string BACKGROUND_ENTRY_POINT = typeof(BackgroundTask.ExampleBackgroundTask).FullName;
        private PushNotificationChannel channel;

        private static String _status; 

        public MainPage()
        {
            this.InitializeComponent();
            
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _status += "Entering MainPage OnNavigatedTo\n";

            var isBGRegistered = await RegisterBackgroundTask();
           
            if (isBGRegistered )
            {
                await RegisterForPushNotification();
            }

            StatusTextBlock.Text = _status;            

        }


        private static void UnregisterBackgroundTask()
        {
            var task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(i => i.Name.Equals(BACKGROUND_ENTRY_POINT));

            if (task != null)
            {
                task.Unregister(true);
                _status += "Task found and unregistered\n";

            }
            else
                _status += "Task not found\n";

        }

        private async Task<bool> RegisterBackgroundTask()
        {
            // Unregister any previous exising background task
            UnregisterBackgroundTask();

            _status += "Constructing task\n";
            // Request access
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            // If denied
            if (status != BackgroundAccessStatus.AlwaysAllowed && status != BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
                return false;

            // Construct the background task
            var builder = new BackgroundTaskBuilder();

            builder.Name = BACKGROUND_ENTRY_POINT;
            builder.TaskEntryPoint = BACKGROUND_ENTRY_POINT;

            builder.IsNetworkRequested = true;           

            // Set trigger
            PushNotificationTrigger trigger = new PushNotificationTrigger();            

            builder.SetTrigger(trigger);

            // Register Background task
            BackgroundTaskRegistration registration = builder.Register();
            _status += "Task registered\n";

            return true;
        }



        private async Task RegisterForPushNotification()
        {
            channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            _status += "PN Channel created\n";
            _status += String.Format("{0}", channel.Uri);

            Debug.WriteLine(channel.Uri);
   
        }       

    }
}
