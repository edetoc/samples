using WSA_PrintPDF_Sample.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;
using Windows.Storage.Pickers;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace WSA_PrintPDF_Sample
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MyMainPage
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();


        /// <summary>
        /// PrintDocument is used to prepare the pages for printing. 
        /// Prepare the pages to print in the handlers for the Paginate, GetPreviewPage, and AddPages events.
        /// </summary>
        private PrintDocument m_printDoc = null;

        /// <summary>
        /// Marker interface for document source
        /// </summary>
        private IPrintDocumentSource m_printDocumentSource = null;

        private List<Image> m_pages = null;

        private PdfDocument _pdfDocument; 
        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MyMainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

      

        // Register for Printing
        private void btnRegisterForPrinting_Click(object sender, RoutedEventArgs e)
        {
            // Create the PrintDocument.
            m_printDoc = new PrintDocument();

            // Save the DocumentSource.
            m_printDocumentSource = m_printDoc.DocumentSource;

            // Add an event handler which creates preview pages.
            m_printDoc.Paginate += CreatePrintPreviewPages;

            // Add an event handler which provides a specified preview page.
            m_printDoc.GetPreviewPage += GetPrintPreviewPage;

            // Add an event handler which provides all final print pages.
            m_printDoc.AddPages += printDocument_AddPages;

            // Create a PrintManager and add a handler for printing initialization.
            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested += printMan_PrintTaskRequested;

        }

        void printDocument_AddPages(object p_sender, AddPagesEventArgs e)
        {
            if (m_printDocumentSource == null)
            {
                return;
            }
            else
            {
                
                if (m_pages == null || m_pages.Count == 0)
                {
                     return;
                }
                
            }

            //LogHelper.LogActivityComplete("IMPRESSION : Envoi des pages à l'imprimante");
        
            foreach (UIElement l_page in m_pages)
            {
                //LogHelper.LogActivityComplete("IMPRESSION : Ajout d'une page");
                m_printDoc.AddPage(l_page);
            }
        
            PrintDocument printDoc = (PrintDocument)p_sender;
            printDoc.AddPagesComplete();

        }

        void printMan_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs p_args)
        {
            PrintTask l_printTask = null;
            l_printTask = p_args.Request.CreatePrintTask("EspaceTablette", p_sourceRequested =>
            {
                //LogHelper.LogActivityComplete("IMPRESSION : Tâche d'impression demandée");
                // Invoqué lorsque la tâche d'impression est terminée
                l_printTask.Completed += async (s, args) =>
                {
                    //LogHelper.LogActivityComplete("IMPRESSION : Tâche d'impression terminée");
                    if (args.Completion == PrintTaskCompletion.Failed)
                    {
                        // On informe l'utilisateur que l'impression a rencontrée une erreur
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            //LogHelper.LogActivityComplete("IMPRESSION : Erreur d'impression");
                            MessageDialog l_dialog = new MessageDialog("print error");
                            
                                //MessageDialogEx l_dialog = new MessageDialogEx(AppResources.GetResources("PrintErrorMessage"), AppResources.GetResources("PrintErrorTitle"));
                            await l_dialog.ShowAsync();
                        });
                    }
                };

                p_sourceRequested.SetSource(m_printDocumentSource);
            });
        }

        /// <summary>
        /// This is the event handler for PrintDocument.Paginate. It creates print preview pages for the app.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Paginate Event Arguments</param>
        private void CreatePrintPreviewPages(object p_sender, PaginateEventArgs e)
        {

            if (m_printDocumentSource == null)
            {
                return;
            }
            else
            {

                if (m_pages == null || m_pages.Count == 0)
                {
                    return;
                }

            }

            PrintDocument l_printDoc = (PrintDocument)p_sender;        
            l_printDoc.SetPreviewPageCount(m_pages.Count, PreviewPageCountType.Intermediate);

        }

        /// <summary>
        /// This is the event handler for PrintDocument.GetPrintPreviewPage. It provides a specific print preview page,
        /// in the form of an UIElement, to an instance of PrintDocument. PrintDocument subsequently converts the UIElement
        /// into a page that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Arguments containing the preview requested page</param>
        private void GetPrintPreviewPage(object p_sender, GetPreviewPageEventArgs p_args)
        {

            if (m_printDocumentSource == null)
            {
                return;
            }
            else
            {

                if (m_pages == null || m_pages.Count == 0)
                {
                    return;
                }

            }
           
            PrintDocument printDoc = (PrintDocument)p_sender;
            printDoc.SetPreviewPage(p_args.PageNumber, m_pages[p_args.PageNumber - 1]);

        }

         public static int ResolutionScale()
        {
            ResolutionScale l_scale = DisplayInformation.GetForCurrentView().ResolutionScale;
            int l_return = 0;
            switch (l_scale)
            {
                case Windows.Graphics.Display.ResolutionScale.Scale100Percent:
                    {
                        l_return = 100;
                        break;
                    }
                case Windows.Graphics.Display.ResolutionScale.Scale120Percent:
                    {
                        l_return = 120;
                        break;
                    }
                case Windows.Graphics.Display.ResolutionScale.Scale140Percent:
                    {
                        l_return = 140;
                        break;
                    }
                case Windows.Graphics.Display.ResolutionScale.Scale150Percent:
                    {
                        l_return = 150;
                        break;
                    }
                case Windows.Graphics.Display.ResolutionScale.Scale160Percent:
                    {
                        l_return = 160;
                        break;
                    }
                case Windows.Graphics.Display.ResolutionScale.Scale180Percent:
                    {
                        l_return = 180;
                        break;
                    }
                case Windows.Graphics.Display.ResolutionScale.Scale225Percent:
                    {
                        l_return = 225;
                        break;
                    }
                case Windows.Graphics.Display.ResolutionScale.Invalid:
                default:
                    {
                        //LogHelper.LogActivity("ResolutionScale.Invalid or Default");
                        l_return = 100;
                        break;
                    }
            }
            return l_return;
        }

         public void ResetDocumentSource()
         {
            // LogHelper.LogActivityDebug("EspaceTablette.Pages.WebViewPage.ResetDocumentSource | Entrée");

             if (m_pages != null)
                m_pages.Clear();
             else 
                 m_pages = new List<Image> ();
             //m_previewPages.Clear();

             //LogHelper.LogActivityDebug("EspaceTablette.Pages.WebViewPage.ResetDocumentSource | Sortie");
         }

        private async void LoadPdfFileAsync(StorageFile  selectedFile)
        {

       
            // LogHelper.LogActivityDebug("EspaceTablette.Pages.PdfViewer.LoadPdfFileAsync | Entrée");

            _pdfDocument = await PdfDocument.LoadFromFileAsync(selectedFile); ;           

            ObservableCollection<SampleDataItem> l_items = new ObservableCollection<SampleDataItem>();
            this.DefaultViewModel["Items"] = l_items;

            if (_pdfDocument != null && _pdfDocument.PageCount > 0)
            {
                for (int l_pageIndex = 0; l_pageIndex < _pdfDocument.PageCount; l_pageIndex++)
                {
                    try
                    {
                        PdfPage l_pdfPage = _pdfDocument.GetPage((uint)l_pageIndex);
                        if (l_pdfPage != null)
                        {
                            BitmapImage l_imgSrc = new BitmapImage();
                           
                            using (IRandomAccessStream l_randomStream = new InMemoryRandomAccessStream())
                            {
                                PdfPageRenderOptions l_pdfPageRenderOptions = new PdfPageRenderOptions();
                                l_pdfPageRenderOptions.DestinationWidth = (uint)((Window.Current.Bounds.Width - 130) / ResolutionScale() * 100);
                                l_pdfPageRenderOptions.IsIgnoringHighContrast = true;

                                await l_pdfPage.RenderToStreamAsync(l_randomStream, l_pdfPageRenderOptions);
                                await l_randomStream.FlushAsync();
                                await l_imgSrc.SetSourceAsync(l_randomStream);
                            }
                            l_pdfPage.Dispose();

                            l_items.Add(new SampleDataItem(
                                l_pageIndex.ToString(),
                                l_pageIndex.ToString(),
                                l_imgSrc));

                            Image l_printableImage = new Image();
                            l_printableImage.Stretch = Stretch.Uniform;
                            l_printableImage.Source = l_imgSrc;

                            m_pages.Add(l_printableImage);
                            //inPage.CurrentPrintType = WebViewPage.PrintType.PdfDocument;
                        }
                    }
                    catch (Exception ex)
                    {
                        
                        //if (App.Debug)
                        //{
                        //    LogHelper.LogActivityWithException(l_ex, "Exception");
                        //}
                    }
                }
            }
                 

        }

        private async void btnLoadPDF_Click(object sender, RoutedEventArgs e)
        {

            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".pdf");
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.SettingsIdentifier = "picker1";
            filePicker.CommitButtonText = "Open Pdf File";

            StorageFile file = await filePicker.PickSingleFileAsync();

            ResetDocumentSource();

            // load PDF file and convert into images
            LoadPdfFileAsync(file);
            
        }

     
        private async void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            bool success = true;
            try
            {
                await PrintManager.ShowPrintUIAsync();
            }
            catch (Exception ex)
            {
                success = false;
                
            }
            if (!success )
            {
                var dlg = new MessageDialog("Have you enabled the page for rendering ?");
                await dlg.ShowAsync();
            
            }
            
        }
    }

    public class SampleDataItem
    {
        public SampleDataItem(String p_uniqueId, String p_pageNumber, BitmapImage p_imageData)
        {
            this.UniqueId = p_uniqueId;
            this.PageNumber = p_pageNumber;
            this.ImageData = p_imageData;
        }

        public string Title { get; set; }

        public string UniqueId { get; private set; }

        public string PageNumber { get; private set; }

        public BitmapImage ImageData
        {
            get { return m_imageData; }
            private set
            {
                m_imageData = value;
            }
        }
        private BitmapImage m_imageData;

        public override string ToString()
        {
            return this.PageNumber;
        }
    }
}
