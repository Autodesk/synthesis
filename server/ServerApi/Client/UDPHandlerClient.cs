using Google.Protobuf;
using Google.Protobuf.Reflection;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SynthesisServer.Client
{
    public class UDPHandlerClient : IUDPHandler
    {
        public UdpClient UDPClient { get; set; }

        public UDPHandlerClient(DHParameters parameters)
        {

        }

        public void Start(UdpClient client, int port, long timeoutMS)
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

        public void ReceiveUpdate()
        {
            throw new NotImplementedException();
        }

        public void HandleKeyExchange()
        {
            throw new NotImplementedException();
        }

        public void HandleGameData()
        {
            throw new NotImplementedException();
        }

        public void HandleDisconnect()
        {
            throw new NotImplementedException();
        }
    }
}
