using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace edetocCCTSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        [DllImport("kernel32.dll")]
        static extern void OutputDebugString(string lpOutputString);

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            OutputDebugString("edetocCCTSample_Tracing:  In MainPage OnNavigatedTo");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BackgroundExecutionManager.RequestAccessAsync  status:");

            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
            
            switch (status)
            {
                case BackgroundAccessStatus.AlwaysAllowed:

                    // App is allowed to use RealTimeConnection broker 
                    // functionality even in low power mode.
                    sb.AppendLine("BackgroundAccessStatus.AlwaysAllowed");
                    break;
                                        
                case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:

                    // App is allowed to use RealTimeConnection broker 
                    // functionality but not in low power mode.
                    sb.AppendLine("BackgroundAccessStatus.AllowedSubjectToSystemPolicy");
                    break;

                case BackgroundAccessStatus.DeniedBySystemPolicy:

                    sb.AppendLine("BackgroundAccessStatus.DeniedBySystemPolicy");
                    break;

                case BackgroundAccessStatus.DeniedByUser:

                    sb.AppendLine("BackgroundAccessStatus.DeniedByUser");
                    // App should switch to polling mode (example: poll for email based on time triggers)

                    break;
            }

            sb.AppendLine();
            sb.AppendLine("Minimize the app to trigger suspension" );
            TB.Text = sb.ToString();
            
        }

        private void Session_Revoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            OutputDebugString("edetocCCTSample_Tracing:   ExtendedExecution revoked");
        }
    }
}
