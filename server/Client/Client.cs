using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using SynthesisServer.Utilities;
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

        private IPAddress _serverIP;
        private Socket _connectedServer;
        private List<Socket> _connectedClients; // might be dictionary and might have UdpClient (subject to change)

        private int _tcpPort;
        private int _udpPort;

        private bool _isRunning = false;
        private bool _hasInit = false;

        private string _id;

        public byte[] SymmetricKey { get; private set; }
        private AsymmetricCipherKeyPair _keyPair;
        private DHParameters parameters;

        private static readonly Lazy<Client> lazy = new Lazy<Client>(() => new Client());
        public static Client Instance { get { return lazy.Value; } }
        private Client()
        {
        }

        // Host will initiate connection with clients

        public void Init(string serverIP, int tcpPort = 18001, int udpPort = 18000)
        {
            _tcpPort = tcpPort;
            _udpPort = udpPort;

            _serverIP = IPAddress.Parse(serverIP);

            _isRunning = true;

            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _udpSocket = new UdpClient(new IPEndPoint(IPAddress.Any, _udpPort));

            _encryptor = new SymmetricEncryptor();

            _hasInit = true;
        }

        public void Start(long timeoutMS = 5000)
        {
            if (_hasInit || TryConnectServer(_tcpSocket, _serverIP, _tcpPort, timeoutMS))
            {
                
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to connect to server (Make sure you call Init() first)"); // Use logger instead
            }
        }
        public void Stop()
        {
            throw new NotImplementedException();
        }
        private bool TryConnectServer(Socket socket, IPAddress ip, int port, long timeoutMS)
        {
            long start = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (!_tcpSocket.Connected || System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - start >= timeoutMS)
            {
                try
                {
                    _tcpSocket.Connect(_serverIP, _tcpPort);
                }
                catch (SocketException) {}
            }
            return _tcpSocket.Connected;
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

        // TCP Callbacks
        private void TCPReceiveCallback(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
        private void TCPSendCallback(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        // UDP Callbacks

        // Client Actions
        public bool TryCreateLobby(string lobbyName)
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                CreateLobbyRequest request = new CreateLobbyRequest()
                {
                    ClientId = _id,
                    LobbyName = lobbyName
                };
                IO.SendEncryptedMessage(request, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                
            }
            return false;
        }
        public bool TryDeleteLobby(string lobbyName)
        {
            throw new NotImplementedException(); //might need id
        }
        public bool TryJoinLobby(string lobbyName)
        {
            throw new NotImplementedException();
        }
        public bool TryLeaveLobby(string lobbyName)
        {
            throw new NotImplementedException();
        }
        public bool TryStartLobby(string lobbyName)
        {
            throw new NotImplementedException();
        }
        public bool TrySwapPosition(string lobbyName, int firstPosition, int secondPosition)
        {
            throw new NotImplementedException();
        }
        public bool TryGetServerData()
        {
            throw new NotImplementedException();
        }
    }
}
