using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DragNDropSample
{
   
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Panel_DragStarting(UIElement sender, DragStartingEventArgs args)
        {

            args.Data.RequestedOperation = DataPackageOperation.Copy;

            var file = await StorageFile.CreateStreamedFileAsync("test.txt", async (streamRequest) =>
            {
                // this code is called when the item is dropped 
                var bytes = Encoding.UTF8.GetBytes("Hello world!");
                await RandomAccessStream.CopyAndCloseAsync(new MemoryStream(bytes, false).AsInputStream(), streamRequest);
            }, null);

            args.Data.SetStorageItems(new[] { file });

        }


        private void DestGrid_DragOver(object sender, DragEventArgs e)
        {
            // Our grid only accepts storage items
            e.AcceptedOperation = (e.DataView.Contains(StandardDataFormats.StorageItems)) ? DataPackageOperation.Copy : DataPackageOperation.None;
        }

        private async void DestGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                // We need to take a Deferral as we won't be able to confirm the end
                // of the operation synchronously
                var def = e.GetDeferral();

                var items = await e.DataView.GetStorageItemsAsync();

                StringBuilder sb = new StringBuilder();

                foreach (var item in items)
                {
                    sb.AppendLine("Dropped file name: " + item.Name);

                    var storageFile = item as StorageFile;

                    using (var stream = await storageFile.OpenReadAsync())
                    {

                        using (DataReader textReader = new DataReader(stream))
                        {
                            uint textLength = (uint)stream.Size;
                            await textReader.LoadAsync(textLength);
                            var content = textReader.ReadString(textLength);

                            sb.AppendLine("content: " + content);

                        }

                    }

                    var md = new MessageDialog(sb.ToString());
                    await md.ShowAsync();

                    e.AcceptedOperation = DataPackageOperation.Copy;

                    def.Complete();
                }
            }
        }

      
    }
}
