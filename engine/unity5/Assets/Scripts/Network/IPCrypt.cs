//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Synthesis.Network
//{
//    /// <summary>
//    /// Used to encrypt an IP address into an 8 character hex code.
//    /// Based off the Python implementation: <seealso cref="https://github.com/veorq/ipcrypt"/>
//    /// </summary>
//    public static class IPCrypt
//    {
//        /// <summary>
//        /// Encrypts the given IP address with the supplied key.
//        /// </summary>
//        /// <param name="ip"></param>
//        /// <returns>An 8 digit hex code representing the encrypted IP address, or <see cref="string.Empty"/>
//        /// if the IP address is invalid.</returns>
//        public static string Encrypt(string ip)
//        {
//            byte[] state;
//            byte[] segment = new byte[4];
//            byte[] key = GetKey();
//            int code = 0;

//            try
//            {
//                state = ip.Split('.').Select(s => byte.Parse(s)).ToArray();
//            }
//            catch (FormatException)
//            {
//                return string.Empty;
//            }

//            if (state == null || state.Length != 4)
//                return string.Empty;

//            Array.Copy(key, 0, segment, 0, 4);
//            Xor4(state, segment);
//            PermuteFwd(state);
//            Array.Copy(key, 4, segment, 0, 4);
//            Xor4(state, segment);
//            PermuteFwd(state);
//            Array.Copy(key, 8, segment, 0, 4);
//            Xor4(state, segment);
//            PermuteFwd(state);
//            Array.Copy(key, 12, segment, 0, 4);
//            Xor4(state, segment);

//            for (int i = 0; i < 4; i++)
//                code += state[i] * Pow(256, (uint)(3 - i));

//            return code.ToString("X8");
//        }

//        /// <summary>
//        /// Decrypts the given code with the supplied key.
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="code"></param>
//        /// <returns>The decrypted IP address, or <see cref="string.Empty"/> if
//        /// The code supplied was invalid.</returns>
//        public static string Decrypt(string code)
//        {
//            if (code.Length != 8)
//                return string.Empty;

//            byte[] state = new byte[4];
//            byte[] segment = new byte[4];
//            byte[] key = GetKey();
            
//            try
//            {
//                for (int i = 0; i < state.Length; i++)
//                    state[i] = byte.Parse(code.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
//            }
//            catch (FormatException)
//            {
//                return string.Empty;
//            }

//            Array.Copy(key, 12, segment, 0, 4);
//            Xor4(state, segment);
//            PermuteBwd(state);
//            Array.Copy(key, 8, segment, 0, 4);
//            Xor4(state, segment);
//            PermuteBwd(state);
//            Array.Copy(key, 4, segment, 0, 4);
//            Xor4(state, segment);
//            PermuteBwd(state);
//            Array.Copy(key, 0, segment, 0, 4);
//            Xor4(state, segment);

//            return string.Join(".", state);
//        }

//        /// <summary>
//        /// Calculates and returns the encryption key.
//        /// </summary>
//        /// <returns></returns>
//        private static byte[] GetKey()
//        {
//            byte[] key = new byte[16];
//            new Random((DateTime.UtcNow.Date - new DateTime(1970, 1, 1)).Days).NextBytes(key);

//            return key;
//        }

//        /// <summary>
//        /// Calculates an integer raised to the given power.
//        /// </summary>
//        /// <param name="val"></param>
//        /// <param name="pow"></param>
//        /// <returns></returns>
//        private static int Pow(int val, uint pow)
//        {
//            int result = 1;

//            while (pow != 0)
//            {
//                if ((pow & 1) == 1)
//                    result *= val;

//                val *= val;
//                pow >>= 1;
//            }

//            return result;
//        }

//        /// <summary>
//        /// Performs a bitwise xor on a 4-byte array.
//        /// </summary>
//        /// <param name="x"></param>
//        /// <param name="y"></param>
//        private static void Xor4(byte[] x, byte[] y)
//        {
//            for (int i = 0; i < 4; i++)
//                x[i] ^= y[i];
//        }

//        /// <summary>
//        /// Rotates the given byte to the left.
//        /// </summary>
//        /// <param name="b"></param>
//        /// <param name="r"></param>
//        /// <returns></returns>
//        private static byte Rotl(byte b, byte r)
//        {
//            return (byte)(((b << r)) | (b >> (8 - r)));
//        }

//        /// <summary>
//        /// Permutes the given byte array forward.
//        /// </summary>
//        /// <param name="b"></param>
//        private static void PermuteFwd(byte[] b)
//        {
//            b[0] += b[1];
//            b[2] += b[3];
//            b[1] = Rotl(b[1], 2);
//            b[3] = Rotl(b[3], 5);
//            b[1] ^= b[0];
//            b[3] ^= b[2];
//            b[0] = Rotl(b[0], 4);
//            b[0] += b[3];
//            b[2] += b[1];
//            b[1] = Rotl(b[1], 3);
//            b[3] = Rotl(b[3], 7);
//            b[1] ^= b[2];
//            b[3] ^= b[0];
//            b[2] = Rotl(b[2], 4);
//        }

//        /// <summary>
//        /// Permutes the given byte array backward.
//        /// </summary>
//        /// <param name="b"></param>
//        private static void PermuteBwd(byte[] b)
//        {
//            b[2] = Rotl(b[2], 4);
//            b[1] ^= b[2];
//            b[3] ^= b[0];
//            b[1] = Rotl(b[1], 5);
//            b[3] = Rotl(b[3], 1);
//            b[0] -= b[3];
//            b[2] -= b[1];
//            b[0] = Rotl(b[0], 4);
//            b[1] ^= b[0];
//            b[3] ^= b[2];
//            b[1] = Rotl(b[1], 6);
//            b[3] = Rotl(b[3], 3);
//            b[0] -= b[1];
//            b[2] -= b[3];
//        }
//    }
//}
