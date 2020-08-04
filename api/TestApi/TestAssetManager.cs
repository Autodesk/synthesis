using System;
using System.IO;
using SynthesisAPI.AssetManager;
using SynthesisAPI.VirtualFileSystem;
using NUnit.Framework;
using SynthesisAPI.EventBus;
using System.Collections.Generic;
using SynthesisAPI.Modules.Attributes;

namespace TestApi
{
    [TestFixture]
    public static class TestAssetManager
    {

        [OneTimeSetUp]
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

            string xml_string = "<TestXmlObject>\n\t<Text>Hello world of XML!</Text>\n</TestXmlObject>";

            if (!File.Exists(FileSystem.TestPath + "test.xml"))
            {
                File.WriteAllText(FileSystem.TestPath + "test.xml", xml_string);
            }
        }

        [OneTimeTearDown]
        public static void Teardown()
        {
            if (System.IO.Directory.Exists(FileSystem.TestPath))
            {
                System.IO.Directory.Delete(FileSystem.TestPath, true);
            }
        }

        [Test]
        public static void TestPlainText()
        {
            TextAsset testTxt = AssetManager.Import<TextAsset>("text/plain", false, "/temp", "test2.txt", Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.txt");

            TextAsset test = AssetManager.GetAsset<TextAsset>("/temp/test2.txt");

            Assert.AreSame(testTxt, test);

            Assert.AreEqual("Hello World!", testTxt?.ReadToEnd());
        }

        [Test]
        public static void TestXml()
        {
            var testXml = AssetManager.Import<XmlAsset>("text/xml", false, "/temp", "test.xml", Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.xml");

            var test = AssetManager.GetAsset<XmlAsset>("/temp/test.xml");

            Assert.AreSame(testXml, test);

            var obj = testXml?.Deserialize<TestXmlObject>();

            Assert.AreEqual("Hello world of XML!", obj?.Text);
        }

        [Test]
        public static void TestJson()
        {
            var testJson = AssetManager.Import<JsonAsset>("text/json", false, "/temp", "test.json", Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.json");

            var test = AssetManager.GetAsset<JsonAsset>("/temp/test.json");

            Assert.AreSame(testJson, test);

            var obj = test?.Deserialize<TestJsonObject>();

            Assert.AreEqual("Hello world of JSON!", obj?.Text);
        }

        [Test]
        public static void TestLazyImport()
        {
            var testJson = AssetManager.ImportLazy("text/json", false, "/temp", "test_lazy.json", Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.json");

            Assert.NotNull(testJson);

            var testLazy = AssetManager.GetAsset("/temp/test_lazy.json");

            Assert.True(testLazy is LazyAsset);

            ((LazyAsset)testLazy).Load();

            var testContents = AssetManager.GetAsset("/temp/test_lazy.json");

            Assert.True(testContents is JsonAsset);

            var obj = ((JsonAsset)testContents).Deserialize<TestJsonObject>();

            Assert.AreEqual("Hello world of JSON!", obj?.Text);
        }

        [Test]
        public static void TestTypeFromFileExtension()
        {
            string source = $"test{Path.DirectorySeparatorChar}test.json";
            string type = AssetManager.GetTypeFromFileExtension(source);
            var testJson = AssetManager.Import(type, false, "/temp", "test2.json", Permissions.PublicReadWrite, source);

            var test = AssetManager.GetAsset<JsonAsset>("/temp/test2.json");

            Assert.AreSame(testJson, test);
        }


        [Test]
        public static void TestCreatePathWithImport()
        {
            var testJson = AssetManager.Import<JsonAsset>("text/json", false, "/modules/module1", "test.json", Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.json");

            var test = AssetManager.GetAsset<JsonAsset>("/modules/module1/test.json");

            Assert.AreSame(testJson, test);
        }

        [ModuleExport]
        private class AssetImportSubscriber
        {
            static public List<AssetImportEvent> Events { get; private set; } = new List<AssetImportEvent>();

            [TaggedCallback(AssetImportEvent.Tag)]
            public static void Handler(AssetImportEvent e)
            {
                Events.Add(e);
            }
        }

        [Test]
        public static void TestAssetImportEvent()
        {
            string location = "/temp";
            string name = "test3.txt";
            string type = "text/plain";

            AssetManager.Import(type, false, location, name, Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.txt");

            Assert.AreEqual(1, AssetImportSubscriber.Events.Count);
            Assert.AreEqual(3, AssetImportSubscriber.Events[0].GetArguments().Length);
            Assert.AreEqual(name, AssetImportSubscriber.Events[0].GetArguments()[0]);
            Assert.AreEqual(location, AssetImportSubscriber.Events[0].GetArguments()[1]);
            Assert.AreEqual(type, AssetImportSubscriber.Events[0].GetArguments()[2]);

            EventBus.ResetAllListeners();
        }

        // TODO test GLTFAsset
    }
}
