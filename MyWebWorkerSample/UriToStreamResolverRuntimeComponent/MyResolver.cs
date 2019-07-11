using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace UriToStreamResolverRuntimeComponent
{

    /// Sample URI resolver object for use with NavigateToLocalStreamUri.
    /// The object must implement the IUriToStreamResolver interface
    public sealed class MyResolver : Windows.Web.IUriToStreamResolver
    {

        /// The entry point for resolving a Uri to a stream.
        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new Exception();
            }

           // remove first slash to keep filename
            string filename = uri.AbsolutePath.Remove (0,1);

            // Because of the signature of this method, it can't use await, so we 
            // call into a separate helper method that can use the C# await pattern.
            return getContent(filename).AsAsyncOperation();
        }

        /// <summary>
        /// Helper that maps the path to package content and resolves the Uri
        /// Uses the C# await pattern to coordinate async operations
        /// </summary>
        private async Task<IInputStream> getContent(string filename)
        {
            
            // Copy files from package installation folder to local storage folder ('LocalState' folder)
            await TransferFilesToStorage();
           
            try
            {

                var local = ApplicationData.Current.LocalFolder;
                StorageFile file = await local.GetFileAsync(filename);
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    return stream.GetInputStreamAt(0);
                
            }
            catch (Exception)
            {
                throw new Exception("Invalid path");
            }
        }

        private async Task TransferFilesToStorage()
        {

            try
            {
                StorageFile htmlfile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///www/content/test.html"));
                await htmlfile.CopyAsync(ApplicationData.Current.LocalFolder, "test.html", NameCollisionOption.ReplaceExisting);

                StorageFile jsfile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///www/content/demo_workers.js"));
                await jsfile.CopyAsync(ApplicationData.Current.LocalFolder, "demo_workers.js", NameCollisionOption.ReplaceExisting);

            }
            catch (Exception)
            {
                throw new Exception("Failure during transfer files to storage");
            }            
            
        }
    }
}
