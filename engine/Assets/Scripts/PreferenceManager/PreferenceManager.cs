using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SynthesisAPI.EventBus;

namespace Synthesis.PreferenceManager {
    public static class PreferenceManager {

        // Prevents Devs and Users from trying to load corruptted or no longer supported preference formats.
        internal const string COHERENCE_ID = "ppqwokopqkwkshjhcwhuyre";

        internal static PreferenceData _data;
        public static bool UnsavedChanges { get; private set; }
        private static Dictionary<string, object> _preferences = new Dictionary<string, object>();
        public static bool AnyPrefs => _preferences.Keys.Count > 0;
        private static string FilePath = Path.Combine(FileSystem.FileSystem.Preferences, "preferences.json");

        public static void Load() {
            if (!File.Exists(FilePath))
                return;

            try {
                string data = File.ReadAllText(FilePath);
                _data = JsonConvert.DeserializeObject<PreferenceData>(data);
                if (_data.CoherenceId != COHERENCE_ID)
                    return;
                _preferences = _data.BulkData != null ? _data.BulkData : _preferences;
            } catch (Exception) { }
        }

        public static void Save() {
            EventBus.Push(new PrePreferenceSaveEvent());
            if(UnsavedChanges) {
                _data.BulkData = _preferences;
                _data.CoherenceId = COHERENCE_ID;
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(_data));
            }
            UnsavedChanges = false;
        }

        public static void SetPreferenceObject(string key, object value) {
            _preferences[key] = value;
            UnsavedChanges = true;
        }

        public static void SetPreference<T>(string key, T value) {
            _preferences[key] = JsonConvert.SerializeObject(value);
            UnsavedChanges = true;
        }

        public static object GetPreferenceObject(string key) {
            return ContainsPreference(key) ? _preferences[key] : null;
        }

        public static T GetPreference<T>(string key) {
            return ContainsPreference(key) ? JsonConvert.DeserializeObject<T>((string)_preferences[key]) : default;
        }

        public static bool ContainsPreference(string key) {
            return _preferences.ContainsKey(key);
        }

        public static void ClearPreferences()
        {
            _preferences.Clear();
            UnsavedChanges = true;
        }
    }

    public class PrePreferenceSaveEvent : IEvent { }

    internal struct PreferenceData {
        public string CoherenceId;
        public Dictionary<string, object> BulkData;
    }
}