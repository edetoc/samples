using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AppServicesClientWithBGTask
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<string> logItems;

        public MainPage()
        {
            logItems = new ObservableCollection<string>();

            this.InitializeComponent();
            
            LogLv.ItemsSource = logItems;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {

            var taskRegistered = false;
            var taskName = "SampleBackgroundTask";

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (taskRegistered)
            {
                logItems.Add("task already registered");
                return;
            }

            logItems.Add("task not found, let's create it");
            // Create background task

            var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if ((requestStatus == BackgroundAccessStatus.DeniedBySystemPolicy)
                || (requestStatus == BackgroundAccessStatus.DeniedByUser))
            {
                logItems.Add(string.Format("RequestAccessAsync failed: {0}", requestStatus.ToString()));
                return;
            }

            logItems.Add("RequestAccessAsync succeeded");

            var builder = new BackgroundTaskBuilder();

            builder.Name = "SampleBackgroundTask";
            builder.TaskEntryPoint = "BGTaskRuntimeComponent.SampleBackgroundTask";
            builder.SetTrigger(new TimeTrigger(15, false)); // periodic every 15 minutes


            // register background task
            BackgroundTaskRegistration task = builder.Register();

            logItems.Add("Background task registered");

            AttachCompletedHandler(task);

        }

        private void UnregisterButton_Click(object sender, RoutedEventArgs e)
        {

            var taskName = "SampleBackgroundTask";

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                    logItems.Add("Background task unregistered");
                }
            }
        }

        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachCompletedHandler(IBackgroundTaskRegistration task)
        {
            //task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
        }

        private async void OnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
           () =>
           {
                //RegisterButton.IsEnabled = !BackgroundTaskSample.TimeTriggeredTaskRegistered;
                //UnregisterButton.IsEnabled = BackgroundTaskSample.TimeTriggeredTaskRegistered;
                //Progress.Text = BackgroundTaskSample.TimeTriggeredTaskProgress;
                //Status.Text = BackgroundTaskSample.GetBackgroundTaskStatus(BackgroundTaskSample.TimeTriggeredTaskName);

                logItems.Add(string.Format("{0} Background Task completed", DateTime.Now.ToString()));

           });
        }

        private void ResetLogButton_Click(object sender, RoutedEventArgs e)
        {
            logItems.Clear();
        }
    }
}
