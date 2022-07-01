using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SynthesisServer
{
    public sealed class Server
    {
        private UdpClient _udpSocket;
        private List<ClientData> _clients;

        public int Port { get; set; } = 10800;
        private static readonly Lazy<Server> lazy = new Lazy<Server>(() => new Server());
        public static Server Instance { get { return lazy.Value; } }
        private Server() 
        {
        }


        public void Start()
        {
            _udpSocket = new UdpClient(Port, AddressFamily.InterNetwork);
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
                } catch (Exception e)
                {
                    Console.WriteLine("Make sure to handle only this specific exception:");
                    Console.WriteLine(e);
                    try
                    {
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
