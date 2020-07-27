﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using SynthesisAPI.AssetManager;
using SynthesisAPI.VirtualFileSystem;
using System.Threading.Tasks;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.PreferenceManager
{
    public static class PreferenceManager
    {
        private static readonly (string Path, string Name) VirtualFilePath = ("/modules", "preferences.json");

        #region Accessing Preferences

        /// <summary>
        /// Set a preference using an identifier inside your unique dictionary
        /// </summary>
        /// <typeparam name="TValueType">Type of preference. No constraints</typeparam>
        /// <param name="moduleName">name of the owner module</param>
        /// <param name="key">Identifier for preference</param>
        /// <param name="value">Preference value</param>
        [ExposedApi]
        public static void SetPreference<TValueType>(string moduleName, string key, TValueType value)
        {
            if (!Instance.Preferences.ContainsKey(moduleName))
                Instance.Preferences[moduleName] = new Dictionary<string, object>();
            Instance.Preferences[moduleName][key] = value!;
            _changesSaved = false;
        }

        /// <summary>
        /// This function will return a specific preference that has loaded in and/or
        /// set using the <see cref="SetPreference{TValueType}(string, string, TValueType)"/> method
        /// </summary>
        /// <param name="moduleName">name of the owner module</param>
        /// <param name="key">Identifier for preference</param>
        /// <returns>Preference</returns>
        [ExposedApi]
        public static object GetPreference(string moduleName, string key)
        {
            if (Instance.Preferences.ContainsKey(moduleName))
            {
                if (Instance.Preferences[moduleName].ContainsKey(key))
                    return Instance.Preferences[moduleName][key];
                throw new ArgumentException($"There is no key of value \"{key}\" for module \"{moduleName}\"");
            }
            throw new ArgumentException($"There is no module with name \"{moduleName}\"");
        }

        /// <summary>
        /// This function will return a specific preference that has loaded in and/or
        /// set using the <see cref="SetPreference{TValueType}(string, string, TValueType)"/> method
        /// </summary>
        /// <typeparam name="TValueType">Return type. If useJsonReserialization is true this type must
        /// have a <see cref="JsonObjectAttribute">JsonObjectAttribute</see></typeparam>
        /// <param name="moduleName">name of the owner module</param>
        /// <param name="key">Identifier for preference</param>
        /// <param name="useJsonDeserialization">Set this to true if you are trying to retrieve an object of a custom type. Be sure
        /// to label everything inside the type with the Attributes Newtonsoft provides</param>
        /// <returns>Preference</returns>
        [ExposedApi]
        public static TValueType GetPreference<TValueType>(string moduleName, string key, bool useJsonDeserialization = false)
        {
            if (Instance.Preferences.ContainsKey(moduleName))
            {
                if (Instance.Preferences[moduleName].ContainsKey(key))
                {
                    if (useJsonDeserialization)
                    {
                        if (!typeof(TValueType).IsDefined(typeof(JsonObjectAttribute), false))
                            throw new ArgumentException(
                                $"Type \"{typeof(TValueType).FullName}\" does not have the Newtonsoft.Json.JsonObjectAttribute");

                        return JsonConvert.DeserializeObject<TValueType>(JsonConvert.SerializeObject(Instance.Preferences[moduleName][key]));
                    }
                    return (TValueType) Convert.ChangeType(Instance.Preferences[moduleName][key], typeof(TValueType));
                }
                throw new ArgumentException($"There is no key of value \"{key}\" for module \"{moduleName}\"");
            }
            throw new ArgumentException($"There is no module with name \"{moduleName}\"");
        }

        /// <summary>
        /// Clear your dictionary of preferences
        /// </summary>
        /// <param name="owner">Your module name</param>
        [ExposedApi]
        public static void ClearPreferences(string moduleName)
        {
            Instance.Preferences[moduleName] = new Dictionary<string, object>();
            _changesSaved = false;
        }

        #endregion

        #region IO

        /// <summary>
        /// Loads a JSON file asynchronously and loads preference data
        /// </summary>
        /// <param name="overrideChanges">Load regardless of unsaved data</param>
        /// <returns>Whether or not the load executed successfully</returns>
        [ExposedApi]
        public static Task<bool> LoadAsync(bool overrideChanges = false) => Task<bool>.Factory.StartNew(() => Load(overrideChanges));

        private static void ImportPreferencesAsset()
        {
            using var _ = ApiCallSource.ForceInternalCall();
            Instance.ImportPreferencesAsset();
        }

        /// <summary>
        /// Loads a JSON file that stores preference data
        /// </summary>
        /// <param name="overrideChanges">Load regardless of unsaved data</param>
        /// <returns>Whether or not the load executed successfully</returns>
        [ExposedApi]
        public static bool Load(bool overrideChanges = false)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return LoadInner(overrideChanges);
        }

        internal static bool LoadInner(bool overrideChanges = false)
        {
            if (!overrideChanges && !_changesSaved)
                return false;

            var deserialized =
                Instance.Asset?.DeserializeInner<Dictionary<string, Dictionary<string, object>>>(offset: 0,
                    retainPosition: true);
            Instance.Preferences =
                deserialized ?? new Dictionary<string, Dictionary<string, object>>(); // Failed to load; reset to default

            _changesSaved = true;

            EventBus.EventBus.Push("prefs/io",
                new PreferencesIOEvent(PreferencesIOEvent.Status.PostLoad));
            return true;
        }

        /// <summary>
        /// Saves a JSON file with preference data asynchronously
        /// </summary>
        /// <returns>Whether or not the save executed successfully</returns>
        [ExposedApi]
        public static Task<bool> SaveAsync() => Task<bool>.Factory.StartNew(Save);

        /// <summary>
        /// Saves a JSON file with preference data
        /// </summary>
        /// <returns>Whether or not the save executed successfully</returns>
        [ExposedApi]
        public static bool Save()
        {
            using var _ = ApiCallSource.StartExternalCall();
            return SaveInner();
        }

        internal static bool SaveInner()
        {
            ImportPreferencesAsset();
            
            EventBus.EventBus.Push("prefs/io",
                new PreferencesIOEvent(PreferencesIOEvent.Status.PreSave));

            Instance.Asset?.SerializeInner(Instance.Preferences);
            Instance.Asset?.SaveToFileInner();

            _changesSaved = true;

            return true;
        }

        #endregion

        private static bool _changesSaved;

        private class Inner
        {
            public readonly object InstanceLock = new object();

            public Dictionary<string, Dictionary<string, object>> Preferences;

            private JsonAsset _asset;
            public JsonAsset? Asset
            {
                get => _asset;
            }

            private Inner()
            {
                Preferences = new Dictionary<string, Dictionary<string, object>>();
            }

            public void ImportPreferencesAsset()
            {
                if (_asset == null)
                {
                    _asset = AssetManager.AssetManager.Import<JsonAsset>("text/json", true,
                        VirtualFilePath.Path, VirtualFilePath.Name,
                        Permissions.PublicReadWrite, VirtualFilePath.Name)!;
                    if (_asset == null)
                    {
                        throw new Exception("Failed to create preferences.json");
                    }
                }
            }

            private static Inner _instance = null!;
            public static Inner InnerInstance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new Inner();
                        Load();
                    }

                    return _instance;
                }
            }
        }
        private static Inner Instance {
            get {
                lock (Inner.InnerInstance.InstanceLock)
                {
                    return Inner.InnerInstance;
                }
            }
        }
    }
}
