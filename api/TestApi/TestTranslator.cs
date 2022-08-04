//using NUnit.Framework;
//using System;
//using System.IO;
//using System.Text;

//namespace TestApi {
//    [TestFixture]
//    public static class TestTranslator {

//        private static string BaseRobotPath;
//        private static string BaseFieldPath;

//        [OneTimeSetUp]
//        public static void Setup() {
            
//            BaseRobotPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.AltDirectorySeparatorChar
//                    + "Autodesk" + Path.AltDirectorySeparatorChar + "Synthesis" + Path.AltDirectorySeparatorChar
//                    + "Robots" + Path.AltDirectorySeparatorChar;
//            BaseFieldPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.AltDirectorySeparatorChar
//                    + "Autodesk" + Path.AltDirectorySeparatorChar + "Synthesis" + Path.AltDirectorySeparatorChar
//                    + "Fields" + Path.AltDirectorySeparatorChar;
//        }

//        [Test]
//        public static void TranslateDozerTest() {
//            string path = BaseRobotPath + "Dozer";
//            if (!File.Exists(path)) {
//                Assert.Warn("File doesn't exist");
//                return;
//            }

//            Translator.Translate(path, Translator.TranslationType.BXDJ_TO_PROTO_ROBOT);

//            Assert.Pass();
//        }
        
//        [Test]
//        public static void TranslateMeanMachineTest() {
//            string path = BaseRobotPath + "2018 - 2471 Mean Machine";
//            if (!File.Exists(path)) {
//                Assert.Warn("File doesn't exist");
//                return;
//            }
            
//            Translator.Translate(path, Translator.TranslationType.BXDJ_TO_PROTO_ROBOT);

//            Assert.Pass();
//        }

//        [Test]
//        public static void TranslateAerialAssistTest() {
//            string path = BaseFieldPath + "2014 Aerial Assist";
//            if (!File.Exists(path)) {
//                Assert.Warn("File doesn't exist");
//                return;
//            }
            
//            Translator.Translate(path, Translator.TranslationType.BXDF_TO_PROTO_FIELD);

//            Assert.Pass();
//        }
        
//        [Test]
//        public static void TranslateDestinationDeepSpaceTest() {
//            string path = BaseFieldPath + "2019 Destination Deep Space";
//            if (!File.Exists(path)) {
//                Assert.Warn("File doesn't exist");
//                return;
//            }
            
//            Translator.Translate(path, Translator.TranslationType.BXDF_TO_PROTO_FIELD);

//            Assert.Pass();
//        }

//        [Test]
//        public static void Vec3Tests() {
//            Vec3 a = new Vec3() { X = 3, Y = 4, Z = 0 };
//            Assert.IsTrue(Math.Abs(a.Magnitude - 5f) < 0.005);
//            a.Z = 7;
//            a.Normalize();
//            Assert.IsTrue(Math.Abs(a.Magnitude - 1f) < 0.005);
//        }

//        [Test]
//        public static void MaterialTests() {
//            Material mat1 = new Material() {Red = 100, Green = 50};
//            Material mat2 = new Material() {Red = 100, Green = 50};
//            Material mat3 = new Material() {Red = 20, Blue = 80};
            
//            Assert.IsTrue(mat1.Equals(mat2));
//            Assert.IsFalse(mat1.Equals(mat3));
//            Assert.IsTrue(mat1.GetHashCode() == mat2.GetHashCode());
//            Assert.IsFalse(mat1.GetHashCode() == mat3.GetHashCode());
//        }

//        [Test]
//        public static void ByteToHexTest() {
//            byte[] buf = {
//                0x00,
//                0xa5,
//                0x0f,
//                0x9e
//            };

//            string singleByte = buf[1].ToHexString();
//            string fullBuffer = buf.ToHexString();
//            char nibble = buf[2].ToHexCharacter();
            
//            Assert.IsTrue(singleByte == "a5");
//            Assert.IsTrue(fullBuffer == "00a50f9e");
//            Assert.IsTrue(nibble == 'f');
//        }

//        [Test]
//        public static void TempFileHashTest() {
//            byte[] buf = Encoding.ASCII.GetBytes("Hello World!");
//            Assert.IsTrue(Translator.TempFileHash(buf).ToHexString() == "7f83b1657ff1fc53b92dc18148a1d65dfc2d4b1fa3d677284addd200126d9069");
//        }
//    }
//}