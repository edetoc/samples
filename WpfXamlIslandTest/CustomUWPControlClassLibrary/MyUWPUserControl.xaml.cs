using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace CustomUWPControlClassLibrary
{
    public sealed partial class MyUWPUserControl : UserControl
    {


        [ComImport]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        interface IInitializeWithWindow
        {
            void Initialize(
                IntPtr hwnd);
        }


        public string XamlIslandMessage { get; set; }
        public MyUWPUserControl()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var fp = new FolderPicker();

            ((IInitializeWithWindow)(object)fp).Initialize(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);

            fp.FileTypeFilter.Add("*");
            var sf = await fp.PickSingleFolderAsync();

            if (sf != null)
                myButton.Content = String.Format ("you chose the {0} folder",sf.DisplayName);
        }
    }
}
