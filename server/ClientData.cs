using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Net;

namespace SynthesisServer
{
    public class ClientData
    {
        // The public and private key for the server that corresponds with a client
        // Maybe try implementing ECDH in the future

        public IPEndPoint ClientEndpoint { get; set; }
        public string Name { get; set; }
        public string ClientID { get; set; } // May change to actually guid object probably not though
        public byte[] SymmetricKey { get; private set; }

        private AsymmetricCipherKeyPair _keyPair;

        public ClientData(string importedPublicKey, DHParameters parameters)
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
    }
}
