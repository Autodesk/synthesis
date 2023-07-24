using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SynthesisAPI.Utilities;
using System.Linq;

namespace TestApi {

    [TestFixture]
    public static class TestByteStream {

        [Test]
        public static void TestKiloByteStream() {
            // Test
            byte[] kiloByte = new byte[1000];
            for (int i = 0; i < kiloByte.Length; i++) {
                kiloByte[i] = (byte)(i & 0x000000FF);
            }

            var stream = new ByteStream(kiloByte.AsEnumerable().GetEnumerator(), 1000);
            byte[] copy = new byte[1000];
            int readBytes = stream.Read(copy, 0, 1000);
            
            // Verification
            Assert.AreEqual(1000, readBytes);
            Assert.AreEqual(1000, stream.Length);
            for (int i = 0; i < 1000; i++) {
                Assert.AreEqual(kiloByte[i], copy[i]);
            }
        }

        [Test]
        public static void TestMegaByteStream() {
            // Test
            byte[] kiloByte = new byte[1000000];
            for (int i = 0; i < kiloByte.Length; i++) {
                kiloByte[i] = (byte)(i & 0x000000FF);
            }
            var stream = new ByteStream(kiloByte.AsEnumerable().GetEnumerator(), 1000000);
            byte[] copy = new byte[1000000];
            int readBytes = stream.Read(copy, 0, 1000000);
            
            // Verification
            Assert.AreEqual(1000000, readBytes);
            Assert.AreEqual(1000000, stream.Length);
            for (int i = 0; i < 1000000; i++) {
                Assert.AreEqual(kiloByte[i], copy[i]);
            }
        }

        [Test]
        public static void TestPartialKiloByteStream() {
            // Test
            byte[] kiloByte = new byte[1000];
            for (int i = 0; i < kiloByte.Length; i++) {
                kiloByte[i] = (byte)(i & 0x000000FF);
            }
            var stream = new ByteStream(kiloByte.AsEnumerable().GetEnumerator(), 1000);
            byte[] firstHalf = new byte[550];
            byte[] secondHalf = new byte[450];
            int readFirstBytes = stream.Read(firstHalf, 0, 550);
            int readSecondBytes = stream.Read(secondHalf, 0, 450);
            
            // Verification
            Assert.AreEqual(550, readFirstBytes);
            Assert.AreEqual(450, readSecondBytes);
            Assert.AreEqual(1000, stream.Length);
            for (int i = 0; i < 550; i++) {
                Assert.AreEqual(kiloByte[i], firstHalf[i]);
            }
            for (int i = 0; i < 450; i++) {
                Assert.AreEqual(kiloByte[i + 550], secondHalf[i]);
            }
        }

        [Test]
        public static void TestOffsetStream() {
            // Test
            byte[] kiloByte = new byte[1000];
            for (int i = 0; i < kiloByte.Length; i++) {
                kiloByte[i] = (byte)(i & 0x000000FD);
            }
            var stream = new ByteStream(kiloByte.AsEnumerable().GetEnumerator(), 1000);
            byte[] copy = new byte[1000];
            for (int i = 0; i < 1000; i++) {
                copy[i] = 2;
            }
            int readBytes = stream.Read(copy, 100, 200);
            
            // Verification
            Assert.AreEqual(200, readBytes);
            Assert.AreEqual(1000, stream.Length);
            for (int i = 0; i < 1000; i++) {
                if (i < 100 || i >= 300) {
                    Assert.AreEqual(2, copy[i]);
                } else {
                    Assert.AreEqual(kiloByte[i - 100], copy[i]);
                }
            }
        }

        [Test]
        public static void TestSha256() {
            byte[] buff = new byte[20];
            for (int i = 0; i < 20; i++) {
                buff[i] = (byte)'a';
            }

            SHA256 sha = SHA256.Create();
            string original = Convert.ToBase64String(sha.ComputeHash(buff));
            string copy = Convert.ToBase64String(sha.ComputeHash(new ByteStream(buff.AsEnumerable().GetEnumerator(), 20)));

            Assert.AreEqual(original, copy);
        }

    }

}