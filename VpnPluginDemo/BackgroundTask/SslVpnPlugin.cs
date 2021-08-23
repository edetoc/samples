using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Vpn;

namespace BackgroundTask
{
    public sealed class SslVpnPlugin : IVpnPlugIn
    {
        private StreamSocket socket = null;
        private string user = "admin";              // expected username for the VPN credentials

        public  void Connect(VpnChannel channel)
        {

            Debug.WriteLine("VPNDEMO: Entering plugin Connect");
                
            try
            {
              
                var prompt = new VpnCustomPromptTextInput
                {
                    Compulsory = true,
                    DisplayName = "Username",
                    Emphasized = false,
                    IsTextHidden = false,
                };

                var promptList = new[] { prompt };

                // Prompt user for credential info 
                _ = channel.RequestCustomPromptAsync(promptList);
                
                if (prompt.Text.Equals(user))                    
                {
                    Debug.WriteLine("VPNDEMO: user verified with success");

                    socket?.Dispose();
                    socket = new StreamSocket();

                    channel.AssociateTransport(this.socket, null);

                    Debug.WriteLine("VPNDEMO: calling ConnectAsync");
                    this.socket.ConnectAsync(new HostName("www.microsoft.com"), "443", SocketProtectionLevel.Tls12).AsTask().GetAwaiter().GetResult();

                    Debug.WriteLine("VPNDEMO: ConnectAsync returned");

                    var myRoute = new VpnRouteAssignment();
                    myRoute.Ipv4InclusionRoutes.Add(new VpnRoute(new HostName("192.168.80.0"), 24));
                    
                    var myAddress = new List<HostName>
                                        {
                                            new HostName("192.168.80.100")
                                        };

                    Debug.WriteLine("VPNDEMO: Calling channel.StartExistingTransports()");

                    channel.StartExistingTransports(myAddress,
                        null,
                        null,
                        myRoute,
                        new VpnDomainNameAssignment(),
                        1472,
                        1500,
                        false);

                    Debug.WriteLine("VPNDEMO: Connected");
                    
                }
                else
                {
                    Debug.WriteLine("VPNDEMO: user verification failed");
                    channel.TerminateConnection("Failed credentials");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("VPNDEMO: EXCEPTION in plugin connect: " + e.ToString());
                
                //throw;
            }

        }

        public void Disconnect(VpnChannel channel)
        {
            channel.Stop();
            this.socket?.Dispose();

            Debug.WriteLine("VPNDEMO: Disconnected");
        }

        public void GetKeepAlivePayload(VpnChannel channel, out VpnPacketBuffer keepAlivePacket)
        {
            keepAlivePacket = channel.GetVpnSendPacketBuffer();
        }

        public void Encapsulate(VpnChannel channel, VpnPacketBufferList packets, VpnPacketBufferList encapulatedPackets)
        {
            var context = channel.PlugInContext;
        }

        public void Decapsulate(VpnChannel channel, VpnPacketBuffer encapBuffer, VpnPacketBufferList decapsulatedPackets, VpnPacketBufferList controlPacketsToSend)
        {

        }
    }
}