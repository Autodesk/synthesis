using Google.Protobuf;
using Google.Protobuf.Reflection;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SynthesisServer.Client
{
    public interface IUDPHandler {
        MessageDescriptor Descriptor { get; set; }
        DHParameters Parameters { get; set; }
        UdpClient UDPClient { get; set; }

        void Start(UdpClient client, int port, long timeoutMS);
        void Stop();

        void SendUpdate(IMessage Update);
        void ReceiveUpdate();

        void HandleKeyExchange();
        void HandleGameData();
        void HandleDisconnect();
    }
}
