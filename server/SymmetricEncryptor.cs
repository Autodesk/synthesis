using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SynthesisServer
{
    class SymmetricEncryptor
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
