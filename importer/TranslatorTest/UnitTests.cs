using NUnit.Framework;
using Synthesis.Import;
using System;
using System.IO;
using NuGet.Frameworks;
using Synthesis.Proto;

namespace Synthesis.Test {
    public class UnitTests {

        private string BaseRobotPath;
        private string BaseFieldPath;

        [SetUp]
        public void Setup() {
            
            Logger.Init();
            
            BaseRobotPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.AltDirectorySeparatorChar
                    + "Autodesk" + Path.AltDirectorySeparatorChar + "Synthesis" + Path.AltDirectorySeparatorChar
                    + "Robots" + Path.AltDirectorySeparatorChar;
            BaseFieldPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.AltDirectorySeparatorChar
                    + "Autodesk" + Path.AltDirectorySeparatorChar + "Synthesis" + Path.AltDirectorySeparatorChar
                    + "Fields" + Path.AltDirectorySeparatorChar;
        }

        [Test]
        public void TranslateDozerTest() {
            Translator.Translate(BaseRobotPath + "Dozer", Translator.TranslationType.BXDJ_TO_PROTO_ROBOT);

            Assert.Pass();
        }
        
        [Test]
        public void TranslateMeanMachineTest() {
            Translator.Translate(BaseRobotPath + "2018 - 2471 Mean Machine", Translator.TranslationType.BXDJ_TO_PROTO_ROBOT);

            Assert.Pass();
        }

        [Test]
        public void TranslateAerialAssistTest() {
            Translator.Translate(BaseFieldPath + "2014 Aerial Assist", Translator.TranslationType.BXDF_TO_PROTO_FIELD);

            Assert.Pass();
        }
        
        [Test]
        public void TranslateDestinationDeepSpaceTest() {
            Translator.Translate(BaseFieldPath + "2019 Destination Deep Space", Translator.TranslationType.BXDF_TO_PROTO_FIELD);

            Assert.Pass();
        }

        [Test]
        public void Vec3Tests() {
            Vec3 a = new Vec3() { X = 3, Y = 4, Z = 0 };
            Assert.IsTrue(Math.Abs(a.Magnitude - 5f) < 0.005);
            a.Z = 7;
            a.Normalize();
            Assert.IsTrue(Math.Abs(a.Magnitude - 1f) < 0.005);
        }

        [Test]
        public void MaterialTests() {
            Material mat1 = new Material() {Red = 100, Green = 50};
            Material mat2 = new Material() {Red = 100, Green = 50};
            Material mat3 = new Material() {Red = 20, Blue = 80};
            
            Assert.IsTrue(mat1.Equals(mat2));
            Assert.IsFalse(mat1.Equals(mat3));
            Assert.IsTrue(mat1.GetHashCode() == mat2.GetHashCode());
            Assert.IsFalse(mat1.GetHashCode() == mat3.GetHashCode());
        }
    }
}