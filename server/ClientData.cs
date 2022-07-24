using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using SynthesisServer.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SynthesisServer
{
    public class ClientData
    {
        public string Name { get; set; }
        public long LastHeartbeat { get; private set; }
        public bool IsReady { get; set; }
        public string CurrentLobby { get; set; }
        public Socket ClientSocket { get; private set; }
        public string ID { get; private set; }
        public byte[] SocketBuffer { get; private set; }
        public IPEndPoint UDPEndPoint { get; set; }

        public byte[] SymmetricKey { get; private set; }
        private AsymmetricCipherKeyPair _keyPair;
        private DHParameters parameters;

        public ReaderWriterLockSlim ClientLock { get; private set; }

        public ClientData(Socket socket, int bufferSize)
        {
            ClientLock = new ReaderWriterLockSlim();
            ClientSocket = socket;
            CurrentLobby = null;
            IsReady = false;
            ID = Guid.NewGuid().ToString();
            SocketBuffer = new byte[bufferSize];
            LastHeartbeat = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void GenerateSharedSecret(string importedPublicKey, DHParameters dhParameters, SymmetricEncryptor encryptor)
        {
            parameters = dhParameters;
            _keyPair = encryptor.GenerateKeys(parameters);
            SymmetricKey = encryptor.GenerateSharedSecret(importedPublicKey, parameters, _keyPair);
        }

        public String GetPublicKey() { return ((DHPublicKeyParameters)_keyPair.Public).Y.ToString(); }

        public void UpdateHeartbeat() { LastHeartbeat = System.DateTimeOffset.Now.ToUnixTimeMilliseconds(); }
    }
}
