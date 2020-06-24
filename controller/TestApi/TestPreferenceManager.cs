using Newtonsoft.Json;
using SynthesisAPI.AssetManager;
using SynthesisAPI.PreferenceManager;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.Text;

namespace TestApi
{
    public static class TestPreferenceManager
    {
        private static (string, object)[] TestPrefs = new (string, object)[]
        {
            ("name", "Nikola Tesla"),
            ("age", 163),
            ("some_float", 1.5837f)
        };

        [JsonObject("CustomStruct")]
        public struct CustomStruct
        {
            [JsonProperty("Name")]
            public string Name { get; set; }
        }

        public static void TestSavingPreferences()
        {
            foreach (var pref in TestPrefs)
            {
                PreferenceManager.SetPreference(Program.TestGuid, pref.Item1, pref.Item2);
            }
            if (!PreferenceManager.Save()) throw new Exception();
        }

        public static void TestLoadingPreferences()
        {
            PreferenceManager.SetPreference(Program.TestGuid, "name", "Thomas Edison");
            if (!PreferenceManager.Load(overrideChanges: true)) throw new Exception();
            string name = PreferenceManager.GetPreference<string>(Program.TestGuid, TestPrefs[0].Item1);
            int age = PreferenceManager.GetPreference<int>(Program.TestGuid, TestPrefs[1].Item1);
            float someFloat = PreferenceManager.GetPreference<float>(Program.TestGuid, TestPrefs[2].Item1);

            if (!(name.Equals(TestPrefs[0].Item2) && age.Equals(TestPrefs[1].Item2) && someFloat.Equals(TestPrefs[2].Item2)))
            {
                throw new Exception("Test data doesn't match");
            }

            Console.WriteLine(string.Format("Name: {0}\nAge: {1}\nSome Float: {2}", name, age, someFloat));
        }

        public static void TestCustomTypes()
        {
            PreferenceManager.SetPreference(Program.TestGuid, "custom_type", new CustomStruct() { Name = "Edward Newton" });
            PreferenceManager.Save();
            PreferenceManager.Load(overrideChanges: true);
            object unCastableData = PreferenceManager.GetPreference(Program.TestGuid, "custom_type");
            CustomStruct data = JsonConvert.DeserializeObject<CustomStruct>(JsonConvert.SerializeObject(unCastableData));

            Console.WriteLine(string.Format("Name: {0}", data.Name));
        }

        public static void Test()
        {
            PreferenceManager.Load();
            TestSavingPreferences();
            TestLoadingPreferences();
            TestSavingPreferences();
            TestCustomTypes();
        }
    }
}