using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SynthesisServer.Client
{
    class Client
    {
        private const int BUFFER_SIZE = 4096;

        private Socket _tcpSocket;
        private UdpClient _udpSocket;
        private SymmetricEncryptor _encryptor;

        private Socket _connectedServer;
        private List<Socket> _connectedClients; // might be dictionary and might have UdpClient (subject to change)

        private int _tcpPort;
        private int _udpPort;

        private static readonly Lazy<Client> lazy = new Lazy<Client>(() => new Client());
        public static Client Instance { get { return lazy.Value; } }
        private Client()
        {
        }

        // Host will initiate connection with clients

        public void Start(string serverIP, int tcpPort = 18001, int udpPort = 18000)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        private bool TryConnectServer(string ip, int timeoutMS)
        {
            throw new NotImplementedException();
        }

        private bool DisconnectServer()
        {
            throw new NotImplementedException();
        }

        private bool TryAddClient(IPEndPoint remoteEP)
        {
            throw new NotImplementedException();
        }

        private bool TryRemoveClient()
        {
            throw new NotImplementedException();
        }


    }
}
