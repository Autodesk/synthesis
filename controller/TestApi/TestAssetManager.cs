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
            TextAsset test_txt = AssetManager.Import<TextAsset>("text/plain", "/modules", "test2.txt", Program.TestGuid, Permissions.PublicRead, "files/test.txt");

            TextAsset test = AssetManager.GetAsset<TextAsset>("/modules/test2.txt");

            Console.WriteLine(ReferenceEquals(test_txt, test));

            Console.WriteLine(test_txt.SharedStream.ReadToEnd());
        }

        public static void TestXML()
        {
            // var test_xml = new XMLAsset("test.xml", Program.TestGuid, Permissions.PublicRead, "/modules");
            // test_xml.LoadAsset(File.ReadAllBytes(FileSystem.BasePath + "files/test.xml"));

            // byte[] file_data = File.ReadAllBytes(FileSystem.BasePath + "files/test.xml");
            // XMLAsset test_xml = AssetManager.Import<XMLAsset>("text/xml", file_data, "/modules", "test.xml", Program.TestGuid, Permissions.PublicRead, "files/test.xml");

            XMLAsset test_xml = AssetManager.Import<XMLAsset>("text/xml", "/modules", "test.xml", Program.TestGuid, Permissions.PublicRead, "files/test.xml");

            XMLAsset test = AssetManager.GetAsset<XMLAsset>("/modules/test.xml");

            Console.WriteLine(ReferenceEquals(test_xml, test));

            TestXMLObject obj = test_xml.Deserialize<TestXMLObject>();

            Console.WriteLine(obj.Text);
        }

        public static void TestJSON()
        {
            JSONAsset test_json = AssetManager.Import<JSONAsset>("text/json", "/modules", "test.json", Program.TestGuid, Permissions.PublicRead, "files/test.json");

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
