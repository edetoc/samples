using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using NotificationsExtensions.Tiles;   // NotificationsExtensions.Win10
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace UpdatePrimaryTile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        string _header_legacy = "legacy";
        string _header_adaptive = "adaptive";
        string _header_adaptive_code = "adaptive with code";
        string _line1 = "line1";
        string _line2 = "line2";
        string _line3 = "line3";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void btnSendLegacy_Click(object sender, RoutedEventArgs e)
        {
            var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);

            //<tile>
            //    <visual version = "4">
            //        <binding template = "TileSquare150x150Text01" fallback = "TileSquareText01">
            //            <text id = "1"></text>
            //            <text id = "2"></text>
            //            <text id = "3"></text>
            //            <text id = "4"></text>
            //        </binding>
            //    </visual>
            //</tile>

            var tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = _header_legacy;
            tileTextAttributes[1].InnerText = _line1;
            tileTextAttributes[2].InnerText = _line2;
            tileTextAttributes[3].InnerText = _line3;

            TileNotification notification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

        private void btnSendAdaptive_Click(object sender, RoutedEventArgs e)
        {
            // Construct the tile content as a string
            string content = $@"
                                <tile>
                                    <visual>
 
                                        <binding template='TileMedium' displayName='A'>
                                            <text>{_header_adaptive}</text>
                                            <text hint-style='captionSubtle'>{_line1}</text>
                                            <text hint-style='captionSubtle'>{_line2}</text>
                                            <text hint-style='captionSubtle'>{_line3}</text>
                                        </binding>
 
                                    </visual>
                                </tile>";

            // Load the string into an XmlDocument
            var doc = new Windows.Data.Xml.Dom.XmlDocument();
            doc.LoadXml(content);

            // Then create the tile notification
            var notification = new TileNotification(doc);

            // And send the notification
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);


        }

        private void btnSendAdaptiveWithCode_Click(object sender, RoutedEventArgs e)
        {

            // install https://github.com/WindowsNotifications/NotificationsExtensions/wiki

            // Construct the tile content
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                                {
                                    new TileText()
                                    {
                                        Text = _header_adaptive_code,
                                        Wrap = true
                                    },

                                    new TileText()
                                    {
                                        Text = _line1,
                                        Style = TileTextStyle.CaptionSubtle
                                    },

                                    new TileText()
                                    {
                                        Text = _line2,
                                        Style = TileTextStyle.CaptionSubtle
                                    },

                                    new TileText()
                                    {
                                        Text = _line3,
                                        Style = TileTextStyle.CaptionSubtle
                                    }
                                }
                        }
                    }


                }
            };

            // Create the tile notification
            var notification = new TileNotification(content.GetXml());

            // And send the notification
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);

        }

        // Clear tile's notification
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();

        }
    }
}
