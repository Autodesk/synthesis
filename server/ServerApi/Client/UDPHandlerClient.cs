using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
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
    public class UDPHandlerClient : IUDPHandler
    {
        private UDPClientInfo _hostInfo;
        private UdpClient _localUdpClient;
        private SymmetricEncryptor _encryptor;
        private byte[] _symmetricKey;

        public UDPHandlerClient(DHParameters parameters, ConnectionDataClient connectionDataClient, int port, SymmetricEncryptor encryptor)
        {
            _encryptor = encryptor;
            _localUdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, port));

            _hostInfo = new UDPClientInfo()
            {
                parameters = parameters,
                ID = connectionDataClient.HostId,
                RemoteEP = new IPEndPoint(IPAddress.Parse(connectionDataClient.HostEp.IpAddress), connectionDataClient.HostEp.Port)
            };
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
