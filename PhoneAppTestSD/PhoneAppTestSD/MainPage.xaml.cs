using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneAppTestSD.Resources;
using Windows.Storage;
using System.Text;

namespace PhoneAppTestSD
{
    public partial class MainPage : PhoneApplicationPage
    {
        private StringBuilder _fileNames;
        // Constructor
        public  MainPage()
        {
            InitializeComponent();
            _fileNames = new StringBuilder();
            
        }

        private async void btnReadFileFromSD_Click(object sender, RoutedEventArgs e)
        {
            _fileNames.Clear();

            var folders = await Windows.Storage.KnownFolders.RemovableDevices.GetFoldersAsync();

            foreach (var storageFolder in folders)
            {
                var files = await storageFolder.GetFilesAsync();

                foreach (var storageFile in files)
                {
                    _fileNames.AppendLine(storageFile.Name);
                }
                
            }

            string str = _fileNames.ToString ();
            if (String.IsNullOrEmpty(str))
            {
                MessageBox.Show("No file found!");
                tbkFileNames.Text = "<empty>";
            }
            else
                tbkFileNames.Text = str;
              
        }

     
    }
}