using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebSocketReversedTunnelServer
{
    public class WsConnection
    {
        private readonly TcpListener _tcpListener;

        public WsConnection(RequestDelegate next)
        {
            _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 9999));
            _tcpListener.Start();
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                while (webSocket.State != WebSocketState.Closed)
                {
                    var listener = _tcpListener.AcceptTcpClient();
                    Task.Run(() =>
                    {
                        var ns = listener.GetStream();
                        Task oneway = Task.Run(() =>
                        {
                            byte[] bytes = new byte[1 << 14];
                            int amountOfBytesTransferred;
                            while (webSocket.State != WebSocketState.Closed
                                   && (amountOfBytesTransferred = ns.Read(bytes, 0, bytes.Length))!=0)
                            {
                                webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, amountOfBytesTransferred),
                                    WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
                            }
                        });
                        
                        Task secondway = Task.Run((() =>
                        {
                            byte[] bytes = new byte[1 << 14];
                            while (webSocket.State != WebSocketState.Closed)
                            {
                                var webSocketReceiveResult = webSocket.ReceiveAsync(
                                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                                    CancellationToken.None).Result;
                                ns.Write(bytes, 0, webSocketReceiveResult.Count);
                            }


                        }));
                        
                        Task.WaitAll(oneway, secondway);
                        listener.Close();
                    });
                }
            }
        }

        ~WsConnection()
        {
            _tcpListener.Stop();
        }
    }
}