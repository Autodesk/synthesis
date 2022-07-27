using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using SynthesisServer.Proto;
using SynthesisServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using PLobby = SynthesisServer.Proto.Lobby;

namespace SynthesisServer {
    public sealed class Server {
        private const int LOBBY_SIZE = 6;
        private const int BUFFER_SIZE = 16384;

        private Socket _tcpSocket;
        private UdpClient _udpSocket;
        private SymmetricEncryptor _encryptor;

        private ReaderWriterLockSlim _clientsLock;
        private Dictionary<string, ClientData> _clients;

        private ReaderWriterLockSlim _lobbiesLock;
        private Dictionary<string, Lobby> _lobbies; // Uses lobby name as key

        private ILogger _logger;
        private bool _isRunning = false;

        private int _tcpPort;
        private int _udpPort;

        private static readonly Lazy<Server> lazy = new Lazy<Server>(() => new Server());
        public static Server Instance { get { return lazy.Value; } }
        private Server() {
        }

        private struct ClientState {
            public byte[] buffer;
            public Socket socket;
            public string id;
        }

        public void Stop() {
            _udpSocket.Close();
            _tcpSocket.Close();
            _clientsLock.EnterWriteLock();
            _clients.Clear();
            _clientsLock.ExitWriteLock();
            _lobbiesLock.EnterWriteLock();
            _lobbies.Clear();
            _lobbiesLock.ExitWriteLock();
        }

        public void Start(ILogger logger, int tcpPort = 18001, int udpPort = 18000) {

            _tcpPort = tcpPort;
            _udpPort = udpPort;

            _logger = logger;
            _isRunning = true;

            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _udpSocket = new UdpClient(new IPEndPoint(IPAddress.Any, _udpPort));

            _encryptor = new SymmetricEncryptor();

            _clients = new Dictionary<string, ClientData>();
            _clientsLock = new ReaderWriterLockSlim();

            _lobbies = new Dictionary<string, Lobby>();
            _lobbiesLock = new ReaderWriterLockSlim();

            _tcpSocket.Bind(new IPEndPoint(IPAddress.Any, _tcpPort));
            _tcpSocket.Listen(5);
            _tcpSocket.BeginAccept(new AsyncCallback(TCPAcceptCallback), null);

            _udpSocket.BeginReceive(new AsyncCallback(UDPReceiveCallback), null);

            _logger.LogInformation("Server is running");

            Task.Run(() => {
                while (_isRunning) {
                    CheckHeartbeats(5000);
                    Thread.Sleep(500);
                }
            });
        }

        // TCP Callbacks
        private void TCPAcceptCallback(IAsyncResult asyncResult) {
            _logger.LogInformation("New client accepted");

            ClientState state = new ClientState() {
                buffer = new byte[BUFFER_SIZE],
                socket = _tcpSocket.EndAccept(asyncResult),
                id = Guid.NewGuid().ToString()
            };

            _logger.LogInformation($"Client ID: '{state.id}'");

            if (_isRunning) {
                state.socket.BeginReceive(state.buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(TCPReceiveCallback), state);
                _tcpSocket.BeginAccept(new AsyncCallback(TCPAcceptCallback), null);
            }
        }
        /*
            Data is send in this format: [4 byte int denoting header length] + [MessageHeader object] + [4 byte int denoting message body length] + [The actual message]
        */
        private void TCPReceiveCallback(IAsyncResult asyncResult) {
            _logger.LogInformation("Received Message");
            try {
                ClientState state = (ClientState)asyncResult.AsyncState;
                int received = state.socket.EndReceive(asyncResult);

                byte[] data = new byte[received];
                Array.Copy(state.buffer, data, received);
                if (BitConverter.IsLittleEndian) { Array.Reverse(data); }

                MessageHeader header;
                Any message;

                try
                {
                    header = MessageHeader.Parser.ParseFrom(IO.GetNextMessage(ref data));
                    if (header.IsEncrypted)
                    {
                        byte[] decryptedData = _encryptor.Decrypt(data, _clients[header.ClientId].SymmetricKey);
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
                            Msg = "Invalid Message Received"
                        });
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning("Invalid Mesage Received");
                    _clientsLock.EnterReadLock();
                    header = new MessageHeader()
                    {
                        ClientId = state.id,
                        IsEncrypted = (_clients.ContainsKey(state.id) && _clients[state.id].SymmetricKey != null)
                    };
                    _clientsLock.ExitReadLock();
                    message = Any.Pack(new StatusMessage()
                    {
                        LogLevel = StatusMessage.Types.LogLevel.Error,
                        Msg = "Invalid Message Received"
                    });
                }

                if (message.Is(KeyExchange.Descriptor))
                {
                    HandleKeyExchange(message.Unpack<KeyExchange>(), state.id, state.socket);
                }
                else if (message.Is(Heartbeat.Descriptor))
                {
                    HandleHeartbeat(message.Unpack<Heartbeat>(), header.ClientId);
                }
                else if (message.Is(CreateLobbyRequest.Descriptor))
                {
                    HandleCreateLobbyRequest(message.Unpack<CreateLobbyRequest>(), header.ClientId);
                }
                else if (message.Is(DeleteLobbyRequest.Descriptor))
                {
                    HandleDeleteLobbyRequest(message.Unpack<DeleteLobbyRequest>(), header.ClientId);
                }
                else if (message.Is(JoinLobbyRequest.Descriptor))
                {
                    HandleJoinLobbyRequest(message.Unpack<JoinLobbyRequest>(), header.ClientId);
                }
                else if (message.Is(LeaveLobbyRequest.Descriptor))
                {
                    HandleLeaveLobbyRequest(message.Unpack<LeaveLobbyRequest>(), header.ClientId);
                }
                else if (message.Is(StartLobbyRequest.Descriptor))
                {
                    HandleStartLobbyRequest(message.Unpack<StartLobbyRequest>(), header.ClientId);
                }
                else if (message.Is(SwapRequest.Descriptor))
                {
                    HandleSwapRequest(message.Unpack<SwapRequest>(), header.ClientId);
                }
                else if (message.Is(ServerInfoRequest.Descriptor))
                {
                    HandleServerInfoRequest(message.Unpack<ServerInfoRequest>(), header.ClientId);
                }
                else if (message.Is(ChangeNameRequest.Descriptor))
                {
                    HandleChangeNameRequest(message.Unpack<ChangeNameRequest>(), header.ClientId);
                }
                else if (message.Is(DisconnectRequest.Descriptor))
                {
                    HandleDisconnectRequest(message.Unpack<DisconnectRequest>(), header.ClientId);
                }


                if (_isRunning) {
                    state.socket.BeginReceive(state.buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(TCPReceiveCallback), state);
                }
            } catch (Exception e) {
                _logger.LogError(e.ToString());
            }
        }
        private void TCPSendCallback(IAsyncResult asyncResult) {

        }


        // UDP Callbacks
        private void UDPReceiveCallback(IAsyncResult asyncResult) {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = _udpSocket.EndReceive(asyncResult, ref remoteEP);
            if (BitConverter.IsLittleEndian) { Array.Reverse(buffer); } // make sure to do while sending

            MessageHeader header = MessageHeader.Parser.ParseFrom(IO.GetNextMessage(ref buffer));

            // TODO: make clients accessible through id not socket (socket can just be a property of the client) (Still check to make sure correct socket is being used for client)

            Any message;

            _clientsLock.EnterReadLock();
            if (header.IsEncrypted && _clients.ContainsKey(header.ClientId)) {
                byte[] decryptedData = _encryptor.Decrypt(buffer, _clients[header.ClientId].SymmetricKey);
                message = Any.Parser.ParseFrom(IO.GetNextMessage(ref decryptedData));
            } else {
                message = Any.Pack(new StatusMessage() {
                    LogLevel = StatusMessage.Types.LogLevel.Error,
                    Msg = "Invalid Message"
                });
            }
            _clientsLock.ExitReadLock();

            if (message.Is(MatchStartResponse.Descriptor)) { HandleMatchStartResponse(message.Unpack<MatchStartResponse>(), header.ClientId, remoteEP); }
            if (_isRunning) {
                _udpSocket.BeginReceive(new AsyncCallback(UDPReceiveCallback), null);
            }

        }
        private void UDPSendCallback(IAsyncResult asyncResult) {

        }


        // UDP Handlers
        private void HandleMatchStartResponse(MatchStartResponse matchStartResponse, string clientID, IPEndPoint remoteEP) {
            _logger.LogInformation(clientID + " is starting a match");


            _clientsLock.EnterWriteLock();
            _clients[clientID].UDPEndPoint = remoteEP;
            _clients.Remove(clientID);
            _clientsLock.ExitWriteLock();
        }


        // TCP Handlers
        private void HandleKeyExchange(KeyExchange keyExchange, string clientID, Socket socket) {
            _logger.LogInformation(clientID + " is performing a key exchange");

            _clientsLock.EnterWriteLock();
            if (!_clients.ContainsKey(clientID)) { _clients.Add(clientID, new ClientData(socket, clientID)); }
            _clients[clientID].GenerateSharedSecret(keyExchange.PublicKey, new DHParameters(new BigInteger(keyExchange.P), new BigInteger(keyExchange.G)), _encryptor);
            _clientsLock.ExitWriteLock();

            _clientsLock.EnterReadLock();
            IO.SendMessage
            (
                new KeyExchange() {
                    ClientId = clientID,
                    PublicKey = _clients[clientID].GetPublicKey()
                },
                socket,
                new AsyncCallback(TCPSendCallback)
            );
            _clientsLock.ExitReadLock();
        }
        private void HandleHeartbeat(Heartbeat heartbeat, string clientID) {
            _logger.LogInformation(clientID + " has sent a heartbeat");
            _clientsLock.EnterWriteLock();
            _clients[clientID].UpdateHeartbeat();
            _clientsLock.ExitWriteLock();
        }
        private void HandleCreateLobbyRequest(CreateLobbyRequest createLobbyRequest, string clientID) {
            _logger.LogInformation(clientID + " has requested to create a lobby");
            _lobbiesLock.EnterUpgradeableReadLock();
            _clientsLock.EnterUpgradeableReadLock();

            if (_lobbies.ContainsKey(createLobbyRequest.LobbyName)) {
                IO.SendEncryptedMessage
                (
                    new CreateLobbyResponse() {
                        LobbyName = createLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = false,
                            LogMessage = "That lobby already exists"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            } else {
                _lobbiesLock.EnterWriteLock();
                _lobbies[createLobbyRequest.LobbyName] = new Lobby(_clients[clientID], LOBBY_SIZE, createLobbyRequest.LobbyName);
                if (_clients[clientID].CurrentLobby != string.Empty && _lobbies.ContainsKey(_clients[clientID].CurrentLobby))
                {
                    _lobbies[_clients[clientID].CurrentLobby].TryRemoveClient(_clients[clientID]);
                }
                _lobbiesLock.ExitWriteLock();

                _clientsLock.EnterWriteLock();
                _clients[clientID].CurrentLobby = createLobbyRequest.LobbyName;
                _clientsLock.ExitWriteLock();

                IO.SendEncryptedMessage
                (
                    new CreateLobbyResponse() {
                        LobbyName = createLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = true,
                            LogMessage = "Lobby created successfully"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            }


            _clientsLock.ExitUpgradeableReadLock();
            _lobbiesLock.ExitUpgradeableReadLock();
        }
        private void HandleDeleteLobbyRequest(DeleteLobbyRequest deleteLobbyRequest, string clientID) {
            _logger.LogInformation(clientID + " has requested to delete a lobby");
            _lobbiesLock.EnterUpgradeableReadLock();
            _clientsLock.EnterUpgradeableReadLock();

            if (_lobbies.ContainsKey(deleteLobbyRequest.LobbyName)) {
                if (_lobbies[deleteLobbyRequest.LobbyName].Host.Equals(_clients[clientID])) {
                    _lobbiesLock.EnterWriteLock();
                    _lobbies.Remove(deleteLobbyRequest.LobbyName);
                    _lobbiesLock.ExitWriteLock();

                    _clientsLock.EnterWriteLock();
                    _clients[clientID].CurrentLobby = string.Empty;
                    _clientsLock.ExitWriteLock();

                    IO.SendEncryptedMessage
                    (
                        new DeleteLobbyResponse() {
                            LobbyName = deleteLobbyRequest.LobbyName,
                            GenericResponse = new GenericResponse() {
                                Success = true,
                                LogMessage = "Lobby successfully deleted"
                            }
                        },
                        clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                        new AsyncCallback(TCPSendCallback)
                    );
                } else {
                    IO.SendEncryptedMessage
                    (
                        new DeleteLobbyResponse() {
                            LobbyName = deleteLobbyRequest.LobbyName,
                            GenericResponse = new GenericResponse() {
                                Success = false,
                                LogMessage = "You do not have permission to delete this lobby"
                            }
                        },
                        clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                        new AsyncCallback(TCPSendCallback)
                    );
                }
            } else {
                IO.SendEncryptedMessage
                (
                    new CreateLobbyResponse() {
                        LobbyName = deleteLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = false,
                            LogMessage = "Lobby does not exist"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            }

            _clientsLock.ExitUpgradeableReadLock();
            _lobbiesLock.ExitUpgradeableReadLock();
        }
        private void HandleJoinLobbyRequest(JoinLobbyRequest joinLobbyRequest, string clientID) {
            _logger.LogInformation(clientID + " has requested to join a lobby");
            _clientsLock.EnterUpgradeableReadLock();
            _lobbiesLock.EnterWriteLock();

            if (_lobbies.ContainsKey(joinLobbyRequest.LobbyName) && _clients[clientID].CurrentLobby == String.Empty && _lobbies[joinLobbyRequest.LobbyName].TryAddClient(_clients[clientID])) {
                _lobbiesLock.ExitWriteLock();

                _clientsLock.EnterWriteLock();
                _clients[clientID].CurrentLobby = joinLobbyRequest.LobbyName;
                _clientsLock.ExitWriteLock();

                IO.SendEncryptedMessage
                (
                    new CreateLobbyResponse() {
                        LobbyName = joinLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = true,
                            LogMessage = "Joined lobby successfully"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            } else {
                _lobbiesLock.ExitWriteLock();
                IO.SendEncryptedMessage
                (
                    new CreateLobbyResponse() {
                        LobbyName = joinLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = false,
                            LogMessage = "Could not join lobby"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            }

            _clientsLock.ExitUpgradeableReadLock();
        }
        private void HandleLeaveLobbyRequest(LeaveLobbyRequest leaveLobbyRequest, string clientID) {
            _logger.LogInformation(clientID + " has requested to leave a lobby");
            _clientsLock.EnterUpgradeableReadLock();
            _lobbiesLock.EnterWriteLock();

            if (_lobbies.ContainsKey(leaveLobbyRequest.LobbyName) && _lobbies[leaveLobbyRequest.LobbyName].TryRemoveClient(_clients[clientID])) {
                _lobbiesLock.ExitWriteLock();

                _clientsLock.EnterWriteLock();
                _clients[clientID].CurrentLobby = string.Empty;
                _clientsLock.ExitWriteLock();

                IO.SendEncryptedMessage
                (
                    new CreateLobbyResponse() {
                        LobbyName = leaveLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = true,
                            LogMessage = "Left lobby successfully"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            } else {
                _lobbiesLock.ExitWriteLock();
                IO.SendEncryptedMessage
                (
                    new CreateLobbyResponse() {
                        LobbyName = leaveLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = false,
                            LogMessage = "Could not leave lobby"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            }
            _clientsLock.ExitUpgradeableReadLock();
        }
        private void HandleStartLobbyRequest(StartLobbyRequest startLobbyRequest, string clientID) {
            _logger.LogInformation(clientID + " has requested to start a lobby");
            _lobbiesLock.EnterUpgradeableReadLock();
            if (_lobbies.ContainsKey(startLobbyRequest.LobbyName)) {
                _clientsLock.EnterReadLock();
                if (_lobbies[startLobbyRequest.LobbyName].Host.Equals(_clients[clientID])) {
                    _lobbiesLock.EnterWriteLock();
                    StartLobby(startLobbyRequest.LobbyName);
                    _lobbiesLock.ExitWriteLock();

                    IO.SendEncryptedMessage
                    (
                        new StartLobbyResponse() {
                            LobbyName = startLobbyRequest.LobbyName,
                            GenericResponse = new GenericResponse() {
                                Success = true,
                                LogMessage = "Lobby successfully started"
                            }
                        },
                        clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                        new AsyncCallback(TCPSendCallback)
                    );
                } else {
                    IO.SendEncryptedMessage
                    (
                        new StartLobbyResponse() {
                            LobbyName = startLobbyRequest.LobbyName,
                            GenericResponse = new GenericResponse() {
                                Success = false,
                                LogMessage = "You do not have permission to start this lobby"
                            }
                        },
                        clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                        new AsyncCallback(TCPSendCallback)
                    );
                }
            } else {
                IO.SendEncryptedMessage
                (
                    new StartLobbyResponse() {
                        LobbyName = startLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = false,
                            LogMessage = "Lobby does not exist"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            }
            _clientsLock.ExitReadLock();
            _lobbiesLock.ExitUpgradeableReadLock();
        }
        private void HandleSwapRequest(SwapRequest swapRequest, string clientID) {
            _logger.LogInformation(clientID + " has requested to swap positions in a lobby");
            _clientsLock.EnterReadLock();
            _lobbiesLock.EnterReadLock();

            if (_lobbies.ContainsKey(swapRequest.LobbyName) && (_clients[clientID].Equals(_lobbies[swapRequest.LobbyName].Host)) && _lobbies[swapRequest.LobbyName].Swap(swapRequest.FirstPostion, swapRequest.SecondPostion)) {
                IO.SendEncryptedMessage
                (
                    new SwapResponse() {
                        LobbyName = swapRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = true,
                            LogMessage = "Swap Successful"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            } else {
                IO.SendEncryptedMessage
                (
                    new SwapResponse() {
                        LobbyName = swapRequest.LobbyName,
                        GenericResponse = new GenericResponse() {
                            Success = false,
                            LogMessage = "Swap failed"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            }

            _lobbiesLock.ExitUpgradeableReadLock();
            _clientsLock.ExitReadLock();
        }

        private void HandleServerInfoRequest(ServerInfoRequest request, string clientID) {
            _logger.LogInformation(clientID + " has requested server info");

            _clientsLock.EnterReadLock();
            _lobbiesLock.EnterReadLock();

            if (_clients.ContainsKey(clientID)) {
                var response = new ServerInfoResponse();
                response.GenericResponse = new GenericResponse() { Success = true };
                response.CurrentName = _clients[clientID].Name;
                response.CurrentLobby = _clients[clientID].CurrentLobby;
                foreach (var l in _lobbies) {
                    response.Lobbies.Add(l.Value.ToProtobuf());
                }
                IO.SendEncryptedMessage(
                    response,
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );
            } else {
                _logger.LogError($"No client with ID '{clientID}'");
            }

            _lobbiesLock.ExitReadLock();
            _clientsLock.ExitReadLock();
        }

        private void HandleChangeNameRequest(ChangeNameRequest changeNameRequest, string clientID)
        {
            _logger.LogInformation(clientID + " has requested server info");

            _clientsLock.EnterUpgradeableReadLock();
            if (_clients.ContainsKey(clientID))
            {
                _clientsLock.EnterWriteLock();
                _clients[clientID].Name = changeNameRequest.Name;
                if (_clients[clientID].Name == null) { _clients[clientID].Name = string.Empty; }
                _clientsLock.ExitWriteLock();
                IO.SendEncryptedMessage(
                    new ChangeNameResponse() 
                    {
                        Name = _clients[clientID].Name,
                        GenericResponse = new GenericResponse()
                        {
                            Success = true,
                            LogMessage = "Successfully renamed client"
                        }
                    },
                    clientID,
                    _clients[clientID].SymmetricKey,
                    _clients[clientID].ClientSocket,
                    _encryptor,
                    new AsyncCallback(TCPSendCallback)
                );

            }
            else
            {
                _logger.LogError($"No client with ID '{clientID}'");
            }
            _clientsLock.ExitUpgradeableReadLock();
        }

        private void HandleDisconnectRequest(DisconnectRequest disconnectRequest, string clientID)
        {
            _logger.LogInformation(clientID + " has requested to disconnect");

            _clientsLock.EnterUpgradeableReadLock();
            if (_clients.ContainsKey(clientID))
            {
                _clientsLock.EnterWriteLock();
                if (!_clients[clientID].CurrentLobby.Equals(string.Empty))
                {
                    _lobbiesLock.EnterWriteLock();
                    _lobbies[_clients[clientID].CurrentLobby].TryRemoveClient(_clients[clientID]);
                    _lobbiesLock.ExitWriteLock();
                    _clients[clientID].CurrentLobby = string.Empty;
                }

                _clients[clientID].ClientSocket.Close();
                _clients.Remove(clientID);
                _clientsLock.ExitWriteLock();
            }
            _clientsLock.ExitUpgradeableReadLock();
        }



        private void StartLobby(string lobbyName) {
            _lobbiesLock.EnterWriteLock();
            _lobbies.Remove(lobbyName, out Lobby lobby);
            _lobbies[lobbyName].Start();
            _lobbiesLock.ExitWriteLock();

            MatchStart startMsg = new MatchStart() {
                UdpPort = _udpPort
            };

            _clientsLock.EnterReadLock();
            for (int i = 0; i < lobby.Clients.Count; i++) {
                IO.SendEncryptedMessage(startMsg, lobby.Clients[i].ID, lobby.Clients[i].SymmetricKey, lobby.Clients[i].ClientSocket, _encryptor, new AsyncCallback(TCPSendCallback));
            }

            long start = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - start <= 5000) {
                _clientsLock.EnterReadLock();
                bool allSet = true;
                foreach (ClientData x in lobby.Clients) {
                    if (x.UDPEndPoint == null) {
                        allSet = false;
                    }
                }
                if (allSet) {
                    _clientsLock.ExitReadLock();
                    break;
                }
                _clientsLock.ExitReadLock();
                Thread.Sleep(200);
            }

            _clientsLock.EnterReadLock();
            foreach (ClientData client in lobby.Clients) {
                if (client.UDPEndPoint == null) {
                    lobby.TryRemoveClient(client);
                }
            }
            _clientsLock.ExitReadLock();

            ConnectionDataHost hostMsg = new ConnectionDataHost();
            foreach (var client in lobby.Clients) {
                hostMsg.ClientEnpoints.Add(client.UDPEndPoint.ToString());
            }

            ConnectionDataClient clientMsg = new ConnectionDataClient() {
                HostEndpoint = lobby.Host.UDPEndPoint.ToString()
            };

            foreach (ClientData client in lobby.Clients) {
                if (lobby.Host.Equals(client)) {
                    IO.SendEncryptedMessage(hostMsg, client.ID, client.SymmetricKey, client.ClientSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                } else {
                    IO.SendEncryptedMessage(clientMsg, client.ID, client.SymmetricKey, client.ClientSocket, _encryptor, new AsyncCallback(TCPSendCallback));
                }
            }

        }

        public void CheckHeartbeats(long clientTimeout) {
            _clientsLock.EnterUpgradeableReadLock();
            foreach (string client in _clients.Keys) {
                if (System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - _clients[client].LastHeartbeat >= clientTimeout) {
                    _clientsLock.EnterWriteLock();
                    _clients.Remove(client);
                    _clientsLock.ExitWriteLock();
                }
            }
            _clientsLock.ExitUpgradeableReadLock();
        }
    }
}