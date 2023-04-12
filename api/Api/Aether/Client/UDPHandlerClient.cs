using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using SynthesisServer.Proto;
using SynthesisAPI.Aether;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SynthesisAPI.Aether.Client
{
    public class UDPHandlerClient : IUDPHandler
    {
        private const int BUFFER_SIZE = 16384;

        private EndPoint _hostEndpoint;
        private string _hostID;

        private IPEndPoint _localEndpoint;
        private string _localID;
        private Socket _localUdpClient;

        private SymmetricEncryptor _encryptor;
        private AsymmetricCipherKeyPair _keyPair;
        private byte[] _symmetricKey;
        private DHParameters _parameters;

        private byte[] _buffer;
        private bool _isRunning;

        public UDPHandlerClient(DHParameters parameters, string id, ConnectionDataClient connectionDataClient, int port, SymmetricEncryptor encryptor)
        {
            _isRunning = false;

            _localID = id;
            _localEndpoint = new IPEndPoint(IPAddress.Any, port);
            _localUdpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _localUdpClient.Bind(_localEndpoint);

            _hostID = connectionDataClient.HostId;
            _hostEndpoint = new IPEndPoint(IPAddress.Parse(connectionDataClient.HostEp.IpAddress), connectionDataClient.HostEp.Port);

            _buffer = new byte[BUFFER_SIZE];

            _encryptor = encryptor;
            _parameters = parameters;
            _keyPair = _encryptor.GenerateKeys(_parameters);
        }

        public void Start(long timeoutMS)
        {
            //EndPoint ep = new IPEndPoint(IPAddress.Any, _hostInfo.Endpoint.Port);
            _localUdpClient.BeginReceiveFrom(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ref _hostEndpoint, new AsyncCallback(UDPReceiveCallback), null);

            SendKeyExchange(
                new KeyExchange()
                {
                    ClientId = _localID,
                    G = _parameters.G.ToString(),
                    P = _parameters.P.ToString(),
                    PublicKey = ((DHPublicKeyParameters)_keyPair.Public).Y.ToString()
                },
                _localID,
                timeoutMS
            );



            throw new NotImplementedException();
        }

        public void Stop()
        {
            _isRunning = false;
            throw new NotImplementedException();
        }

        public void SendUpdate(IMessage Update)
        {
            IO.SendEncryptedMessageTo(Update, _localID, _symmetricKey, _localUdpClient, _hostEndpoint, _encryptor, new AsyncCallback(UDPSendCallback));
        }

        public void SendKeyExchange(KeyExchange keyExchange, string id, long timeoutMS)
        {
            long startTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (_isRunning && System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime <= timeoutMS)
            {
                IO.SendMessageTo(keyExchange, id, _localUdpClient, _hostEndpoint, new AsyncCallback(UDPSendCallback));
                Thread.Sleep(500);
            }
        }

        public void HandleKeyExchange(KeyExchange keyExchange)
        {
            _symmetricKey = _encryptor.GenerateSharedSecret(keyExchange.PublicKey, _parameters, _keyPair);
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
            int received = _localUdpClient.EndReceive(asyncResult);
            if (received != 0 && _isRunning)
            {

                byte[] data = new byte[received];
                Array.Copy(_buffer, data, received);
                if (BitConverter.IsLittleEndian) { Array.Reverse(data); }

                MessageHeader header;
                Any message;

                try
                {
                    header = MessageHeader.Parser.ParseFrom(IO.GetNextMessage(ref data));
                    if (header.IsEncrypted && header.ClientId.Equals(_hostID))
                    {
                        byte[] decryptedData = _encryptor.Decrypt(data, _symmetricKey);
                        message = Any.Parser.ParseFrom(IO.GetNextMessage(ref decryptedData));
                    }
                    else if (!header.IsEncrypted)
                    {
                        message = Any.Parser.ParseFrom(IO.GetNextMessage(ref data));
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
                    HandleGameData(message.Unpack<GameUpdate>(), _hostID);
                }
                else if (message.Is(DisconnectRequest.Descriptor))
                {
                    HandleDisconnect(message.Unpack<DisconnectRequest>(), _hostID);
                }
            }

            if (_isRunning)
            {
                _localUdpClient.BeginReceiveFrom(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ref _hostEndpoint, new AsyncCallback(UDPReceiveCallback), null);
            }
        }

        public void UDPSendCallback(IAsyncResult asyncResult)
        {
        }

        public void UDPEndReceiveCallback(IAsyncResult asyncResult)
        {
        }
    }
}
