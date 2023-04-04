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
using SynthesisServer.Proto;

using Client = SynthesisServer.Proto.Client;

namespace SynthesisServer {
    public class ClientData {
        // The public and private key for the server that corresponds with a client
        // Maybe try implementing ECDH in the future

        //public IPEndPoint ClientEndpoint { get; set; }
        public string Name { get; set; } = "Epic Gamer";
        public byte[] SymmetricKey { get; private set; }
        public long LastHeartbeat { get; private set; }
        public bool IsReady { get; set; }
        public string CurrentLobby { get; set; } = string.Empty;
        public Socket ClientSocket { get; private set; }
        public IPEndPoint UDPEndPoint { get; set; }
        public string ID { get; private set; }

        private AsymmetricCipherKeyPair _keyPair;

        private DHParameters _parameters;

        public ClientData(Socket socket, string id) {
            ID = id;
            ClientSocket = socket;
            IsReady = false;
            LastHeartbeat = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void GenerateSharedSecret(string importedPublicKey, DHParameters parameters, SymmetricEncryptor encryptor) {
            _parameters = parameters;
            _keyPair = encryptor.GenerateKeys(parameters);
            SymmetricKey = encryptor.GenerateSharedSecret(importedPublicKey, parameters, _keyPair);
        }

        public String GetPublicKey() { return ((DHPublicKeyParameters)_keyPair.Public).Y.ToString(); }

        public void UpdateHeartbeat() { LastHeartbeat = System.DateTimeOffset.Now.ToUnixTimeMilliseconds(); }

        public static implicit operator SynthesisServer.Proto.Client(ClientData data)
            => new SynthesisServer.Proto.Client() { Id = data.ID, Name = data.Name };
    }
}