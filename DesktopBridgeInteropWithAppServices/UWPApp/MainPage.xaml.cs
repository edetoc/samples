using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        #region Start full trust processes
        private async void StartSumButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
                {
                    await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("SumId");
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.Message);
            }
        }

        private async void StartMulButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
                {
                    await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("MulId");
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.Message);
            }
        }

        #endregion

        #region Stop full trust processes
        private async void StopSumButton_Click(object sender, RoutedEventArgs e)
        {
            ValueSet valueSet = new ValueSet();
            valueSet.Add("killSumApp", "please kill yourself!");

            if (App.sumConn != null)
            {

                var response = await App.sumConn.SendMessageAsync(valueSet);
                if (response.Status == AppServiceResponseStatus.Success)
                    ResultField.Text = response.Message["response"] as string;
                else
                    ResultField.Text = "ERROR!";

                App.sumConn.Dispose();
                App.sumConn = null;
            }

        }

        private async void StopMulButton_Click(object sender, RoutedEventArgs e)
        {
            ValueSet valueSet = new ValueSet();
            valueSet.Add("killMulApp", "please kill yourself!");

            if (App.mulConn != null)
            {
                var response = await App.mulConn.SendMessageAsync(valueSet);

                if (response.Status == AppServiceResponseStatus.Success)
                    ResultField.Text = response.Message["response"] as string;
                else
                    ResultField.Text = "ERROR!";

                App.mulConn.Dispose();
                App.mulConn = null;
            }
        }

        #endregion

        #region Compute

      
        private async void SumButton_Click(object sender, RoutedEventArgs e)
        {
            // Get numbers 1 and 2 from fields
            int num1, num2;

            if (!Int32.TryParse(Number1Field.Text, out num1))
                return;

            if (!Int32.TryParse(Number2Field.Text, out num2))
                return;

            ValueSet valueSet = new ValueSet();
            valueSet.Add("sum", string.Format("{0};{1}", num1.ToString(), num2.ToString()));

            if (App.sumConn != null)
            {
                var response = await App.sumConn.SendMessageAsync(valueSet);
                ResultField.Text = response.Message["response"] as string;
            }

        }

        private async void MulButton_Click(object sender, RoutedEventArgs e)
        {
            // Get numbers 1 and 2 from fields
            int num1, num2;

            if (!Int32.TryParse(Number1Field.Text, out num1))
                return;

            if (!Int32.TryParse(Number2Field.Text, out num2))
                return;

            ValueSet valueSet = new ValueSet();
            valueSet.Add("mul", string.Format("{0};{1}", num1.ToString(), num2.ToString()));

            if (App.mulConn != null)
            {
                var response = await App.mulConn.SendMessageAsync(valueSet);
                ResultField.Text = response.Message["response"] as string;
            }
        }
        #endregion


    }
}
