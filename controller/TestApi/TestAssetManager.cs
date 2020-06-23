using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.AssetManager;
using SynthesisAPI.VirtualFileSystem;

namespace TestApi
{
    public static class TestAssetManager
    {
        public static void TestPlainText()
        {
            byte[] file_data = File.ReadAllBytes(FileSystem.BasePath + "files/test.txt");
            PlainTextAsset test_txt = AssetManager.Import<PlainTextAsset>("text/plain", "/modules", file_data, "test2.txt", Program.TestGuid, Permissions.PublicRead);

            PlainTextAsset test = AssetManager.GetAsset<PlainTextAsset>("/modules/test2.txt");

            Console.WriteLine(ReferenceEquals(test_txt, test));

            var reader = test_txt.CreateReader();

            Console.WriteLine(reader.ReadToEnd());
        }

        public static void TestXML()
        {
            // var test_xml = new XMLAsset("test.xml", Program.TestGuid, Permissions.PublicRead, "/modules");
            // test_xml.LoadAsset(File.ReadAllBytes(FileSystem.BasePath + "files/test.xml"));

            byte[] file_data = File.ReadAllBytes(FileSystem.BasePath + "files/test.xml");
            XMLAsset test_xml = AssetManager.Import<XMLAsset>("text/xml", "/modules", file_data, "test.xml", Program.TestGuid, Permissions.PublicRead);

            XMLAsset test = AssetManager.GetAsset<XMLAsset>("/modules/test.xml");

            Console.WriteLine(ReferenceEquals(test_xml, test));

            TestXMLObject obj = test_xml.Deserialize<TestXMLObject>();

            Console.WriteLine(obj.Text);
        }

        public static void TestJSON()
        {
            byte[] file_data = File.ReadAllBytes(FileSystem.BasePath + "files/test.json");
            JSONAsset test_json = AssetManager.Import<JSONAsset>("text/json", "/modules", file_data, "test.json", Program.TestGuid, Permissions.PublicRead);

            JSONAsset test = AssetManager.GetAsset<JSONAsset>("/modules/test.json");

            Console.WriteLine(ReferenceEquals(test_json, test));

            TestJSONObject obj = test.Deserialize<TestJSONObject>();

            Console.WriteLine(obj.Text);
        }

        public static void Test()
        {
            TestPlainText();
            TestXML();
            TestJSON();
        }
    }
}
