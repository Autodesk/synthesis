using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SynthesisServer
{
    public class ClientData
    {
        // The public and private key for the server that corresponds with a client
        // Maybe try implementing ECDH in the future

        public IPEndPoint ClientEndpoint { get; set; }
        public string Name { get; set; }
        public string ClientID { get; set; } // May change to actually guid object

        private AsymmetricCipherKeyPair _keyPair;
        private byte[] _symmetricKey;

        private readonly RandomNumberGenerator _random;
        private const int _AES_BLOCK_BYTE_SIZE = 128 / 8;

        public ClientData(string importedPublicKey, DHParameters parameters)
        {
            _random = RandomNumberGenerator.Create();

            _keyPair = GenerateKeys(parameters);

            DHPublicKeyParameters importedPublicKeyParameters = new DHPublicKeyParameters(new BigInteger(importedPublicKey), parameters);
            IBasicAgreement internalKeyAgreement = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgreement.Init(_keyPair.Private);
            BigInteger sharedKey = internalKeyAgreement.CalculateAgreement(importedPublicKeyParameters);
            byte[] sharedKeyBytes = sharedKey.ToByteArray();

            IDigest digest = new Sha256Digest();
            byte[] symmetricKey = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(sharedKeyBytes, 0, sharedKeyBytes.Length);
            digest.DoFinal(symmetricKey, 0);
        }

        public byte[] Encrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _symmetricKey;
                aes.IV = GenerateRandomBytes(_AES_BLOCK_BYTE_SIZE);
                
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] result = new byte[aes.IV.Length + aes.Key.Length];
                    byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

                    aes.IV.CopyTo(result, 0);
                    encryptedData.CopyTo(result, aes.IV.Length);

                    return result;
                }
            }
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _symmetricKey;
                aes.IV = encryptedData.Take(_AES_BLOCK_BYTE_SIZE).ToArray();

                byte[] data = encryptedData.Skip(_AES_BLOCK_BYTE_SIZE).ToArray();

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        private AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        public String GetPublicKey() { return ((DHPublicKeyParameters)_keyPair.Public).Y.ToString(); }

        private byte[] GenerateRandomBytes(int numberOfBytes)
        {
            var randomBytes = new byte[numberOfBytes];
            _random.GetBytes(randomBytes);
            return randomBytes;
        }












        /*
        // It makes more sense for the clients to establish the parameters as they will be sending over the initial request
        private DHParameters GenerateParameters()
        {
            var generator = new DHParametersGenerator();
            generator.Init(1024, 80, new SecureRandom()); // Might need to change certainty; it is currently using what is used in Bouncy Castle source
            return generator.GenerateParameters();
        }
        */

        /*
        public byte[] CalculateSharedKey(string importedPublicKey, DHParameters parameters)
        {
            _keyPair = GenerateKeys(parameters);

            DHPublicKeyParameters importedPublicKeyParameters = new DHPublicKeyParameters(new BigInteger(importedPublicKey), parameters);
            IBasicAgreement internalKeyAgreement = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgreement.Init(_keyPair.Private);
            BigInteger sharedKey = internalKeyAgreement.CalculateAgreement(importedPublicKeyParameters);
            byte[] sharedKeyBytes = sharedKey.ToByteArray();

            IDigest digest = new Sha256Digest();
            byte[] symmetricKey = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(sharedKeyBytes, 0, sharedKeyBytes.Length);
            digest.DoFinal(symmetricKey, 0);

             return symmetricKey;
        }
        */
    }
}
