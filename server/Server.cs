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

                        KeyExchange exchangeMessage = KeyExchange.Parser.ParseFrom(data);
                        ClientData EstablishedClientData = null;
                     
                        _clientsLock.EnterUpgradeableReadLock();
                        if (_clients.ContainsKey(remoteEP.ToString()))
                        {
                            EstablishedClientData = _clients[remoteEP.ToString()];
                        }
                        else
                        {
                            _clientsLock.EnterWriteLock();
                            EstablishedClientData = new ClientData(exchangeMessage.PublicKey, new DHParameters(new BigInteger(exchangeMessage.P), new BigInteger(exchangeMessage.G)))
                            {
                                ClientID = Guid.NewGuid().ToString(),
                                ClientEndpoint = remoteEP
                            };
                            _clients.Add(remoteEP.ToString(), EstablishedClientData);
                            _clientsLock.ExitWriteLock();
                        }
                        _clientsLock.ExitUpgradeableReadLock();
                        
                        SendMessage(new KeyExchange()
                        {
                            ClientId = EstablishedClientData.ClientID,
                            PublicKey = EstablishedClientData.GetPublicKey()
                        }, _udpSocket);

                        break;

                    // Encrypted Message
                    case (int)MessageType.Message:

                        ServerMessage receivedMessage = null;

                        _clientsLock.EnterReadLock();
                        if (_clients.ContainsKey(remoteEP.ToString()))
                        {
                            receivedMessage = ServerMessage.Parser.ParseFrom(_encryptor.Decrypt(data, _clients[remoteEP.ToString()].SymmetricKey));
                        }

                        switch (receivedMessage.ServerMsgTypeCase)
                        {
                            case ServerMessage.ServerMsgTypeOneofCase.ClientStatus:
                                throw new NotImplementedException();
                                break;
                            default:
                                SendEncryptedMessage(new ServerMessage()
                                {
                                    StatusMessage = new ServerMessage.Types.StatusMessage()
                                    {
                                        Level = ServerMessage.Types.StatusMessage.Types.Level.Error,
                                        Msg = "Invalid Message Sent"
                                    }
                                }, _encryptor, _clients[remoteEP.ToString()].SymmetricKey, _udpSocket);
                                break;
                        }
                        
                        throw new NotImplementedException();
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


        private void SendMessage(IMessage msg, UdpClient server)
        {
            byte[] data = new byte[msg.CalculateSize()];
            msg.WriteTo(data);

            if (BitConverter.IsLittleEndian) { Array.Reverse(data); }
            server.BeginSend(data, data.Length, SendCallback, null);
        }

        private void SendEncryptedMessage(IMessage msg, SymmetricEncryptor encryptor, byte[] symmetricKey, UdpClient server)
        {
            byte[] data = new byte[msg.CalculateSize()];
            msg.WriteTo(data);

            byte[] encryptedData = encryptor.Encrypt(data, symmetricKey);
            if (BitConverter.IsLittleEndian) { Array.Reverse(encryptedData); }
            server.BeginSend(encryptedData, encryptedData.Length, SendCallback, null);
        }

        private void HandleClientStatus(ServerMessage.Types.Client desiredState, ClientData client, IPEndPoint clientEndpoint)
        {
            _clients[clientEndpoint.ToString()].Name = desiredState.ClientName; //probably needs a profanity filter
            _lobbiesLock.EnterUpgradeableReadLock();

            client.SetCurrentLobby(desiredState.CurrentLobbyName, _lobbies, _lobbiesLock);
            client.IsReady = desiredState.IsReady;
            if (desiredState.CurrentLobbyName != null && _lobbies[desiredState.CurrentLobbyName].Host.IsReady)
            {
                StartGame(_lobbies[desiredState.CurrentLobbyName]);
            }
        }

        private void StartGame(Lobby lobby)
        {
            throw new NotImplementedException();
        }
    }
}
