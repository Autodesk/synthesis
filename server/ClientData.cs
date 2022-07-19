using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SynthesisServer
{
    public class ClientData
    {
        // The public and private key for the server that corresponds with a client
        // Maybe try implementing ECDH in the future

        //public IPEndPoint ClientEndpoint { get; set; }
        public string Name { get; set; }
        public string ClientID { get; set; }
        public byte[] SymmetricKey { get; private set; }
        public long LastHeartbeat { get; private set; }
        public bool IsReady { get; set; }
        public string CurrentLobby { get; set; }
        public Socket ClientSocket { get; private set; }

        private AsymmetricCipherKeyPair _keyPair;

        public ClientData(Socket socket)
        {
            ClientSocket = socket;
            CurrentLobby = null;
            IsReady = false;
            LastHeartbeat = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void GenerateSharedSecret(string importedPublicKey, DHParameters parameters)
        {
            _keyPair = GenerateKeys(parameters);

            DHPublicKeyParameters importedPublicKeyParameters = new DHPublicKeyParameters(new BigInteger(importedPublicKey), parameters);
            IBasicAgreement internalKeyAgreement = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgreement.Init(_keyPair.Private);
            BigInteger sharedKey = internalKeyAgreement.CalculateAgreement(importedPublicKeyParameters);
            byte[] sharedKeyBytes = sharedKey.ToByteArray();

            IDigest digest = new Sha256Digest();
            SymmetricKey = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(sharedKeyBytes, 0, sharedKeyBytes.Length);
            digest.DoFinal(SymmetricKey, 0);
        }

        private AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        public String GetPublicKey() { return ((DHPublicKeyParameters)_keyPair.Public).Y.ToString(); }

        public void UpdateHeartbeat() { LastHeartbeat = System.DateTimeOffset.Now.ToUnixTimeMilliseconds(); }
    }
}
