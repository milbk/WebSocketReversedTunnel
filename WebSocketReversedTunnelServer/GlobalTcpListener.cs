using System.Net;
using System.Net.Sockets;

namespace WebSocketReversedTunnelServer
{
    public class GlobalTcpListener
    {
        private static TcpListener _instance = null;

        private GlobalTcpListener()
        {
        }

        public static TcpListener Instance => _instance;

        public static void ChangeListener(IPEndPoint ipEndPoint)
        {
            _instance.Stop();
            _instance = new TcpListener(ipEndPoint);
            _instance.Start();
        }
    }
}