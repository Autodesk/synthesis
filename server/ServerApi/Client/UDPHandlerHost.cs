using Google.Protobuf;
using Google.Protobuf.Reflection;
using Org.BouncyCastle.Crypto.Parameters;
using SynthesisServer.Proto;
using SynthesisServer.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SynthesisServer.Client
{
    public class UDPHandlerHost : IUDPHandler
    {
        private Dictionary<string, UDPClientInfo> _clientsInfo; // Accessed by guid
        private UdpClient _localUdpClient;

        public UDPHandlerHost(ConnectionDataHost connectionDataHost, int port)
        {
            _localUdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            foreach (var client in connectionDataHost.Clients)
            {
                _clientsInfo.Add(client.Key, new UDPClientInfo()
                {
                    ID = client.Key,
                    RemoteEP = new IPEndPoint(IPAddress.Parse(client.Value.IpAddress), client.Value.Port)
                }); // Do not set parameters for each client; those will be sent by the client during key exchange
            }
        }

        public void Start(long timeoutMS)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void SendUpdate(IMessage Update)
        {
            throw new NotImplementedException();
        }

        public void HandleKeyExchange(KeyExchange keyExchange, string id)
        {
            throw new NotImplementedException();
        }

        public void HandleGameData(GameUpdate gameUpdate, string id)
        {
            throw new NotImplementedException();
        }

        public void HandleDisconnect(DisconnectRequest disconnectRequest, string id)
        {
            throw new NotImplementedException();
        }

        public void UDPReceiveCallback(IAsyncResult asyncResult)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = _localUdpClient.EndReceive(asyncResult, ref remoteEP);
            if (BitConverter.IsLittleEndian) { Array.Reverse(buffer); } // make sure to do while sending

            MessageHeader header = MessageHeader.Parser.ParseFrom(IO.GetNextMessage(ref buffer));
            throw new NotImplementedException();
        }

        public void UDPSendCallback(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
    }
}
