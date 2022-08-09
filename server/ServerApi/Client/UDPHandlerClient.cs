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
using System.Threading;

namespace SynthesisServer.Client
{
    public class UDPHandlerClient : IUDPHandler
    {
        private const int BUFFER_SIZE = 16384;

        private UDPClientInfo _hostInfo;
        private UDPClientInfo _localInfo;

        private Socket _localUdpClient;

        private SymmetricEncryptor _encryptor;
        private AsymmetricCipherKeyPair _keyPair;
        private byte[] _symmetricKey;
        private DHParameters _parameters;

        private byte[] _buffer;

        public UDPHandlerClient(DHParameters parameters, string id, ConnectionDataClient connectionDataClient, int port, SymmetricEncryptor encryptor)
        {
            _localInfo = new UDPClientInfo()
            {
                ID = id,
                Endpoint = new IPEndPoint(IPAddress.Any, port)
            };
            _hostInfo = new UDPClientInfo()
            {
                ID = connectionDataClient.HostId,
                Endpoint = new IPEndPoint(IPAddress.Parse(connectionDataClient.HostEp.IpAddress), connectionDataClient.HostEp.Port)
            };

            _localUdpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _localUdpClient.Bind(_localInfo.Endpoint);
            _buffer = new byte[BUFFER_SIZE];

            _encryptor = encryptor;
            _parameters = parameters;
            _keyPair = _encryptor.GenerateKeys(_parameters);
        }

        public void Start(long timeoutMS)
        {
            EndPoint ep = new IPEndPoint(IPAddress.Any, _hostInfo.Endpoint.Port);
            _localUdpClient.BeginReceiveFrom(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ref ep, new AsyncCallback(UDPReceiveCallback), null);

            long startTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime <= timeoutMS)
            {
                HandleKeyExchange
                (
                    new KeyExchange()
                    {
                        ClientId = _localInfo.ID,
                        G = _parameters.G.ToString(),
                        P = _parameters.P.ToString(),
                        PublicKey = ((DHPublicKeyParameters)_keyPair.Public).Y.ToString()
                    },
                    _localInfo.ID
                );
                Thread.Sleep(500);
            }
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
            IO.SendMessage(keyExchange, id, _localUdpClient, new AsyncCallback(UDPSendCallback));
            
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

            MessageHeader header;
            Any message;

            try
            {
                header = MessageHeader.Parser.ParseFrom(IO.GetNextMessage(ref buffer));
                if (header.IsEncrypted && header.ClientId.Equals(_hostInfo))
                {
                    byte[] decryptedData = _encryptor.Decrypt(buffer, _symmetricKey);
                    message = Any.Parser.ParseFrom(IO.GetNextMessage(ref decryptedData));
                }
                else if (!header.IsEncrypted)
                {
                    message = Any.Parser.ParseFrom(IO.GetNextMessage(ref buffer));
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
                HandleKeyExchange(message.Unpack<KeyExchange>(), _hostInfo.ID);
            } else if (message.Is(GameUpdate.Descriptor))
            {
                HandleGameData(message.Unpack<GameUpdate>(), _hostInfo.ID);
            } else if (message.Is(DisconnectRequest.Descriptor))
            {
                HandleDisconnect(message.Unpack<DisconnectRequest>(), _hostInfo.ID);
            }

            _localUdpClient.BeginReceive(new AsyncCallback(UDPReceiveCallback), null);
        }

        public void UDPSendCallback(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
    }
}
