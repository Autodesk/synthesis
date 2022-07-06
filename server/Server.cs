using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
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
                
                // decode and parse data and then send response
                try
                {
                    KeyExchange exchangeMessage = KeyExchange.Parser.ParseFrom(buffer);
                    bool clientAlreadyEstablished = false;
                    _clientsLock.EnterWriteLock();
                    foreach (var x in _clients)
                    {
                        if (x.ClientID.Equals(exchangeMessage.ClientId))
                        {
                            clientAlreadyEstablished = true;
                        }
                    }
                    if (clientAlreadyEstablished)
                    {
                        //_udpSocket.BeginSend() (pass in remote endpoint and send generic response with error)
                        throw new NotImplementedException();
                    } else
                    {
                        _clients.Add(new ClientData
                        {
                            ClientID = exchangeMessage.ClientId,
                            ClientEndpoint = remoteEP,
                            Parameters = new DHParameters(new BigInteger(exchangeMessage.P), new BigInteger(exchangeMessage.G))
                        });
                    }
                    _clientsLock.ExitWriteLock();
                    

                } catch (Exception e)
                {
                    Console.WriteLine("Make sure to handle only this specific exception:");
                    Console.WriteLine(e);
                    try
                    {
                        Aes test = Aes.Create(); // check to create using key
                        ServerMessage msg = ServerMessage.Parser.ParseFrom(buffer);
                    }
                    catch (Exception asdf)
                    {
                        Console.WriteLine("Invalid message");
                        //Send message to client saying its invalid
                    }
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
