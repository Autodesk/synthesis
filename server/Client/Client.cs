using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
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

        private byte[] _serverBuffer;
        private Socket _tcpSocket;
        private UdpClient _udpSocket;

        private IPAddress _serverIP;
        private Socket _connectedServer;
        private List<Socket> _connectedClients; // might be dictionary and might have UdpClient (subject to change)

        private int _tcpPort;
        private int _udpPort;

        private bool _isRunning = false;
        private bool _hasInit = false;

        private string _id;

        //private MessageDescriptor _expectedMessageType;

        public byte[] SymmetricKey { get; private set; }
        private AsymmetricCipherKeyPair _keyPair;
        private DHParameters _parameters;
        private SymmetricEncryptor _encryptor;

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
            _parameters = _encryptor.GenerateParameters();
            _keyPair = _encryptor.GenerateKeys(_parameters);

            _serverBuffer = new byte[BUFFER_SIZE];

            _hasInit = true;
        }

        public void Start(long timeoutMS = 5000)
        {
            if (_hasInit && TryConnectServer(_tcpSocket, _serverIP, _tcpPort, timeoutMS))
            {
                _isRunning = true;
                if (TrySendKeyExchange())
                {
                    _tcpSocket.BeginReceive(_serverBuffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(TCPReceiveCallback), null);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to connect to server (Make sure you call Init() first)"); // Use logger instead
            }
        }
        public void Stop()
        {
            _isRunning = false;
            try
            {
                _tcpSocket.Close();
                _udpSocket.Close();
            }
            catch (SocketException) { }
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
            if (_tcpSocket.Connected)
            {
                try
                {
                    _tcpSocket.Close();
                    return true;
                }
                catch (SocketException)
                {
                    return false;
                }
            }
            return false;
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
            try
            {
                int received = _tcpSocket.EndReceive(asyncResult);
                byte[] data = new byte[received];
                Array.Copy(_serverBuffer, data, received);
                if (BitConverter.IsLittleEndian) { Array.Reverse(data); }

                MessageHeader header = MessageHeader.Parser.ParseFrom(IO.GetNextMessage(ref data));
                Any message;
                if (header.IsEncrypted && SymmetricKey != null)
                {
                    byte[] decryptedData = _encryptor.Decrypt(data, SymmetricKey);
                    message = Any.Parser.ParseFrom(IO.GetNextMessage(ref decryptedData));
                }
                else if (!header.IsEncrypted)
                {
                    message = Any.Parser.ParseFrom(IO.GetNextMessage(ref data));
                }
                else
                {
                    message = Any.Pack(new StatusMessage()
                    {
                        LogLevel = StatusMessage.Types.LogLevel.Error,
                        Msg = "Invalid Message body"
                    });
                }

                if (message.Is(KeyExchange.Descriptor)) { HandleKeyExchange(message.Unpack<KeyExchange>()); }
                else if (message.Is(StatusMessage.Descriptor)) { HandleStatusMessage(message.Unpack<StatusMessage>()); }
                else if (message.Is(ServerInfoResponse.Descriptor)) { HandleServerInfoResponse(message.Unpack<ServerInfoResponse>()); }
                else if (message.Is(CreateLobbyResponse.Descriptor)) { HandleCreateLobbyResponse(message.Unpack<CreateLobbyResponse>()); }
                else if (message.Is(DeleteLobbyResponse.Descriptor)) { HandleDeleteLobbyResponse(message.Unpack<DeleteLobbyResponse>()); }
                else if (message.Is(JoinLobbyResponse.Descriptor)) { HandleJoinLobbyResponse(message.Unpack<JoinLobbyResponse>()); }
                else if (message.Is(LeaveLobbyResponse.Descriptor)) { HandleLeaveLobbyResponse(message.Unpack<LeaveLobbyResponse>()); }
                else if (message.Is(StartLobbyResponse.Descriptor)) { HandleStartLobbyResponse(message.Unpack<StartLobbyResponse>()); }
                else if (message.Is(SwapResponse.Descriptor)) { HandleSwapResponse(message.Unpack<SwapResponse>()); }

                _tcpSocket.BeginReceive(_serverBuffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(TCPReceiveCallback), null);
            }
            catch (SocketException) { }
        }
        private void TCPSendCallback(IAsyncResult asyncResult)
        {
        }


        // UDP Callbacks

        // Client Actions
        private bool TrySendKeyExchange()
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                KeyExchange exchangeMsg = new KeyExchange()
                {
                    ClientId = _id,
                    G = _parameters.G.ToString(),
                    P = _parameters.P.ToString(),
                    PublicKey = ((DHPublicKeyParameters)_keyPair.Public).Y.ToString()
                };
                //_expectedMessageType = KeyExchange.Descriptor;
                IO.SendMessage(exchangeMsg, _tcpSocket, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }
        public bool TrySendCreateLobby(string lobbyName)
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                CreateLobbyRequest request = new CreateLobbyRequest()
                {
                    LobbyName = lobbyName
                };
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }
        public bool TrySendDeleteLobby(string lobbyName)
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                DeleteLobbyRequest request = new DeleteLobbyRequest()
                {
                    LobbyName = lobbyName
                };
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }
        public bool TrySendJoinLobby(string lobbyName)
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                JoinLobbyRequest request = new JoinLobbyRequest()
                {
                    LobbyName = lobbyName
                };
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }
        public bool TrySendLeaveLobby(string lobbyName)
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                LeaveLobbyRequest request = new LeaveLobbyRequest()
                {
                    LobbyName = lobbyName
                };
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }
        public bool TrySendStartLobby(string lobbyName)
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                StartLobbyRequest request = new StartLobbyRequest()
                {
                    LobbyName = lobbyName
                };
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }
        public bool TrySendSwapPosition(string lobbyName, int firstPosition, int secondPosition)
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                SwapRequest request = new SwapRequest()
                {
                    LobbyName = lobbyName,
                    FirstPostion = firstPosition,
                    SecondPostion = secondPosition
                };
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }
        public bool TrySendGetServerData()
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                ServerInfoRequest request = new ServerInfoRequest()
                {
                };
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }

        // Server Response Handlers
        private void HandleKeyExchange(KeyExchange exchangeMessage)
        {
            throw new NotImplementedException();
        }
        private void HandleStatusMessage(StatusMessage statusMessage)
        {
            throw new NotImplementedException();
        }
        private void HandleServerInfoResponse(ServerInfoResponse serverInfoResponse)
        {
            throw new NotImplementedException();
        }
        private void HandleCreateLobbyResponse(CreateLobbyResponse createLobbyResponse)
        {
            throw new NotImplementedException();
        }
        private void HandleDeleteLobbyResponse(DeleteLobbyResponse deleteLobbyResponse)
        {
            throw new NotImplementedException();
        }
        private void HandleJoinLobbyResponse(JoinLobbyResponse joinLobbyResponse)
        {
            throw new NotImplementedException();
        }
        private void HandleLeaveLobbyResponse(LeaveLobbyResponse leaveLobbyResponse)
        {
            throw new NotImplementedException();
        }
        private void HandleStartLobbyResponse(StartLobbyResponse startLobbyResponse)
        {
            throw new NotImplementedException();
        }
        private void HandleSwapResponse(SwapResponse swapResponse)
        {
            throw new NotImplementedException();
        }
    }
}
