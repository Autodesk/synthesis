using Google.Protobuf;
using Google.Protobuf.Reflection;
using Org.BouncyCastle.Crypto.Parameters;
using SynthesisServer.Proto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SynthesisAPI.Aether.Client
{
    public interface IUDPHandler {
        
        void Start(long timeoutMS);
        void Stop();

        void UDPReceiveCallback(IAsyncResult asyncResult);
        void UDPSendCallback(IAsyncResult asyncResult);

        void SendUpdate(IMessage Update);
        void SendKeyExchange(KeyExchange keyExchange, string id, long timeoutMS);

        void HandleKeyExchange(KeyExchange keyExchange);
        void HandleGameData(GameUpdate gameUpdate, string id);
        void HandleDisconnect(DisconnectRequest disconnectRequest, string id);
    }
}
