using Google.Protobuf;
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

                byte[] msgTypeBytes = buffer.Take(sizeof(int)).ToArray();
                int msgType = BitConverter.ToInt32(msgTypeBytes, 0);
                byte[] data = buffer.Skip(msgTypeBytes.Length).ToArray();

                switch (msgType)
                {
                    // Key Exchange
                    case (int)MessageType.Exchange:
                        HandleExchange(data, remoteEP);
                        break;

                    // Encrypted Message
                    case (int)MessageType.Message:

                        ServerMessage receivedMessage = null;

                        _clientsLock.EnterReadLock();
                        if (_clients.ContainsKey(remoteEP.ToString()))
                        {
                            receivedMessage = ServerMessage.Parser.ParseFrom(_encryptor.Decrypt(data, _clients[remoteEP.ToString()].SymmetricKey));
                        }
                        _clientsLock.ExitReadLock();

                        switch (receivedMessage.ServerMsgTypeCase)
                        {
                            case ServerMessage.ServerMsgTypeOneofCase.ClientStatus:
                                HandleClientStatus(receivedMessage.ClientStatus, _clients[remoteEP.ToString()], remoteEP);
                                break;
                            default:
                                SendEncryptedMessage(new ServerMessage()
                                {
                                    StatusMessage = new ServerMessage.Types.StatusMessage()
                                    {
                                        Level = ServerMessage.Types.StatusMessage.Types.Level.Error,
                                        Msg = "Invalid Message Sent"
                                    }
                                }, MessageType.Message, _encryptor, _clients[remoteEP.ToString()].SymmetricKey, _udpSocket);
                                break;
                        }
                        break;

                    // Invalid Message
                    default:
                        throw new NotImplementedException();
                        break;
                }

                _udpSocket.BeginReceive(RecieveCallback, Port);
                
            } catch (SocketException e)
            {
                Console.WriteLine("Socket Exception");
            }
        }

        private void SendCallback(IAsyncResult asyncResult)
        {

        }

        // Use this due to issues with built in WriteDelimitedTo() method not working
        private byte[] CreateDelimitedMessage(IMessage msg)
        {
            byte[] msgBuffer = new byte[msg.CalculateSize()];
            msg.WriteTo(msgBuffer);

            byte[] delimitedMessage = new byte[sizeof(int) + msgBuffer.Length];
            BitConverter.GetBytes(msgBuffer.Length).CopyTo(delimitedMessage, 0);
            msgBuffer.CopyTo(delimitedMessage, sizeof(int));

            return delimitedMessage;
        }

        private byte[] GetFirstMessageFromBuffer(byte[] delimitedMsgBytes)
        {
            byte[] msgLength = new byte[sizeof(int)];
            Array.Copy(delimitedMsgBytes, 0, msgLength, 0, sizeof(int));

            byte[] msg = new byte[BitConverter.ToInt32(msgLength)];
            Array.Copy(delimitedMsgBytes, sizeof(int), msg, 0, msg.Length);

            return msg;
        }


        private void SendMessage(IMessage msg, bool isEncrypted, IPEndPoint remoteEndPoint, ClientData client, UdpClient server)
        {
            byte[] msgBytes = new byte[msg.CalculateSize()];
            msg.WriteTo(msgBytes);

            byte[] data = new byte[sizeof(int) + msgBytes.Length];

            BitConverter.GetBytes(msgBytes.Length).CopyTo(data, 0);
            msgBytes.CopyTo(data, sizeof(int));

            byte[] finalData;
            if (isEncrypted)
            {
                finalData = _encryptor.Encrypt(data, client.SymmetricKey);
            } 
            else
            {
                finalData = data;
            }

            if (BitConverter.IsLittleEndian) { Array.Reverse(finalData); }
            server.BeginSend(finalData, finalData.Length, remoteEndPoint, SendCallback, null);
        }

        private void HandleExchange(byte[] exchangeData, IPEndPoint clientEP)
        {
            KeyExchange exchangeMessage = KeyExchange.Parser.ParseFrom(exchangeData);
            ClientData EstablishedClientData = null;

            _clientsLock.EnterUpgradeableReadLock();
            if (_clients.ContainsKey(clientEP.ToString()))
            {
                EstablishedClientData = _clients[clientEP.ToString()];
            }
            else
            {
                _clientsLock.EnterWriteLock();
                EstablishedClientData = new ClientData(exchangeMessage.PublicKey, new DHParameters(new BigInteger(exchangeMessage.P), new BigInteger(exchangeMessage.G)))
                {
                    ClientID = Guid.NewGuid().ToString(),
                    ClientEndpoint = clientEP
                };
                _clients.Add(clientEP.ToString(), EstablishedClientData);
                _clientsLock.ExitWriteLock();
            }
            _clientsLock.ExitUpgradeableReadLock();

            SendMessage(new KeyExchange()
            {
                ClientId = EstablishedClientData.ClientID,
                PublicKey = EstablishedClientData.GetPublicKey()
            }, MessageType.Exchange, _udpSocket);
        }

        private void HandleMessage(byte[] data, IPEndPoint clientEP)
        {
            ServerMessage receivedMessage = null;

            _clientsLock.EnterReadLock();
            if (_clients.ContainsKey(clientEP.ToString()))
            {
                receivedMessage = ServerMessage.Parser.ParseFrom(_encryptor.Decrypt(data, _clients[clientEP.ToString()].SymmetricKey));
            }
            _clientsLock.ExitReadLock();

            switch (receivedMessage.ServerMsgTypeCase)
            {
                case ServerMessage.ServerMsgTypeOneofCase.ClientStatus:
                    HandleClientStatus(receivedMessage.ClientStatus, _clients[clientEP.ToString()], clientEP);
                    break;
                default:
                    SendEncryptedMessage(new ServerMessage()
                    {
                        StatusMessage = new ServerMessage.Types.StatusMessage()
                        {
                            Level = ServerMessage.Types.StatusMessage.Types.Level.Error,
                            Msg = "Invalid Message Sent"
                        }
                    }, MessageType.Message, _encryptor, _clients[clientEP.ToString()].SymmetricKey, _udpSocket);
                    break;
            }
        }

        private void HandleClientStatus(ServerMessage.Types.Client desiredState, ClientData client, IPEndPoint clientEndpoint)
        {
            _clients[clientEndpoint.ToString()].Name = desiredState.ClientName; //probably needs a profanity filter

            client.SetCurrentLobby(desiredState.CurrentLobbyName, desiredState.CurrentLobbyIndex, _lobbies, _lobbiesLock);
            client.IsReady = desiredState.IsReady;
            if (desiredState.CurrentLobbyName != null && _lobbies[desiredState.CurrentLobbyName].Host.IsReady)
            {
                StartGame(desiredState.CurrentLobbyName, _lobbies[desiredState.CurrentLobbyName], 5000);
            }
        }

        private Task StartGame(string lobbyName, Lobby lobby, long timeoutMS)
        {
            return Task.Run(() => 
            {
                long time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
                _lobbiesLock.EnterReadLock();
                while (timeoutMS >= System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - time && _lobbies.ContainsKey(lobbyName))
                {
                    foreach (ClientData client in lobby.Clients)
                    {
                        if (lobby.Host.)
                    }
                    _lobbiesLock.ExitReadLock();
                    Thread.Sleep(500);
                    _lobbiesLock.EnterReadLock();
                }
                _lobbiesLock.ExitReadLock();
                throw new NotImplementedException();
            });
        }
    }
}
