using Newtonsoft.Json;
using SynthesisAPI.PreferenceManager;
using System;
using NUnit.Framework;
using System.Threading.Tasks;

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
            PreferenceManager.SetPreference(Program.TestModuleName, "name", "Nikola Tesla");
            PreferenceManager.SetPreference(Program.TestModuleName, "age", 163);

            if (!PreferenceManager.Save()) throw new Exception();
            if (!PreferenceManager.Load()) throw new Exception();
            
            string name = PreferenceManager.GetPreference<string>(Program.TestModuleName, "name");
            int age = PreferenceManager.GetPreference<int>(Program.TestModuleName, "age");

            Assert.AreEqual(name, "Nikola Tesla");
            Assert.AreEqual(age, 163);
        }

        [Test]
        public static void TestAsyncSaveLoad()
        {
            PreferenceManager.SetPreference(Program.TestModuleName, "name", "Nikola Tesla");
            PreferenceManager.SetPreference(Program.TestModuleName, "age", 163);

            Task<bool> saveTask = PreferenceManager.SaveAsync();
            if (!saveTask.Wait(1000)) throw new Exception();

            Task<bool> loadTask = PreferenceManager.LoadAsync();
            if (!loadTask.Wait(1000)) throw new Exception();

            string name = PreferenceManager.GetPreference<string>(Program.TestModuleName, "name");
            int age = PreferenceManager.GetPreference<int>(Program.TestModuleName, "age");

            Assert.AreEqual(name, "Nikola Tesla");
            Assert.AreEqual(age, 163);
        }

        [Test]
        public static void TestCustomTypeSavingLoading()
        {
            CustomStruct customOriginal = new CustomStruct() { Name = "Thomas Edison" };
            PreferenceManager.SetPreference(Program.TestModuleName, "custom_type", customOriginal);

            if (!PreferenceManager.Save()) throw new Exception();
            if (!PreferenceManager.Load()) throw new Exception();

            CustomStruct customCopy = PreferenceManager.GetPreference<CustomStruct>(Program.TestModuleName, "custom_type");

            Assert.AreEqual(customOriginal, customCopy);
        }
    }
}