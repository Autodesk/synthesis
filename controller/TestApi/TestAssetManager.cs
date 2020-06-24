﻿using System;
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
            TextAsset testTxt = AssetManager.Import<TextAsset>("text/plain", "/modules", "test2.txt", Program.TestGuid,
                Permissions.PublicRead, "files/test.txt");

            TextAsset test = AssetManager.GetAsset<TextAsset>("/modules/test2.txt");

            Console.WriteLine(ReferenceEquals(testTxt, test));

            Console.WriteLine(testTxt?.ReadToEnd());
        }

        public static void TestXml()
        {
            // var test_xml = new XMLAsset("test.xml", Program.TestGuid, Permissions.PublicRead, "/modules");
            // test_xml.LoadAsset(File.ReadAllBytes(FileSystem.BasePath + "files/test.xml"));

            // byte[] file_data = File.ReadAllBytes(FileSystem.BasePath + "files/test.xml");
            // XMLAsset test_xml = AssetManager.Import<XMLAsset>("text/xml", file_data, "/modules", "test.xml", Program.TestGuid, Permissions.PublicRead, "files/test.xml");

            var testXml = AssetManager.Import<XmlAsset>("text/xml", "/modules", "test.xml", Program.TestGuid,
                Permissions.PublicRead, "files/test.xml");

            var test = AssetManager.GetAsset<XmlAsset>("/modules/test.xml");

            Console.WriteLine(ReferenceEquals(testXml, test));

            var obj = testXml?.Deserialize<TestXMLObject>();

            Console.WriteLine(obj?.Text);
        }

        public static void TestJson()
        {
            var testJson = AssetManager.Import<JsonAsset>("text/json", "/modules", "test.json", Program.TestGuid, Permissions.PublicRead, "files/test.json");

            var test = AssetManager.GetAsset<JsonAsset>("/modules/test.json");

            Console.WriteLine(ReferenceEquals(testJson, test));

            var obj = test?.Deserialize<TestJSONObject>();

            Console.WriteLine(obj?.Text);
        }

        public static void Test()
        {
            TestPlainText();
            TestXml();
            TestJson();
        }
    }
}
