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
        private IPEndPoint _endpoint;
        //private UdpClient _udpSocket;
        private Socket _socket;
        private int _bufferSize = 4096;

        private static readonly Lazy<Server> lazy = new Lazy<Server>(() => new Server());
        public static Server Instance { get { return lazy.Value; } }
        private Server() 
        {
        }

        private void ConfigureServer(int port)
        {
            _endpoint = new IPEndPoint(IPAddress.Any, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(_endpoint);
            //_udpSocket = new UdpClient(port, AddressFamily.InterNetwork);
            //_udpSocket.Connect(_endpoint);
        }

        public void Start(int port)
        {
            //_udpSocket.BeginReceive(RecieveCallback, null);
            ConfigureServer(port);
            _socket.BeginReceive(new byte[_bufferSize], 0, _bufferSize, SocketFlags.None, RecieveCallback, );
        }

        private void RecieveCallback(IAsyncResult asyncResult)
        {

            try
            {
                //IPEndPoint remoteEndpoint = new IPEndPoint();
                //byte[] _data = _udpSocket.EndReceive(asyncResult, ref remoteEndpoint);
                SocketFlags flags = SocketFlags.None;
                EndPoint remoteEP = _endpoint;
                IPPacketInformation info;
                _socket.EndReceiveMessageFrom(asyncResult, ref flags, ref remoteEP, out info);
                _socket.BeginReceive();
                
            } catch (SocketException e)
            {

            }
        }

        private void SendCallback(IAsyncResult asyncResult)
        {

        }


    }
}
