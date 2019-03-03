using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace WebSocketReversedTunnelClient
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var webSocket = new ClientWebSocket();
            webSocket.ConnectAsync(new Uri(  "ws://localhost:5000/ws-connection"), CancellationToken.None);
            

            while (webSocket.State != WebSocketState.Closed)
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, 22));
                var ns = tcpClient.GetStream();

                var oneway = Task.Run((() =>
                {
                    byte[] bytes = new byte[1 << 14];
                    int amountOfBytesTransferred;
                    while (webSocket.State != WebSocketState.Closed &&
                           (amountOfBytesTransferred = ns.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, amountOfBytesTransferred),
                            WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                }));

                var secondway = Task.Run(() =>
                {
                    byte[] bytes = new byte[1 << 14];
                    while (webSocket.State != WebSocketState.Closed)
                    {
                        var webSocketReceiveResult = webSocket.ReceiveAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), CancellationToken.None)
                            .Result;
                        ns.Write(bytes, 0, webSocketReceiveResult.Count);
                    }
                });

                Task.WaitAny(oneway, secondway);
                tcpClient.Close();
            }
            
        }
    }
}