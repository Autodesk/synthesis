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
        private List<ClientData> _clients;

        private ReaderWriterLockSlim _lobbiesLock;
        private List<Lobby> _lobbies;

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

            _clients = new List<ClientData>();
            _clientsLock = new ReaderWriterLockSlim();


            _udpSocket.BeginReceive(RecieveCallback, Port);
        }

        private void RecieveCallback(IAsyncResult asyncResult)
        {

            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, (int)asyncResult.AsyncState);
                byte[] buffer = _udpSocket.EndReceive(asyncResult, ref remoteEP);
                if (BitConverter.IsLittleEndian) { Array.Reverse(buffer); } // make sure to do while sending
                // decode and parse data and then send response
                try
                {
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
                            foreach (var x in _clients)
                            {
                                if (x.ClientID.Equals(exchangeMessage.ClientId))
                                {
                                    EstablishedClientData = x;
                                }
                            }
                            if (EstablishedClientData == null)
                            {
                                _clientsLock.EnterWriteLock();
                                _clients.Add(new ClientData(exchangeMessage.PublicKey, new DHParameters(new BigInteger(exchangeMessage.P), new BigInteger(exchangeMessage.G)))
                                {
                                    ClientID = exchangeMessage.ClientId,
                                    ClientEndpoint = remoteEP
                                });
                                _clientsLock.ExitWriteLock();
                            }

                            //_udpSocket.BeginSend();
                            _clientsLock.ExitUpgradeableReadLock();


                            throw new NotImplementedException();
                            break;

                        // Encrypted Message
                        case (int)MessageType.Message:
                            throw new NotImplementedException();
                            break;
                        
                        // Invalid Message
                        default:
                            throw new NotImplementedException();
                            break;
                    }

                } catch (Exception e)
                {
                    Console.WriteLine(e);
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


    }
}
