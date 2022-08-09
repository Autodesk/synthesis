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
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using SynthesisServer.Proto;

namespace SynthesisServer.Client {
    public class Client {
        private const int BUFFER_SIZE = 16384;

        public static event Action<string> ErrorReport;

        public bool IsTcpConnected => _isRunning && (_tcpSocket != null && _tcpSocket.Connected);

        private byte[] _serverBuffer;
        private IClientHandler _handler;
        private Socket _tcpSocket;

        private IPAddress _serverIP;
        private Socket _connectedServer;
        private List<Socket> _connectedClients; // might be dictionary and might have UdpClient (subject to change)

        private IUDPHandler _udpHandler;

        private int _tcpPort;
        private int _udpPort;

        private bool _isRunning = false;
        private bool _hasInit = false;

        private string _id;

        public byte[] SymmetricKey { get; private set; }
        private Task<AsymmetricCipherKeyPair> _keyPair;
        public AsymmetricCipherKeyPair KeyPair {
            get {
                if (!_hasInit)
                    throw new Exception("Must Init");

                if (!_keyPair.IsCompleted)
                    _keyPair.Wait();

                return _keyPair.Result;
            }
        }
        private Task<DHParameters> _parameters;
        public bool HasParameters => _hasInit && (_parameters != null && _parameters.IsCompleted);
        public DHParameters Parameters {
            get {
                if (!_hasInit)
                    throw new Exception("Must Init");

                if (!_parameters.IsCompleted)
                    _parameters.Wait();

                return _parameters.Result;
            }
        }
        private SymmetricEncryptor _encryptor;

        private static readonly Lazy<Client> lazy = new Lazy<Client>(() => new Client());
        public static Client Instance { get { return lazy.Value; } }
        private Client() {
        }

        // Host will initiate connection with clients

        public void Init(IClientHandler handler, DHParameters existingParameters = null) {
            _handler = handler;

            //_udpSocket = new UdpClient();
            ErrorReport("Socket created");
            // _udpSocket = new UdpClient(new IPEndPoint(IPAddress.Any, _udpPort));

            _encryptor = new SymmetricEncryptor();

            if (existingParameters != null) {
                _parameters = Task<DHParameters>.Factory.StartNew(() => existingParameters);
            } else {
                _parameters = Task<DHParameters>.Factory.StartNew(() => {
                    var res = _encryptor.GenerateParameters();
                    ErrorReport("Generated Parameters");
                    return res;
                });
            }
            _keyPair = Task<AsymmetricCipherKeyPair>.Factory.StartNew(() => {
                var res = _encryptor.GenerateKeys(Parameters);
                ErrorReport("Generated Keys");
                return res;
            });

            _serverBuffer = new byte[BUFFER_SIZE];

            _hasInit = true;
        }

        public bool Start(string serverIP, int tcpPort = 18001, int udpPort = 18000, long timeoutMS = 5000) {

            _tcpPort = tcpPort;
            _udpPort = udpPort;
            _serverIP = IPAddress.Parse(serverIP);
            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (_hasInit && TryConnectServer(_tcpSocket, _serverIP, _tcpPort, timeoutMS)) {
                _isRunning = true;
                if (TrySendKeyExchange()) {
                    _tcpSocket.BeginReceive(_serverBuffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(TCPReceiveCallback), null);
                    return true;
                } else {
                    // TODO: Fail
                    return false;
                }
            } else {
                System.Diagnostics.Debug.WriteLine("Failed to connect to server (Make sure you call Init() first)"); // Use logger instead
                return false;
            }
        }
        public void Stop(long timeout = 5000) {
            if (!TrySendDisconnectRequest())
                throw new Exception();

            try {
                // Gives the server a chance to disconnect the client on its end
                //long start = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
                //while (_tcpSocket.Connected && System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - start <= timeout) {
                //    try {
                //        var rec = _tcpSocket.Receive(new byte[32]);
                //        if (!_tcpSocket.Connected || rec == 0) {
                //            throw new Exception();
                //        }
                //    } catch (Exception) {
                //        break;
                //    }
                //}
                _tcpSocket.Close();
                _tcpSocket = null;
                _id = string.Empty;
                //if (_tcpSocket != null) {
                //    ErrorReport("Forcing disconnect");
                //    _tcpSocket.Disconnect(true);
                //} else {
                //    ErrorReport("Disconnected by remote");
                //}
                _isRunning = false;
                //if (_udpSocket != null)
                //    _udpSocket.Close();
            } catch (SocketException e) {
                ErrorReport($"{e.Message}:\n{e.StackTrace}");
            }
        }
        private bool TryConnectServer(Socket socket, IPAddress ip, int port, long timeoutMS)
        {
            ErrorReport($"TryConnect Called: {System.DateTime.Now}");
            long start = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (!_tcpSocket.Connected || System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - start >= timeoutMS)
            {
                try {
                    _tcpSocket.Connect(_serverIP, _tcpPort);
                }
                catch (SocketException e) {
                    if (ErrorReport != null)
                        ErrorReport(e.Message);
                }
            }
            ErrorReport("Connected To Server");
            return _tcpSocket.Connected;
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

                Action handleFunc = null;

                if (message.Is(KeyExchange.Descriptor)) {
                    handleFunc = () => {
                        var ke = message.Unpack<KeyExchange>();
                        this.SymmetricKey = _encryptor.GenerateSharedSecret(ke.PublicKey, Parameters, KeyPair);
                        _id = ke.ClientId;
                        ErrorReport($"Key Set\nKey Length {this.SymmetricKey.Length}\nClient ID: {_id}");
                    };
                }

                if (message.Is(StatusMessage.Descriptor)) { handleFunc = () => _handler.HandleStatusMessage(message.Unpack<StatusMessage>()); }
                else if (message.Is(ServerInfoResponse.Descriptor)) { handleFunc = () => _handler.HandleServerInfoResponse(message.Unpack<ServerInfoResponse>()); }
                else if (message.Is(CreateLobbyResponse.Descriptor)) { handleFunc = () => _handler.HandleCreateLobbyResponse(message.Unpack<CreateLobbyResponse>()); }
                else if (message.Is(DeleteLobbyResponse.Descriptor)) { handleFunc = () => _handler.HandleDeleteLobbyResponse(message.Unpack<DeleteLobbyResponse>()); }
                else if (message.Is(JoinLobbyResponse.Descriptor)) { handleFunc = () => _handler.HandleJoinLobbyResponse(message.Unpack<JoinLobbyResponse>()); }
                else if (message.Is(LeaveLobbyResponse.Descriptor)) { handleFunc = () => _handler.HandleLeaveLobbyResponse(message.Unpack<LeaveLobbyResponse>()); }
                else if (message.Is(StartLobbyResponse.Descriptor)) { handleFunc = () => _handler.HandleStartLobbyResponse(message.Unpack<StartLobbyResponse>()); }
                else if (message.Is(SwapResponse.Descriptor)) { handleFunc = () => _handler.HandleSwapResponse(message.Unpack<SwapResponse>()); }
                else if (message.Is(ChangeNameResponse.Descriptor)) { handleFunc = () => _handler.HandleChangeNameResponse(message.Unpack<ChangeNameResponse>()); }
                else if (message.Is(ConnectionDataClient.Descriptor)) { handleFunc = () => _handler.HandleConnectionDataClient(message.Unpack<ConnectionDataClient>()); }
                else if (message.Is(ConnectionDataHost.Descriptor)) { handleFunc = () => _handler.HandleConnectionDataHost(message.Unpack<ConnectionDataHost>()); }

                try {
                    handleFunc();
                } catch (Exception e) {
                    // ERROR
                }

                if (_isRunning)
                    _tcpSocket.BeginReceive(_serverBuffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(TCPReceiveCallback), null);
            }
            catch (SocketException) { }
        }
        private void TCPSendCallback(IAsyncResult asyncResult) { }

        // UDP Callbacks

        // Client Actions

        public void LockUntilKeyAvailable() {
            while (SymmetricKey == null || SymmetricKey.Length == 0 || _id == string.Empty)
                Thread.Sleep(50);
        }

        private bool TrySendKeyExchange()
        {
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                KeyExchange exchangeMsg = new KeyExchange()
                {
                    // ClientId = _id,
                    G = Parameters.G.ToString(),
                    P = Parameters.P.ToString(),
                    PublicKey = ((DHPublicKeyParameters)KeyPair.Public).Y.ToString()
                };
                //_expectedMessageType = KeyExchange.Descriptor;
                IO.SendMessage(exchangeMsg, _tcpSocket, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }
        public bool TrySendCreateLobby(string lobbyName) {
            LockUntilKeyAvailable();
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
        public bool TrySendChangeName(string name) {
            LockUntilKeyAvailable();
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                ChangeNameRequest request = new ChangeNameRequest()
                {
                    Name = name
                };
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }
        public bool TrySendDeleteLobby(string lobbyName) {
            LockUntilKeyAvailable();
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
        public bool TrySendJoinLobby(string lobbyName) {
            LockUntilKeyAvailable();
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
        public bool TrySendLeaveLobby(string lobbyName) {
            LockUntilKeyAvailable();
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
        public bool TrySendStartLobby(string lobbyName) {
            LockUntilKeyAvailable();
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
        public bool TrySendSwapPosition(string lobbyName, int firstPosition, int secondPosition) {
            LockUntilKeyAvailable();
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
        public bool TrySendGetServerData() {
            LockUntilKeyAvailable();
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
        public bool TrySendHeartbeat() {
            if (!HasParameters)
                return false;

            LockUntilKeyAvailable(); // Ensure keypair exists because that is sequential
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                Heartbeat request = new Heartbeat();
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }

        public bool TrySendDisconnectRequest()
        {
            LockUntilKeyAvailable();
            if (_hasInit && _isRunning && _tcpSocket.Connected)
            {
                DisconnectRequest request = new DisconnectRequest();
                IO.SendEncryptedMessage(request, _id, SymmetricKey, _tcpSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                return true;
            }
            return false;
        }

        private void TCPEndReceiveCallback() {  }
    }
}
