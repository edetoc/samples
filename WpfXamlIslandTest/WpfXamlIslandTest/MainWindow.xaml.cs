using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfXamlIslandTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowsXamlHost_ChildChanged(object sender, EventArgs e)
        {
            // Hook up x:Bind source.
            global::Microsoft.Toolkit.Wpf.UI.XamlHost.WindowsXamlHost windowsXamlHost =
                sender as global::Microsoft.Toolkit.Wpf.UI.XamlHost.WindowsXamlHost;
            global::CustomUWPControlClassLibrary.MyUWPUserControl userControl =
                windowsXamlHost.GetUwpInternalObject() as global::CustomUWPControlClassLibrary.MyUWPUserControl;

            if (userControl != null)
            {
                userControl.XamlIslandMessage = this.WPFMessage;
            }
        }

        public string WPFMessage
        {
            get
            {
                return "this text is a binding from WPF to UWP XAML";
            }
        }
    }
}
