using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Synthesis.PreferenceManager
{
    public static class PreferenceManager
    {
        public static bool UnsavedChanges { get; private set; }
        private static Dictionary<string, object> _preferences = new Dictionary<string, object>();
        private static string FilePath = Path.Combine(FileSystem.FileSystem.Preferences, "preferences.json");

        public static void Load()
        {
            if (!File.Exists(FilePath))
                return;
            string data = File.ReadAllText(FilePath);
            _preferences = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
        }

        public static void Save()
        {
            if(UnsavedChanges)
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(_preferences));
            UnsavedChanges = false;
        }

        public static void SetPreference<T>(string key, T value)
        {
            _preferences[key] = value;
            UnsavedChanges = true;
        }

        public static object GetPreference(string key)
        {
            return ContainsPreference(key) ? _preferences[key] : null;
        }

        public static T GetPreference<T>(string key)
        {
            return ContainsPreference(key) ? (T) Convert.ChangeType(_preferences[key], typeof(T)) : default;
        }

        public static bool ContainsPreference(string key)
        {
            return _preferences.ContainsKey(key);
        }

        public static void ClearPreferences()
        {
            _preferences.Clear();
            UnsavedChanges = true;
        }
    }
}