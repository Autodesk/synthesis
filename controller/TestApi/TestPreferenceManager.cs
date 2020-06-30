using Newtonsoft.Json;
using SynthesisAPI.PreferenceManager;
using System;
using NUnit.Framework;

namespace TestApi
{
    [TestFixture]
    public static class TestPreferenceManager
    {
        [JsonObject("CustomStruct")]
        public struct CustomStruct
        {
            [JsonProperty("Name")]
            public string Name { get; set; }
        }

        [Test]
        public static void TestSavingLoading()
        {
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
        public static void TestCustomTypeSavingLoading()
        {
            CustomStruct customOriginal = new CustomStruct() { Name = "Thomas Edison" };
            PreferenceManager.SetPreference(Program.TestGuid, "custom_type", customOriginal);
            if (!PreferenceManager.Save()) throw new Exception();
            if (!PreferenceManager.Load()) throw new Exception();
            CustomStruct customCopy = PreferenceManager.GetPreference<CustomStruct>(Program.TestGuid, "custom_type", useJsonDeserialization: true);

            Assert.AreEqual(customOriginal, customCopy);
        }
    }
}