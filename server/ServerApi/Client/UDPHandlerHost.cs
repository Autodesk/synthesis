using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Crypto;
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
        struct UDPClientInfo
        {
            public string id;
            public EndPoint remoteEP;
            public byte[] buffer;
            public DHParameters parameters;
            public AsymmetricCipherKeyPair keyPair;
            public byte[] symmetricKey;
            public bool exchangeStatus;
            public long lastMessageTime;
        }

        private const int BUFFER_SIZE = 16384;

        private Dictionary<string, UDPClientInfo> _clientsInfo; // Accessed by guid
        private Socket _localUdpClient;
        private IPEndPoint _localEndpoint;
        private string _localID;
        private bool _isRunning;

        private SymmetricEncryptor _encryptor;

        public UDPHandlerHost(ConnectionDataHost connectionDataHost, int port, string id, SymmetricEncryptor encryptor)
        {
            _isRunning = false;

            _localEndpoint = new IPEndPoint(IPAddress.Any, port);
            _localUdpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _localUdpClient.Bind(_localEndpoint);

            _localID = id;

            _encryptor = encryptor;

            foreach (var client in connectionDataHost.Clients)
            {
                UDPClientInfo clientInfo;
                clientInfo.id = client.Key;
                clientInfo.remoteEP = new IPEndPoint(IPAddress.Parse(client.Value.IpAddress), client.Value.Port);
                clientInfo.buffer = new byte[BUFFER_SIZE];

                clientInfo.parameters = null;
                clientInfo.keyPair = null;
                clientInfo.symmetricKey = null;
                clientInfo.exchangeStatus = false;

                clientInfo.lastMessageTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

                _clientsInfo.Add(client.Key, clientInfo); // Do not set parameters for each client; those will be sent by the client during key exchange
            }
        }

        public void Start(long timeoutMS)
        {
            _isRunning = true;

            foreach (var client in _clientsInfo.Values)
            {
                EndPoint ep = client.remoteEP;
                _localUdpClient.BeginReceiveFrom(client.buffer, 0, BUFFER_SIZE, SocketFlags.None, ref ep, new AsyncCallback(UDPReceiveCallback), client);
            }
        }

        public void Stop()
        {
            _isRunning = false;
            throw new NotImplementedException();
        }

        public void SendUpdate(IMessage Update)
        {
            foreach (var client in _clientsInfo.Values)
            {
                IO.SendEncryptedMessageTo(Update, client.id, client.symmetricKey, _localUdpClient, client.remoteEP, _encryptor, new AsyncCallback(UDPSendCallback)); 
            }
        }

        public void SendKeyExchange(KeyExchange keyExchange, string id, long timeoutMS)
        {

        }

        public void HandleKeyExchange(KeyExchange keyExchange)
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
            UDPClientInfo info = (UDPClientInfo)asyncResult.AsyncState;
            int length = _localUdpClient.EndReceiveFrom(asyncResult, ref info.remoteEP);
            if (BitConverter.IsLittleEndian) { Array.Reverse(info.buffer); } // make sure to do while sending

            MessageHeader header;
            Any message;

            try
            {
                header = MessageHeader.Parser.ParseFrom(IO.GetNextMessage(ref info.buffer));
                if (header.IsEncrypted && header.ClientId.Equals(info.id))
                {
                    byte[] decryptedData = _encryptor.Decrypt(info.buffer, info.buffer);
                    message = Any.Parser.ParseFrom(IO.GetNextMessage(ref decryptedData));
                }
                else if (!header.IsEncrypted)
                {
                    message = Any.Parser.ParseFrom(IO.GetNextMessage(ref info.buffer));
                }
                else
                {
                    message = Any.Pack(new StatusMessage()
                    {
                        LogLevel = StatusMessage.Types.LogLevel.Error,
                        Msg = "Invalid Message Received"
                    });
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                message = Any.Pack(new StatusMessage()
                {
                    LogLevel = StatusMessage.Types.LogLevel.Error,
                    Msg = "Invalid Message Received"
                });
            }
            if (message.Is(KeyExchange.Descriptor))
            {

            }
            else if (message.Is(GameUpdate.Descriptor))
            {
                HandleGameData(message.Unpack<GameUpdate>(), info.id);
            }
            else if (message.Is(DisconnectRequest.Descriptor))
            {
                HandleDisconnect(message.Unpack<DisconnectRequest>(), info.id);
            }

            throw new NotImplementedException();
        }

        public void UDPSendCallback(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
    }
}
