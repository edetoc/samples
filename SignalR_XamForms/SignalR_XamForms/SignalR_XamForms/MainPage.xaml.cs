using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

/*

backend : 
https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-2.2&tabs=visual-studio

client
https://github.com/aspnet/SignalR-samples/tree/master/WindowsUniversal

https://montemagno.com/real-time-communication-for-mobile-with-signalr/
     
 */


namespace SignalR_XamForms
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private ViewState _state = ViewState.Disconnected;

        private ObservableCollection<string> _messages = new ObservableCollection<string>();
        private HubConnection _connection;

        public MainPage()
        {
            InitializeComponent();

            ServerUrlEntry.Text = @"https://edetocsignalrchat.azurewebsites.net/chatHub";
            MessagesListView.ItemsSource = _messages;

        }

        private async void ConnectDisconnectButton_Clicked(object sender, EventArgs e)
        {
            if (_state == ViewState.Connected)
            {
                UpdateState(ViewState.Disconnecting);

                try
                {
                    await _connection.StopAsync();
                }
                catch (Exception ex)
                {
                    AppendMessage($"An error occurred while disconnecting: {ex}");
                    UpdateState(ViewState.Connected);
                    return;
                }

                UpdateState(ViewState.Disconnected);
            }
            else
            {
                UpdateState(ViewState.Connecting);

                try
                {
                    _connection = new HubConnectionBuilder()
                        .WithUrl(ServerUrlEntry.Text)
                        .Build();

                    //_connection.On<string>("Send", (message) =>
                    //{
                    //    AppendMessage(message);
                    //});


                    _connection.On<string, string>("ReceiveMessage", (user, message) =>
                    {
                        var s = $"{user} says {message}";
                        
                        AppendMessage(s);
                    });

                    await _connection.StartAsync();
                }
                catch (Exception ex)
                {
                    AppendMessage($"An error occurred while connecting: {ex}");
                    UpdateState(ViewState.Disconnected);
                    return;
                }

                UpdateState(ViewState.Connected);
            }
        }

        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            if (_state != ViewState.Connected)
            {
                await DisplayAlert("Error", "Must be Connected to Send!", "OK");
                return;
            }

            try
            {
               
                await _connection.InvokeAsync("SendMessage", UserEntry.Text, MessageEntry.Text);
                
                MessageEntry.Text = "";
            }
            catch (Exception ex)
            {
                AppendMessage($"An error occurred while sending: {ex}");
            }
        }

        private void AppendMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _messages.Add(message);
                MessagesListView.ScrollTo(_messages.Last(), ScrollToPosition.End, animated: true);
            });
        }

        private void UpdateState(ViewState state)
        {
            if (_state == state)
            {
                return;
            }

            switch (state)
            {
                case ViewState.Disconnected:
                    SendButton.IsEnabled = false;
                    MessageEntry.IsEnabled = false;
                    UserEntry.IsEnabled = false;
                    ServerUrlEntry.IsEnabled = true;                   
                    ConnectDisconnectButton.IsEnabled = true;
                    ConnectDisconnectButton.Text = "Connect";
                    break;
                case ViewState.Connecting:
                    SendButton.IsEnabled = false;
                    MessageEntry.IsEnabled = false;
                    UserEntry.IsEnabled = false;
                    ServerUrlEntry.IsEnabled = false;
                    ConnectDisconnectButton.IsEnabled = false;
                    ConnectDisconnectButton.Text = "Connecting...";
                    break;
                case ViewState.Disconnecting:
                    SendButton.IsEnabled = false;
                    MessageEntry.IsEnabled = false;
                    UserEntry.IsEnabled = false;
                    ServerUrlEntry.IsEnabled = false;
                    ConnectDisconnectButton.IsEnabled = false;
                    ConnectDisconnectButton.Text = "Disconnecting...";
                    break;
                case ViewState.Connected:
                    SendButton.IsEnabled = true;
                    MessageEntry.IsEnabled = true;
                    UserEntry.IsEnabled = true;
                    ServerUrlEntry.IsEnabled = false;
                    ConnectDisconnectButton.IsEnabled = true;
                    ConnectDisconnectButton.Text = "Disconnect";
                    break;
            }
            _state = state;
        }

        private enum ViewState
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting
        }
    }
}
