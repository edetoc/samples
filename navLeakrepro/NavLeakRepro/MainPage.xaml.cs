using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NavLeakRepro
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

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Page2));
        }

        private void GCbutton_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect(2);
            GC.Collect(2);
            GC.WaitForPendingFinalizers();
            GC.Collect(2);
            GC.Collect(2);
        }


    }
}
