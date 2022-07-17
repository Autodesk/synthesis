using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynthesisServer
{
    public sealed class Server
    {
        private UdpClient _udpSocket;
        private SymmetricEncryptor _encryptor;

        private ReaderWriterLockSlim _clientsLock;
        private Dictionary<string, ClientData> _clients; // Uses IPEndpoint.ToString() as key

        private ReaderWriterLockSlim _lobbiesLock;
        private Dictionary<string, Lobby> _lobbies; // Uses lobby name as key

        public int Port { get; set; } = 10800;
        private static readonly Lazy<Server> lazy = new Lazy<Server>(() => new Server());
        public static Server Instance { get { return lazy.Value; } }
        private Server() 
        {
        }

        public void Stop()
        {
            _udpSocket.Close();
            _clientsLock.EnterWriteLock();
            _clients.Clear();
            _clientsLock.ExitWriteLock();
            _lobbiesLock.EnterWriteLock();
            _lobbies.Clear();
            _lobbiesLock.ExitWriteLock();
        }

        public void Start()
        {
            _udpSocket = new UdpClient(Port, AddressFamily.InterNetwork);
            _encryptor = new SymmetricEncryptor();

            _clients = new Dictionary<string, ClientData>();
            _clientsLock = new ReaderWriterLockSlim();

            _lobbies = new Dictionary<string, Lobby>();
            _lobbiesLock = new ReaderWriterLockSlim();

            _udpSocket.BeginReceive(RecieveCallback, null);
        }
        /*
            Data is send in this format: [4 byte int that denotes the message type] + [protobuf data (if is a Message it is encrypted and the iv is prepended]
        */
        private void RecieveCallback(IAsyncResult asyncResult)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = _udpSocket.EndReceive(asyncResult, ref remoteEP);
                if (BitConverter.IsLittleEndian) { Array.Reverse(buffer); } // make sure to do while sending

                
                MessageHeader header = MessageHeader.Parser.ParseFrom(GetNextMessage(ref buffer));
                Any message = ParseMessage(header, buffer, remoteEP);
                
                if (message.Is(KeyExchange.Descriptor)) { HandleKeyExchange(message.Unpack<KeyExchange>(), remoteEP); } 
                else if (message.Is(Heartbeat.Descriptor)) { HandleHeartbeat(message.Unpack<Heartbeat>(), remoteEP); }
                else if (message.Is(CreateLobbyRequest.Descriptor)) { HandleCreateLobbyRequest(message.Unpack<CreateLobbyRequest>(), remoteEP); }
                else if (message.Is(DeleteLobbyRequest.Descriptor)) { HandleDeleteLobbyRequest(message.Unpack<DeleteLobbyRequest>(), remoteEP); }
                else if (message.Is(JoinLobbyRequest.Descriptor)) { HandleJoinLobbyRequest(message.Unpack<JoinLobbyRequest>(), remoteEP); }
                else if (message.Is(LeaveLobbyRequest.Descriptor)) { HandleLeaveLobbyRequest(message.Unpack<LeaveLobbyRequest>(), remoteEP); }
                else if (message.Is(StartLobbyRequest.Descriptor)) { HandleStartLobbyRequest(message.Unpack<StartLobbyRequest>(), remoteEP); }
                else if (message.Is(SwapRequest.Descriptor)) { HandleSwapRequest(message.Unpack<SwapRequest>(), remoteEP); }
                else if (message.Is(MatchStartResponse.Descriptor)) { HandleMatchStartResponse(message.Unpack<MatchStartResponse>(), remoteEP); }

                _udpSocket.BeginReceive(RecieveCallback, Port);
                
            } catch (SocketException e)
            {
                Console.WriteLine("Socket Exception");
            }
        }

        private void HandleMatchStartResponse(MatchStartResponse matchStartResponse, IPEndPoint remoteEP)
        {
            _clientsLock.EnterWriteLock();
            _clients.Remove(remoteEP.ToString());
            _clientsLock.ExitWriteLock();
        }

        private void HandleKeyExchange(KeyExchange keyExchange, IPEndPoint remoteEP)
        {
            ClientData EstablishedClientData = null;

            _clientsLock.EnterUpgradeableReadLock();
            if (_clients.ContainsKey(remoteEP.ToString()))
            {
                EstablishedClientData = _clients[remoteEP.ToString()];
            }
            else
            {
                _clientsLock.EnterWriteLock();
                EstablishedClientData = new ClientData(keyExchange.PublicKey, new DHParameters(new BigInteger(keyExchange.P), new BigInteger(keyExchange.G)))
                {
                    ClientID = Guid.NewGuid().ToString(),
                    ClientEndpoint = remoteEP
                };
                _clients.Add(remoteEP.ToString(), EstablishedClientData);
                _clientsLock.ExitWriteLock();
            }
            _clientsLock.ExitUpgradeableReadLock();

            SendMessage
            (
                new KeyExchange()
                {
                    ClientId = EstablishedClientData.ClientID,
                    PublicKey = EstablishedClientData.GetPublicKey()
                },
                remoteEP,
                _udpSocket
            );
        }

        private void HandleHeartbeat(Heartbeat heartbeat, IPEndPoint remoteEP)
        {
            _clientsLock.EnterWriteLock();
            _clients[remoteEP.ToString()].UpdateHeartbeat();
            _clientsLock.ExitWriteLock();
        }

        private void HandleCreateLobbyRequest(CreateLobbyRequest createLobbyRequest, IPEndPoint remoteEP)
        {
            _lobbiesLock.EnterUpgradeableReadLock();
            _clientsLock.EnterReadLock();

            if (_lobbies.ContainsKey(createLobbyRequest.LobbyName))
            {
                SendEncryptedMessage
                (
                    new CreateLobbyResponse()
                    {
                        ClientId = _clients[remoteEP.ToString()].ClientID,
                        LobbyName = createLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse()
                        {
                            Success = false,
                            LogMessage = "That lobby already exists"
                        }
                    },
                    remoteEP,
                    _clients[remoteEP.ToString()],
                    _udpSocket
                );

                _lobbiesLock.ExitUpgradeableReadLock();
                _clientsLock.ExitReadLock();
                return;
            }

            _lobbiesLock.EnterWriteLock();
            _lobbies[createLobbyRequest.LobbyName] = new Lobby(_clients[remoteEP.ToString()]);
            _lobbiesLock.ExitWriteLock();

            SendEncryptedMessage
            (
                new CreateLobbyResponse()
                {
                    ClientId = _clients[remoteEP.ToString()].ClientID,
                    LobbyName = createLobbyRequest.LobbyName,
                    GenericResponse = new GenericResponse()
                    {
                        Success = true,
                        LogMessage = "Lobby created successfully"
                    }
                },
                remoteEP,
                _clients[remoteEP.ToString()],
                _udpSocket
            );

            _clientsLock.ExitReadLock();
            _lobbiesLock.ExitUpgradeableReadLock();
        }

        private void HandleDeleteLobbyRequest(DeleteLobbyRequest deleteLobbyRequest, IPEndPoint remoteEP)
        {
            _lobbiesLock.EnterUpgradeableReadLock();
            _clientsLock.EnterReadLock();

            if (_lobbies.ContainsKey(deleteLobbyRequest.LobbyName))
            {
                if (_lobbies[deleteLobbyRequest.LobbyName].Host.Equals(_clients[remoteEP.ToString()]))
                {
                    _lobbiesLock.EnterWriteLock();
                    _lobbies.Remove(deleteLobbyRequest.LobbyName);
                    _lobbiesLock.ExitWriteLock();

                    SendEncryptedMessage
                    (
                        new DeleteLobbyResponse()
                        {
                            ClientId = _clients[remoteEP.ToString()].ClientID,
                            LobbyName = deleteLobbyRequest.LobbyName,
                            GenericResponse = new GenericResponse()
                            {
                                Success = true,
                                LogMessage = "Lobby successfully deleted"
                            }
                        },
                        remoteEP,
                        _clients[remoteEP.ToString()],
                        _udpSocket
                    );
                }
                else
                {
                    SendEncryptedMessage
                    (
                        new DeleteLobbyResponse()
                        {
                            ClientId = _clients[remoteEP.ToString()].ClientID,
                            LobbyName = deleteLobbyRequest.LobbyName,
                            GenericResponse = new GenericResponse()
                            {
                                Success = false,
                                LogMessage = "You do not have permission to delete this lobby"
                            }
                        },
                        remoteEP,
                        _clients[remoteEP.ToString()],
                        _udpSocket
                    );
                }
            }
            else
            {
                SendEncryptedMessage
                (
                    new CreateLobbyResponse()
                    {
                        ClientId = _clients[remoteEP.ToString()].ClientID,
                        LobbyName = deleteLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse()
                        {
                            Success = false,
                            LogMessage = "Lobby does not exist"
                        }
                    },
                    remoteEP,
                    _clients[remoteEP.ToString()],
                    _udpSocket
                );
            }

            _clientsLock.ExitReadLock();
            _lobbiesLock.ExitUpgradeableReadLock();
        }

        private void HandleJoinLobbyRequest(JoinLobbyRequest joinLobbyRequest, IPEndPoint remoteEP)
        {
            _clientsLock.EnterReadLock();
            _lobbiesLock.EnterWriteLock();

            if (_lobbies.ContainsKey(joinLobbyRequest.LobbyName) && _lobbies[joinLobbyRequest.LobbyName].TryAddClient(_clients[remoteEP.ToString()]))
            {
                _lobbiesLock.ExitWriteLock();
                SendEncryptedMessage
                (
                    new CreateLobbyResponse()
                    {
                        ClientId = _clients[remoteEP.ToString()].ClientID,
                        LobbyName = joinLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse()
                        {
                            Success = true,
                            LogMessage = "Joined lobby successfully"
                        }
                    },
                    remoteEP,
                    _clients[remoteEP.ToString()],
                    _udpSocket
                );
            }
            else
            {
                _lobbiesLock.ExitWriteLock();
                SendEncryptedMessage
                (
                    new CreateLobbyResponse()
                    {
                        ClientId = _clients[remoteEP.ToString()].ClientID,
                        LobbyName = joinLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse()
                        {
                            Success = false,
                            LogMessage = "Could not join lobby"
                        }
                    },
                    remoteEP,
                    _clients[remoteEP.ToString()],
                    _udpSocket
                );
            }

            _clientsLock.ExitReadLock();
        }

        private void HandleLeaveLobbyRequest(LeaveLobbyRequest leaveLobbyRequest, IPEndPoint remoteEP)
        {
            _clientsLock.EnterReadLock();
            _lobbiesLock.EnterWriteLock();
            if (_lobbies.ContainsKey(leaveLobbyRequest.LobbyName) && _lobbies[leaveLobbyRequest.LobbyName].TryRemoveClient(_clients[remoteEP.ToString()]))
            {
                _lobbiesLock.ExitWriteLock();
                SendEncryptedMessage
                (
                    new CreateLobbyResponse()
                    {
                        ClientId = _clients[remoteEP.ToString()].ClientID,
                        LobbyName = leaveLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse()
                        {
                            Success = true,
                            LogMessage = "Left lobby successfully"
                        }
                    },
                    remoteEP,
                    _clients[remoteEP.ToString()],
                    _udpSocket
                );
            }
            else
            {
                _lobbiesLock.ExitWriteLock();
                SendEncryptedMessage
                (
                    new CreateLobbyResponse()
                    {
                        ClientId = _clients[remoteEP.ToString()].ClientID,
                        LobbyName = leaveLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse()
                        {
                            Success = false,
                            LogMessage = "Could not leave lobby"
                        }
                    },
                    remoteEP,
                    _clients[remoteEP.ToString()],
                    _udpSocket
                );
            }
            _clientsLock.ExitReadLock();
        }

        private void HandleStartLobbyRequest(StartLobbyRequest startLobbyRequest, IPEndPoint remoteEP)
        {
            _lobbiesLock.EnterUpgradeableReadLock();
            if (_lobbies.ContainsKey(startLobbyRequest.LobbyName))
            {
                _clientsLock.EnterReadLock();
                if (_lobbies[startLobbyRequest.LobbyName].Host.Equals(_clients[remoteEP.ToString()]))
                {
                    _lobbiesLock.EnterWriteLock();
                    StartLobby(startLobbyRequest.LobbyName);
                    _lobbiesLock.ExitWriteLock();

                    SendEncryptedMessage
                    (
                        new StartLobbyResponse()
                        {
                            ClientId = _clients[remoteEP.ToString()].ClientID,
                            LobbyName = startLobbyRequest.LobbyName,
                            GenericResponse = new GenericResponse()
                            {
                                Success = true,
                                LogMessage = "Lobby successfully started"
                            }
                        },
                        remoteEP,
                        _clients[remoteEP.ToString()],
                        _udpSocket
                    );
                }
                else
                {
                    SendEncryptedMessage
                    (
                        new StartLobbyResponse()
                        {
                            ClientId = _clients[remoteEP.ToString()].ClientID,
                            LobbyName = startLobbyRequest.LobbyName,
                            GenericResponse = new GenericResponse()
                            {
                                Success = false,
                                LogMessage = "You do not have permission to start this lobby"
                            }
                        },
                        remoteEP,
                        _clients[remoteEP.ToString()],
                        _udpSocket
                    );
                }
            }
            else
            {
                SendEncryptedMessage
                (
                    new StartLobbyResponse()
                    {
                        ClientId = _clients[remoteEP.ToString()].ClientID,
                        LobbyName = startLobbyRequest.LobbyName,
                        GenericResponse = new GenericResponse()
                        {
                            Success = false,
                            LogMessage = "Lobby does not exist"
                        }
                    },
                    remoteEP,
                    _clients[remoteEP.ToString()],
                    _udpSocket
                );
            }
            _clientsLock.ExitReadLock();
            _lobbiesLock.ExitUpgradeableReadLock();
        }

        private void HandleSwapRequest(SwapRequest swapRequest, IPEndPoint remoteEP)
        {
            _clientsLock.EnterReadLock();
            _lobbiesLock.EnterReadLock();

            if (_lobbies.ContainsKey(swapRequest.LobbyName) && (_clients[remoteEP.ToString()].Equals(_lobbies[swapRequest.LobbyName].Host)) && _lobbies[swapRequest.LobbyName].Swap(swapRequest.FirstPostion, swapRequest.SecondPostion))
            {
                SendEncryptedMessage
                (
                    new SwapResponse()
                    {
                        ClientId = _clients[remoteEP.ToString()].ClientID,
                        LobbyName = swapRequest.LobbyName,
                        GenericResponse = new GenericResponse()
                        {
                            Success = true,
                            LogMessage = "Swap Successful"
                        }
                    },
                    remoteEP,
                    _clients[remoteEP.ToString()],
                    _udpSocket
                );
            }
            else
            {
                SendEncryptedMessage
                (
                    new SwapResponse()
                    {
                        ClientId = _clients[remoteEP.ToString()].ClientID,
                        LobbyName = swapRequest.LobbyName,
                        GenericResponse = new GenericResponse()
                        {
                            Success = false,
                            LogMessage = "Swap failed"
                        }
                    },
                    remoteEP,
                    _clients[remoteEP.ToString()],
                    _udpSocket
                );
            }

            _lobbiesLock.ExitUpgradeableReadLock();
            _clientsLock.ExitReadLock();
        }

        private void SendCallback(IAsyncResult asyncResult)
        {

        }

        private byte[] GetNextMessage(ref byte[] buffer) 
        {
            byte[] msgLength = new byte[sizeof(int)];
            Array.Copy(buffer, 0, msgLength, 0, msgLength.Length);

            byte[] msg = new byte[BitConverter.ToInt32(msgLength)];
            Array.Copy(buffer, sizeof(int), msg, 0, msg.Length);

            buffer = buffer.Skip(msgLength.Length + msg.Length).ToArray();
            return msg;
        }

        // Handles packing the message as 'Any'
        private void SendMessage(IMessage msg, IPEndPoint remoteEndPoint, UdpClient server)
        {
            Any packedMsg = Any.Pack(msg);
            byte[] msgBytes = new byte[packedMsg.CalculateSize()];
            packedMsg.WriteTo(msgBytes);

            MessageHeader header = new MessageHeader() { IsEncrypted = false };
            byte[] headerBytes = new byte[header.CalculateSize()];
            header.WriteTo(headerBytes);

            byte[] data = new byte[sizeof(int) + headerBytes.Length + sizeof(int) + msgBytes.Length];

            BitConverter.GetBytes(headerBytes.Length).CopyTo(data, 0);
            headerBytes.CopyTo(data, sizeof(int));

            BitConverter.GetBytes(msgBytes.Length).CopyTo(data, sizeof(int) + headerBytes.Length);
            msgBytes.CopyTo(data, sizeof(int) + headerBytes.Length + sizeof(int));

            if (BitConverter.IsLittleEndian) { Array.Reverse(data); }
            server.BeginSend(data, data.Length, remoteEndPoint, SendCallback, null);
        }

        private void SendEncryptedMessage(IMessage msg, IPEndPoint remoteEndPoint, ClientData client, UdpClient server)
        {
            Any packedMsg = Any.Pack(msg);
            byte[] msgBytes = new byte[packedMsg.CalculateSize()];
            packedMsg.WriteTo(msgBytes);

            byte[] delimitedMessage = new byte[sizeof(int) + msgBytes.Length];
            BitConverter.GetBytes(msgBytes.Length).CopyTo(delimitedMessage, 0);
            msgBytes.CopyTo(delimitedMessage, sizeof(int));

            byte[] encryptedMessage = _encryptor.Encrypt(delimitedMessage, client.SymmetricKey);

            MessageHeader header = new MessageHeader() { IsEncrypted = true };
            byte[] headerBytes = new byte[header.CalculateSize()];
            header.WriteTo(headerBytes);

            byte[] data = new byte[sizeof(int) + headerBytes.Length + encryptedMessage.Length];

            BitConverter.GetBytes(headerBytes.Length).CopyTo(data, 0);
            headerBytes.CopyTo(data, sizeof(int));

            encryptedMessage.CopyTo(data, sizeof(int) + headerBytes.Length);

            if (BitConverter.IsLittleEndian) { Array.Reverse(data); }
            server.BeginSend(data, data.Length, remoteEndPoint, SendCallback, null);
        }

        private Any ParseMessage(MessageHeader header, byte[] delimitedMessageBody, IPEndPoint remoteEndPoint)
        {
            if (!header.IsEncrypted)
            {
                return Any.Parser.ParseFrom(GetNextMessage(ref delimitedMessageBody));
            }
            else if (header.IsEncrypted)
            {
                return Any.Parser.ParseFrom(delimitedMessageBody);
            }
            return Any.Pack(new StatusMessage()
            {
                LogLevel = StatusMessage.Types.LogLevel.Error,
                Msg = "Invalid Message body"
            });
        }

        private void StartLobby(string lobbyName)
        {
            _lobbiesLock.EnterWriteLock();
            _lobbies.Remove(lobbyName, out Lobby lobby);
            _lobbiesLock.ExitWriteLock();

            _clientsLock.EnterReadLock();
            MatchStart matchStartMessage = new MatchStart()
            {
                HostEndpoint = lobby.Host.ClientEndpoint.ToString()
            };
            foreach (ClientData client in lobby.Clients)
            {
                matchStartMessage.ClientEndpoints.Add(client.ClientEndpoint.ToString());
            }

            foreach (ClientData client in lobby.Clients)
            {
                Task.Run(() => StartClient(client, matchStartMessage, 5000));
            }
            _clientsLock.ExitReadLock();
        }

        private void StartClient(ClientData client, MatchStart matchStartMessage, long timeout)
        {
            _clientsLock.EnterReadLock();
            long startTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (_clients.ContainsKey(client.ClientEndpoint.ToString()) && System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime <= timeout) {
                SendEncryptedMessage(matchStartMessage, client.ClientEndpoint, client, _udpSocket);
                _clientsLock.ExitReadLock();
                Thread.Sleep(500);
                _clientsLock.EnterReadLock();
            }
            _clientsLock.ExitReadLock();

            _clientsLock.EnterWriteLock();
            _clients.Remove(client.ClientEndpoint.ToString());
            _clientsLock.ExitWriteLock();
        }
    }
}
