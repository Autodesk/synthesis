using Newtonsoft.Json;
using SynthesisAPI.AssetManager;
using SynthesisAPI.PreferenceManager;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.Text;
using NUnit.Framework;

namespace TestApi
{
    [TestFixture]
    public class TestPreferenceManager
    {
        [JsonObject("CustomStruct")]
        public struct CustomStruct
        {
            [JsonProperty("Name")]
            public string Name { get; set; }
        }

        [Test]
        public void TestSavingLoading()
        {
            if (FileSystem.RootNode == null) FileSystem.Init();

            PreferenceManager.SetPreference(Program.TestGuid, "name", "Nikola Tesla");
            PreferenceManager.SetPreference(Program.TestGuid, "age", 163);
            if (!PreferenceManager.Save()) throw new Exception();
            if (!PreferenceManager.Load()) throw new Exception();
            string name = PreferenceManager.GetPreference<string>(Program.TestGuid, "name");
            int age = PreferenceManager.GetPreference<int>(Program.TestGuid, "age");

            Assert.AreEqual(name, "Nikola Tesla");
            Assert.AreEqual(age, 163);
        }

        [Test]
        public void TestCustomTypeSavingLoading()
        {
            if (FileSystem.RootNode == null) FileSystem.Init();

            CustomStruct customOriginal = new CustomStruct() { Name = "Thomas Edison" };
            PreferenceManager.SetPreference(Program.TestGuid, "custom_type", customOriginal);
            if (!PreferenceManager.Save()) throw new Exception();
            if (!PreferenceManager.Load()) throw new Exception();
            CustomStruct customCopy = PreferenceManager.GetPreference<CustomStruct>(Program.TestGuid, "custom_type", useJsonReserialization: true);

            Assert.AreEqual(customOriginal, customCopy);
        }
    }
}