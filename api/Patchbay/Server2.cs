using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

#nullable enable

namespace Patchbay {
    public class Server2 {

        private TcpListener _tcpListener;
        private UdpClient _udpSocket;
        
        public Server2(ILogger logger, int udpPort, int tcpPort) {

            if (udpPort == tcpPort) {
                logger.LogError("UDP and TCP can't operate on the same port");
                return; // TODO: Can I do this? Should I do this?
            }
            
            // Create listeners and bind to ports
            _udpSocket = new UdpClient(udpPort);
            _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), tcpPort);
            
            // Setup listeners
            _udpSocket.BeginReceive(new AsyncCallback(UdpReceiveCallback), null);
        }

        private void UdpReceiveCallback(IAsyncResult result) {
            if (!result.IsCompleted) {
                // ?
                return;
            }

            IPEndPoint? ipep = null;
            byte[] data = _udpSocket.EndReceive(result, ref ipep);
            
            
        }
    }
}