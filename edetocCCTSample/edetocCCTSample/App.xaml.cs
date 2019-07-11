using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WebSocketTransportHelper;
using Windows.ApplicationModel.ExtendedExecution;
using System.Threading.Tasks;

namespace edetocCCTSample
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        [DllImport("kernel32.dll")]
        static extern void OutputDebugString(string lpOutputString);
        
        private CommModule _communicationModule = null;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            this.Suspending += OnSuspending;
            this.Resuming += App_Resuming;

            // added by edetoc
            this.LeavingBackground += App_LeavingBackground;
            this.EnteredBackground += App_EnteredBackground;
        }

        

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private  void OnSuspending(object sender, SuspendingEventArgs e)
        {
            //var deferral = e.SuspendingOperation.GetDeferral();

            OutputDebugString("edetocCCTSample_Tracing:  Entering App_Suspending");
          
            OutputDebugString("edetocCCTSample_Tracing:  Leaving App_Suspending");

            //deferral.Complete();
        }

       

        private void App_Resuming(object sender, object e)
        {
            OutputDebugString("edetocCCTSample_Tracing:   App_Resuming");
            // DiagnosticsHelper.Diag.DebugPrint("OnResuming");
        }


        private void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            OutputDebugString("edetocCCTSample_Tracing : LeavingBackground");
        }

        private void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {

            
            OutputDebugString("edetocCCTSample_Tracing : EnteredBackground");

            var deferral = e.GetDeferral();
            
            NotifySuspend();

            deferral.Complete();
        }


        private void NotifySuspend()
        {

            //DiagnosticsHelper.UWP.Diag.DebugPrint("in NotifySuspend"); 
            OutputDebugString("edetocCCTSample_Tracing:   Entering NotifySuspend");


            try
            {
                Task.Run(() =>
                {
                    if (_communicationModule != null)
                    {
                        OutputDebugString("edetocCCTSample_Tracing:   Need to reset CommModule");
                        _communicationModule.Reset();
                        _communicationModule = null;
                        OutputDebugString("edetocCCTSample_Tracing:   CommModule reset done");
                    }

                    // create communication channel
                    _communicationModule = new CommModule();
                    _communicationModule.SetupTransport(GetServerUri());
                }).Wait();
                
            }
            catch (Exception ex)
            {
                OutputDebugString("edetocCCTSample_Tracing:   NotifySuspend. Exception" + ex.Message);
            }

            OutputDebugString("edetocCCTSample_Tracing:   Leaving NotifySuspend");
        }

        private string GetServerUri()
        {
            // URL of the ws server
            return "ws://172.29.138.33/websocketsample/echowebsocket.ashx";
        }
    }
}
