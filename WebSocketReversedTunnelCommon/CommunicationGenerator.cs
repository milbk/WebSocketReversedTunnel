using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketReversedTunnelCommon
{
    public class CommunicationGenerator
    {
        private const int TCP_STATUS_OFFSET = 1;
        private const int ID_OFFSET = 2;
        private static Dictionary<ushort, TcpClient> Connections = new Dictionary<ushort, TcpClient>();

        public static Task FromWebToNetworkStream(WebSocket webSocket, NetworkStream networkStream)
        {
            var buffer = new byte[1 << 14];
            return Task.Run(() =>
            {
                do
                {
                    // [1 byte - tcp status (0x00|0xFF) | 2 bytes - connection ID | x bytes - payload]
                    var webSocketReceiveStatus = webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer,
                                0,
                                buffer.Length),
                            CancellationToken.None)
                        .Result;

                    if (buffer[0] == 0xFF) break;

                    networkStream.Write(
                        buffer,
                        TCP_STATUS_OFFSET + ID_OFFSET,
                        webSocketReceiveStatus.Count - ID_OFFSET - TCP_STATUS_OFFSET);
                } while (true);
            });
        }

        public static Task FromNetworkStreamToWebSocket(NetworkStream networkStream, WebSocket webSocket)
        {
            var buffer = new byte[1 << 14];
            int bytesThroughTcp;
            return Task.Run(() =>
            {
                do
                {
                    // [1 byte - tcp status (0x00|0xFF) | 2 bytes - connection ID | x bytes - payload]
                    bytesThroughTcp = networkStream.Read(
                        buffer,
                        TCP_STATUS_OFFSET + ID_OFFSET,
                        buffer.Length - ID_OFFSET - TCP_STATUS_OFFSET);

                    if (bytesThroughTcp == 0) buffer[0] = 0xFF;

                    webSocket.SendAsync(
                        new ArraySegment<byte>(buffer,
                            0,
                            bytesThroughTcp + TCP_STATUS_OFFSET + ID_OFFSET),
                        WebSocketMessageType.Binary,
                        true,
                        CancellationToken.None);


                    if (buffer[0] == 0xFF) break;
                } while (true);
            });
        }
    }
}