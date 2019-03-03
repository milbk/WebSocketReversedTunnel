using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocketReversedTunnelCommon;

namespace WebSocketReversedTunnelClient
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // New ID = new TcpClient generated!
            using (var webSocketClient = new ClientWebSocket())
            {
                webSocketClient.ConnectAsync(
                    new Uri("ws://localhost:5000/ws-connection"),
                    CancellationToken.None
                ).Wait();

                var sshPusher = new TcpClient();
                sshPusher.Connect(new IPEndPoint(IPAddress.Loopback, 22));

                using (sshPusher)
                using (var sshPusherNS = sshPusher.GetStream())
                {
                    var oneway = CommunicationGenerator.FromNetworkStreamToWebSocket(sshPusherNS, webSocketClient);
                    var secondway = CommunicationGenerator.FromWebToNetworkStream(webSocketClient, sshPusherNS);

                    Task.WaitAll(oneway, secondway);
                }
            }
        }
    }
}