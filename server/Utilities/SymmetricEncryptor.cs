using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SynthesisServer.Utilities
{
    public class SymmetricEncryptor
    {
        private readonly RandomNumberGenerator _random;
        private const int _AES_BLOCK_BYTE_SIZE = 128 / 8;

        public SymmetricEncryptor()
        {
            _random = RandomNumberGenerator.Create();
        }

        private byte[] GenerateRandomBytes(int numberOfBytes)
        {
            var randomBytes = new byte[numberOfBytes];
            _random.GetBytes(randomBytes);
            return randomBytes;
        }


        public byte[] GenerateSharedSecret(string importedPublicKey, DHParameters parameters, AsymmetricCipherKeyPair keyPair)
        {
            DHPublicKeyParameters importedPublicKeyParameters = new DHPublicKeyParameters(new BigInteger(importedPublicKey), parameters);
            IBasicAgreement internalKeyAgreement = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgreement.Init(keyPair.Private);
            BigInteger sharedKey = internalKeyAgreement.CalculateAgreement(importedPublicKeyParameters);
            byte[] sharedKeyBytes = sharedKey.ToByteArray();

            IDigest digest = new Sha256Digest();
            byte[] SymmetricKey = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(sharedKeyBytes, 0, sharedKeyBytes.Length);
            digest.DoFinal(SymmetricKey, 0);
            return SymmetricKey;
        }

        public AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        public byte[] Encrypt(byte[] data, byte[] symmetricKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = symmetricKey;
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

        public byte[] Decrypt(byte[] encryptedData, byte[] symmetricKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = symmetricKey;
                aes.IV = encryptedData.Take(_AES_BLOCK_BYTE_SIZE).ToArray();

                byte[] data = encryptedData.Skip(_AES_BLOCK_BYTE_SIZE).ToArray();

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }
    }
}
