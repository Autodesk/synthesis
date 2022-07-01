using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SynthesisServer
{
    public class ClientData
    {
        // The public and private key for the server that corresponds with a client
        // Maybe try implementing ECDH in the future
        public DHParameters Parameters { get; private set; } // send parameters to client in order to generate keys
        public IPEndPoint ClientEndpoint { get; set; }
        public string Name { get; set; }
        public string ClientID { get; private set; } // May change to actually guid object

        private AsymmetricCipherKeyPair _keyPair;
        private BigInteger _sharedKey;

        public ClientData()
        {
            Parameters = GenerateParameters();
            _keyPair = GenerateKeys(Parameters);
        }

        private DHParameters GenerateParameters()
        {
            var generator = new DHParametersGenerator();
            generator.Init(1024, 80, new SecureRandom()); // Might need to change certainty; it is currently using what is used in Bouncy Castle source
            return generator.GenerateParameters();
        }

        private AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        public String GetPublicKey() { return ((DHPublicKeyParameters)_keyPair.Public).Y.ToString(); }

        public void CalculateSharedKey(string importedPublicKey)
        {
            DHPublicKeyParameters importedPublicKeyParameters = new DHPublicKeyParameters(new BigInteger(importedPublicKey), Parameters);
            IBasicAgreement internalKeyAgreement = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgreement.Init(_keyPair.Private);
            _sharedKey = internalKeyAgreement.CalculateAgreement(importedPublicKeyParameters);
        }
    }
}
