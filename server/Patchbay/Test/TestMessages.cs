using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using SynthesisServer.Proto;
using SynthesisServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynthesisServer.Test
{
    // Run tests on a different machine from the server

    [TestFixture]
    public static class TestMessages
    {
        private static UdpClient _udpClient;
        private static Socket _tcpSocket;

        private static int _tcpPort;
        private static int _udpPort;
        private static IPAddress _serverIP;

        private static string _clientID;

        private static SymmetricEncryptor _encryptor;
        private static AsymmetricCipherKeyPair _keyPair;
        private static DHParameters _dhParameters;
        private static byte[] _symmetricKey;

        private static bool _isRunning = false;

        private static Heartbeat _heartbeat;

        private static void Init()
        {
            

            _tcpPort = 18001;
            _udpPort = 18000;

            _serverIP = IPAddress.Parse("76.144.67.63"); // Specify during actual test 

            _dhParameters = GenerateParameters();
            _keyPair = GenerateKeys(_dhParameters);

            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private static void GenerateSharedSecret(string importedPublicKey, DHParameters parameters)
        {
            _keyPair = GenerateKeys(parameters);

            DHPublicKeyParameters importedPublicKeyParameters = new DHPublicKeyParameters(new BigInteger(importedPublicKey), parameters);
            IBasicAgreement internalKeyAgreement = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgreement.Init(_keyPair.Private);
            BigInteger sharedKey = internalKeyAgreement.CalculateAgreement(importedPublicKeyParameters);
            byte[] sharedKeyBytes = sharedKey.ToByteArray();

            IDigest digest = new Sha256Digest();
            _symmetricKey = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(sharedKeyBytes, 0, sharedKeyBytes.Length);
            digest.DoFinal(_symmetricKey, 0);
        }

        private static AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        private static DHParameters GenerateParameters()
        {
            DHParametersGenerator generator = new DHParametersGenerator();
            generator.Init(1024, 80, new SecureRandom()); // not too sure about these numbers
            return generator.GenerateParameters();
        }

        private static void SendMessage(IMessage msg, Socket socket)
        {
            Any packedMsg = Any.Pack(msg);
            byte[] msgBytes = new byte[packedMsg.CalculateSize()];
            packedMsg.WriteTo(msgBytes);

            MessageHeader header = new MessageHeader() { IsEncrypted = false };
            byte[] headerBytes = new byte[header.CalculateSize()];
            header.WriteTo(headerBytes);

            byte[] data = new byte[sizeof(int) + headerBytes.Length + sizeof(int) + msgBytes.Length];

            BitConverter.GetBytes(headerBytes.Length).CopyTo(data, 0);
            headerBytes.CopyTo(data, sizeof(int));

            BitConverter.GetBytes(msgBytes.Length).CopyTo(data, sizeof(int) + headerBytes.Length);
            msgBytes.CopyTo(data, sizeof(int) + headerBytes.Length + sizeof(int));

            if (BitConverter.IsLittleEndian) { Array.Reverse(data); }
            socket.Send(data);
        }

        private static void SendEncryptedMessage(IMessage msg, Socket socket)
        {
            Any packedMsg = Any.Pack(msg);
            byte[] msgBytes = new byte[packedMsg.CalculateSize()];
            packedMsg.WriteTo(msgBytes);

            byte[] delimitedMessage = new byte[sizeof(int) + msgBytes.Length];
            BitConverter.GetBytes(msgBytes.Length).CopyTo(delimitedMessage, 0);
            msgBytes.CopyTo(delimitedMessage, sizeof(int));

            byte[] encryptedMessage = _encryptor.Encrypt(delimitedMessage, _symmetricKey);

            MessageHeader header = new MessageHeader() { IsEncrypted = true };
            byte[] headerBytes = new byte[header.CalculateSize()];
            header.WriteTo(headerBytes);

            byte[] data = new byte[sizeof(int) + headerBytes.Length + encryptedMessage.Length];

            BitConverter.GetBytes(headerBytes.Length).CopyTo(data, 0);
            headerBytes.CopyTo(data, sizeof(int));

            encryptedMessage.CopyTo(data, sizeof(int) + headerBytes.Length);

            if (BitConverter.IsLittleEndian) { Array.Reverse(data); }
            socket.Send(data);
        }

        private static byte[] GetNextMessage(ref byte[] buffer)
        {
            byte[] msgLength = new byte[sizeof(int)];
            Array.Copy(buffer, 0, msgLength, 0, msgLength.Length);

            byte[] msg = new byte[BitConverter.ToInt32(msgLength)];
            Array.Copy(buffer, sizeof(int), msg, 0, msg.Length);

            buffer = buffer.Skip(msgLength.Length + msg.Length).ToArray();
            return msg;
        }

        [Test]
        public static void TestExchange()
        {
            Init();
            while (!_tcpSocket.Connected)
            {
                try
                {
                    _tcpSocket.Connect(_serverIP, _tcpPort);
                }
                catch (SocketException e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            // Connection to server established!
            Assert.IsTrue(_tcpSocket.Connected);

            SendMessage
            (
                new KeyExchange()
                {
                    G = _dhParameters.G.ToString(),
                    P = _dhParameters.P.ToString(),
                    PublicKey = ((DHPublicKeyParameters)_keyPair.Public).Y.ToString()
                },
                _tcpSocket
            );

            byte[] buffer = new byte[4096];
            int rec = _tcpSocket.Receive(buffer);
            byte[] data = new byte[rec];
            Array.Copy(buffer, data, rec);
            if (BitConverter.IsLittleEndian) { Array.Reverse(data); }

            MessageHeader header = MessageHeader.Parser.ParseFrom(GetNextMessage(ref data));

            // Recieved a non-encrypted header
            Assert.IsTrue(!header.IsEncrypted);

            if (!header.IsEncrypted)
            {
                Any message = Any.Parser.ParseFrom(GetNextMessage(ref data));
                if (message.Is(KeyExchange.Descriptor)) 
                {
                    // Received a key exchange message!
                    Assert.IsTrue(true);
                }
            }
        }

        /*
        [Test]
        public static void TestAllHandshakes()
        {
            _isRunning = true;
            Init();
            while (!_tcpSocket.Connected)
            {
                try
                {
                    _tcpSocket.Connect(_serverIP, _tcpPort);
                }
                catch (SocketException e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            // Connection to server established!
            Assert.IsTrue(_tcpSocket.Connected);

            SendMessage
            (
                new KeyExchange()
                {
                    G = _dhParameters.G.ToString(),
                    P = _dhParameters.P.ToString(),
                    PublicKey = ((DHPublicKeyParameters)_keyPair.Public).Y.ToString()
                },
                _tcpSocket
            );

            byte[] buffer = new byte[4096];
            int rec = _tcpSocket.Receive(buffer);
            byte[] data = new byte[rec];
            Array.Copy(buffer, data, rec);
            if (BitConverter.IsLittleEndian) { Array.Reverse(data); }

            MessageHeader header = MessageHeader.Parser.ParseFrom(GetNextMessage(ref data));

            Assert.IsFalse(header.IsEncrypted);
            Any message = Any.Parser.ParseFrom(GetNextMessage(ref data));
            Assert.IsTrue(message.Is(KeyExchange.Descriptor));

            GenerateSharedSecret(message.Unpack<KeyExchange>().PublicKey, _dhParameters);
            _clientID = message.Unpack<KeyExchange>().ClientId;
            System.Diagnostics.Debug.WriteLine(BitConverter.ToString(_symmetricKey));

            _heartbeat = new Heartbeat()
            {
                ClientId = _clientID
            };


            Task.Run(() =>
            {
                while (_isRunning)
                {
                    SendEncryptedMessage(_heartbeat, _tcpSocket);
                    Thread.Sleep(500);
                }
            });

            
            
        }
        */
    }
}
