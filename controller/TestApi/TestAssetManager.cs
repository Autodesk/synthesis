using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.AssetManager;
using SynthesisAPI.VirtualFileSystem;
using NUnit.Framework;

namespace TestApi
{
    [TestFixture]
    public static class TestAssetManager
    {
        [SetUp]
        public static void Init()
        {
            if (!System.IO.Directory.Exists(FileSystem.TestPath))
            {
                System.IO.Directory.CreateDirectory(FileSystem.TestPath);
            }

            string text_string = "Hello World!";

            if (!File.Exists(FileSystem.TestPath + "test.txt"))
            {
                File.WriteAllText(FileSystem.TestPath + "test.txt", text_string);
            }

            string json_string = "{\n\t\"Text\": \"Hello world of JSON!\"\n}";

            if (!File.Exists(FileSystem.TestPath + "test.json"))
            {
                File.WriteAllText(FileSystem.TestPath + "test.json", json_string);
            }

            string xml_string = "<TestXMLObject>\n\t<Text>Hello world of XML!</Text>\n</TestXMLObject>";

            if (!File.Exists(FileSystem.TestPath + "test.xml"))
            {
                File.WriteAllText(FileSystem.TestPath + "test.xml", xml_string);
            }
        }

        [Test]
        public static void TestPlainText()
        {
            TextAsset testTxt = AssetManager.Import<TextAsset>("text/plain", "/temp", "test2.txt", Program.TestGuid, Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.txt");

            TextAsset test = AssetManager.GetAsset<TextAsset>("/temp/test2.txt");

            Assert.AreSame(testTxt, test);

            Console.WriteLine(testTxt?.ReadToEnd());
        }

        [Test]
        public static void TestXml()
        {
            // byte[] file_data = File.ReadAllBytes(FileSystem.BasePath + "test.xml");
            // XmlAsset test_xml = AssetManager.Import<XmlAsset>("text/xml", file_data, "/temp", "test.xml", Program.TestGuid, Permissions.PublicRead, "test.xml");

            var testXml = AssetManager.Import<XmlAsset>("text/xml", "/temp", "test.xml", Program.TestGuid, Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.xml");

            var test = AssetManager.GetAsset<XmlAsset>("/temp/test.xml");

            Assert.AreSame(testXml, test);

            var obj = testXml?.Deserialize<TestXMLObject>();

            Console.WriteLine(obj?.Text);
        }

        [Test]
        public static void TestJson()
        {
            var testJson = AssetManager.Import<JsonAsset>("text/json", "/temp", "test.json", Program.TestGuid, Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.json");

            var test = AssetManager.GetAsset<JsonAsset>("/temp/test.json");

            Assert.AreSame(testJson, test);

            var obj = test?.Deserialize<TestJSONObject>();

            Console.WriteLine(obj?.Text);
        }

        [Test]
        public static void TestTypeFromFileExtension()
        {
            string source = $"test{Path.DirectorySeparatorChar}test.json";
            var testJson = AssetManager.Import("/temp", "test2.json", Program.TestGuid, Permissions.PublicReadWrite, source);

            var test = AssetManager.GetAsset<JsonAsset>("/temp/test2.json");

            Assert.AreSame(testJson, test);

            var obj = test?.Deserialize<TestJSONObject>();

            Console.WriteLine(obj?.Text);
        }

        [Test]
        public static void TestTypeFromTypeParameter()
        {
            var testJson = AssetManager.Import<JsonAsset>("/temp", "test3.json", Program.TestGuid, Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.json");

            var test = AssetManager.GetAsset<JsonAsset>("/temp/test3.json");

            Assert.AreSame(testJson, test);

            var obj = test?.Deserialize<TestJSONObject>();

            Console.WriteLine(obj?.Text);
        }
    }
}
