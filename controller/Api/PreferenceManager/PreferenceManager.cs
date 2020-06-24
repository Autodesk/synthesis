using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using SynthesisAPI.AssetManager;
using SynthesisAPI.VirtualFileSystem;

namespace SynthesisAPI.PreferenceManager
{
    public static class PreferenceManager
    {

        private static readonly (string Path, string File) VirtualFilePath = ("/modules", "preferences.json");

        public static string BasePath = string.Format("{0}{1}Autodesk{1}Synthesis{1}",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Path.DirectorySeparatorChar);

        #region Accessing Preferences

        /// <summary>
        /// Set a preference using an identifier inside your unique dictionary
        /// </summary>
        /// <typeparam name="TValueType">Type of preference. No constraints</typeparam>
        /// <param name="owner">GUID of the owner</param>
        /// <param name="key">Identifier for preference</param>
        /// <param name="value">Preference value</param>
        public static void SetPreference<TValueType>(Guid owner, string key, TValueType value)
        {
            if (!Instance.Preferences.ContainsKey(owner))
                Instance.Preferences[owner] = new Dictionary<string, object>();
            Instance.Preferences[owner][key] = value!;
            _changesSaved = false;
        }

        /// <summary>
        /// This function will return a specific preference that has loaded in and/or
        /// set using the <see cref="SetPreference{TValueType}(Guid, string, TValueType)"/> method
        /// </summary>
        /// <param name="owner">GUID of the owner</param>
        /// <param name="key">Identifier for preference</param>
        /// <returns>Preference</returns>
        public static object GetPreference(Guid owner, string key)
        {
            if (Instance.Preferences.ContainsKey(owner))
            {
                if (Instance.Preferences[owner].ContainsKey(key))
                    return Instance.Preferences[owner][key];
                throw new ArgumentException($"There is no key of value \"{key}\" under owner \"{owner}\"");
            }
            throw new ArgumentException($"There is no owner of value \"{owner}\"");
        }

        /// <summary>
        /// This function will return a specific preference that has loaded in and/or
        /// set using the <see cref="SetPreference{TValueType}(Guid, string, TValueType)"/> method
        /// </summary>
        /// <typeparam name="TValueType">Return type. If useJsonReserialization is true this type must
        /// have a <see cref="JsonObjectAttribute">JsonObjectAttribute</see></typeparam>
        /// <param name="owner">GUID of the owner</param>
        /// <param name="key">Identifier for preference</param>
        /// <param name="useJsonDeserialization">Set this to true if you are trying to retrieve an object of a custom type. Be sure
        /// to label everything inside the type with the Attributes Newtonsoft provides</param>
        /// <returns>Preference</returns>
        public static TValueType GetPreference<TValueType>(Guid owner, string key, bool useJsonDeserialization = false)
        {
            if (Instance.Preferences.ContainsKey(owner))
            {
                if (Instance.Preferences[owner].ContainsKey(key))
                {
                    if (useJsonDeserialization)
                    {
                        if (!typeof(TValueType).IsDefined(typeof(JsonObjectAttribute), false))
                            throw new ArgumentException(
                                $"Type \"{typeof(TValueType).FullName}\" does not have the Newtonsoft.Json.JsonObjectAttribute");

                        return JsonConvert.DeserializeObject<TValueType>(JsonConvert.SerializeObject(Instance.Preferences[owner][key]));
                    }
                    return (TValueType) Instance.Preferences[owner][key];
                }
                throw new ArgumentException($"There is no key of value \"{key}\" under owner \"{owner}\"");
            }
            throw new ArgumentException($"There is no owner of value \"{owner}\"");
        }

        /// <summary>
        /// Clear your dictionary of preferences
        /// </summary>
        /// <param name="owner">Your guid</param>
        public static void ClearPreferences(Guid owner)
        {
            Instance.Preferences[owner] = new Dictionary<string, object>();
            _changesSaved = false;
        }

        #endregion

        #region IO

        /// <summary>
        /// Loads a JSON file that stores preference data
        /// </summary>
        /// <param name="overrideChanges">Load regardless of unsaved data</param>
        /// <returns>Whether or not the load executed successfully</returns>
        public static bool Load(bool overrideChanges = false)
        {
            if (!overrideChanges && !_changesSaved)
                return false;

            Instance.Asset = AssetManager.AssetManager.ImportOrCreate<JsonAsset>("text/json",
                VirtualFilePath.Path, VirtualFilePath.File, Guid.Empty,
                Permissions.PublicWrite, VirtualFilePath.File)!;

            var deserialized =
                Instance.Asset.Deserialize<Dictionary<Guid, Dictionary<string, object>>>(offset: 0,
                    retainPosition: true);
            Instance.Preferences =
                deserialized ?? new Dictionary<Guid, Dictionary<string, object>>(); // Failed to load; reset to default

            _changesSaved = true;
            return true;
        }

        /// <summary>
        /// Saves a JSON file with preference data
        /// </summary>
        /// <returns>Whether or not the save executed successfully</returns>
        public static bool Save()
        {
            JsonAsset? jsonAsset;

            if (FileSystem.ResourceExists(VirtualFilePath.Path, VirtualFilePath.File))
            {
                jsonAsset = FileSystem.Traverse(VirtualFilePath.Path + '/' + VirtualFilePath.File) as JsonAsset;
            }
            else
            {
                jsonAsset = new JsonAsset(VirtualFilePath.File, Guid.Empty, Permissions.Private, VirtualFilePath.File);
                FileSystem.AddResource(VirtualFilePath.Path, jsonAsset);
            }

            jsonAsset?.Serialize(Instance.Preferences);
            jsonAsset?.SaveToFile();

            _changesSaved = true;

            return true;
        }

        #endregion

        private static bool _changesSaved = false;

        private class Inner
        {
            private Inner()
            {
                Preferences = new Dictionary<Guid, Dictionary<string, object>>();
            }
            public Dictionary<Guid, Dictionary<string, object>> Preferences;

            public JsonAsset? Asset;
            private static Inner _instance = null!;
            public static Inner InnerInstance
            {
                get
                {
                    if (_instance is null)
                    {
                        _instance = new Inner();
                        PreferenceManager.Load();
                    }

                    return _instance;
                }
            }
        }
        private static Inner Instance => Inner.InnerInstance;
    }
}
