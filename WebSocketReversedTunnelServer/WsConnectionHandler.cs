using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebSocketReversedTunnelCommon;

namespace WebSocketReversedTunnelServer
{
    public class WsConnectionHandler
    {
        private readonly TcpListener _tcpListener;

        public WsConnectionHandler(RequestDelegate next)
        {
            _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 9999));
            _tcpListener.Start();
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
                using (var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync())
                {
                    using (var incomingConnection = _tcpListener.AcceptTcpClient())
                    using (var incomingConnectionNS = incomingConnection.GetStream())
                    {
                        var oneway =
                            CommunicationGenerator.FromNetworkStreamToWebSocket(incomingConnectionNS, webSocket);
                        var secondway = CommunicationGenerator.FromWebToNetworkStream(webSocket, incomingConnectionNS);
                        Task.WaitAll(oneway, secondway);
                        Console.WriteLine("Connection closed!");
                    }
                }
        }

        ~WsConnectionHandler()
        {
            _tcpListener.Stop();
        }
    }
}