using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SynthesisAPI.AssetManager;
using SynthesisAPI.VirtualFileSystem;
using System.Threading.Tasks;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.PreferenceManager
{
    public static class PreferenceManager
    {
        public static bool UnsavedChanges { get; private set; }
        private static Dictionary<string, Dictionary<string, object>> _preferences = new Dictionary<string, Dictionary<string, object>>();
        public static IReadOnlyDictionary<string, Dictionary<string, object>> Preferences {
            get => new ReadOnlyDictionary<string, Dictionary<string, object>>(_preferences);
        }
        
        private static readonly (string Directory, string Name) VirtualFilePath = ("/modules", "preferences.json");

        private static JsonAsset _asset;
        private static void ImportPreferencesAsset()
        {
            _asset = AssetManager.AssetManager.Import<JsonAsset>("text/json", true, VirtualFilePath.Directory,
                    VirtualFilePath.Name, Permissions.PublicReadWrite,
                    $"{VirtualFilePath.Directory}/{VirtualFilePath.Name}");
        }

        /// <summary>
        /// Set a preference using an identifier inside your unique dictionary. Uses an object type
        /// </summary>
        /// <typeparam name="TValueType">Type of preference. No constraints</typeparam>
        /// <param name="moduleName">name of the owner module</param>
        /// <param name="key">Identifier for preference</param>
        /// <param name="value">Preference value</param>
        [ExposedApi]
        public static void SetPreferenceObject(string moduleName, string key, object value) {
            if (!_preferences.ContainsKey(moduleName))
                _preferences[moduleName] = new Dictionary<string, object>();
            _preferences[moduleName][key] = value;
            UnsavedChanges = true;
        }

        /// <summary>
        /// Set a preference using an identifier inside your unique dictionary. Uses a generic type
        /// </summary>
        /// <typeparam name="TValueType">Type of preference. No constraints</typeparam>
        /// <param name="moduleName">name of the owner module</param>
        /// <param name="key">Identifier for preference</param>
        /// <param name="value">Preference value</param>
        [ExposedApi]
        public static void SetPreference<T>(string moduleName, string key, T value)
        {
            if (!_preferences.ContainsKey(moduleName))
                _preferences[moduleName] = new Dictionary<string, object>();
            _preferences[moduleName][key] = JsonConvert.SerializeObject(value);
            UnsavedChanges = true;
        }

        public static void SetPreferences(string moduleName, Dictionary<string, object> preferences)
        {
            foreach (string key in preferences.Keys)
            {
                SetPreference(moduleName, key, preferences[key]);
            }
        }
        
        /// <summary>
        /// This function will return a specific preference that has loaded in and/or
        /// set using the <see cref="SetPreferenceObject(string, string, object)"/> method
        /// </summary>
        /// <param name="moduleName">name of the owner module</param>
        /// <param name="key">Identifier for preference</param>
        /// <returns>Preference or null if no preference exists. Potential problems with the way JSON deserializes into object types</returns>
        [ExposedApi]
        public static object GetPreferenceObject(string moduleName, string key) {
            return ContainsPreference(moduleName, key) ? _preferences[moduleName][key] : null;
        }

        public static bool ContainsPreference(string moduleName, string key)
        {
            if (_preferences.ContainsKey(moduleName))
            {
                if (_preferences[moduleName].ContainsKey(key))
                {
                    return true;
                }
                //Logger.Log($"There is no key of value \"{key}\" for module \"{moduleName}\"");
            }
            //Logger.Log($"There is no module with name \"{moduleName}\"");
            return false;
        }

        /// <summary>
        /// This function will return a specific preference that has loaded in and/or
        /// set using the <see cref="SetPreference{TValueType}(string, string, TValueType)"/> method
        /// </summary>
        /// <typeparam name="TValueType">Return type. If useJsonReserialization is true this type must
        /// have a <see cref="JsonObjectAttribute">JsonObjectAttribute</see></typeparam>
        /// <param name="moduleName">name of the owner module</param>
        /// <param name="key">Identifier for preference</param>
        /// <returns>Preference or default if no preference exists. Will be of type T</returns>
        [ExposedApi]
        public static T GetPreference<T>(string moduleName, string key) {
            if (_preferences.ContainsKey(moduleName)) {
                if (_preferences[moduleName].ContainsKey(key)) {
                    return JsonConvert.DeserializeObject<T>((string)_preferences[moduleName][key]);
                }
                Logger.Log($"There is no key of value \"{key}\" for module \"{moduleName}\"");
            }
            Logger.Log($"There is no module with name \"{moduleName}\"");
            return default;
        }

        /// <summary>
        /// Clear your dictionary of preferences
        /// </summary>
        /// <param name="owner">Your module name</param>
        [ExposedApi]
        public static void ClearPreferences(string moduleName)
        {
            _preferences[moduleName] = new Dictionary<string, object>();
            UnsavedChanges = true;
        }

        #region IO

        /// <summary>
        /// Loads a JSON file asynchronously and loads preference data
        /// </summary>
        /// <param name="overrideChanges">Load regardless of unsaved data</param>
        /// <returns>Whether or not preferences were loaded successfully</returns>
        [ExposedApi]
        public static Task<bool> LoadAsync(bool overrideChanges = false) => Task<bool>.Factory.StartNew(() => Load(overrideChanges));

        /// <summary>
        /// Loads a JSON file that stores preference data
        /// </summary>
        /// <param name="overrideChanges">Load regardless of unsaved data</param>
        /// <returns>Whether or not preferences were loaded successfully</returns>
        [ExposedApi]
        public static bool Load(bool overrideChanges = false)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return LoadInner(overrideChanges);
        }

        internal static bool LoadInner(bool overrideChanges = false)
        {
            if (!overrideChanges && UnsavedChanges)
            {
                Logger.Log("Load failed: Preferences would have been overwritten");
                return false;
            }

            if (_asset == null)
            {
                ImportPreferencesAsset();
                if(_asset == null)
                {
                    Logger.Log("Load failed: No preferences exist");
                    return false;
                }
            }

            _preferences = _asset!.DeserializeInner<Dictionary<string, Dictionary<string, object>>>(offset: 0, retainPosition: true) ?? new Dictionary<string, Dictionary<string, object>>();

            UnsavedChanges = false;

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
            if(_asset == null)
                ImportPreferencesAsset();
            
            _asset.SerializeInner(_preferences);
            _asset.SaveToFileInner();

            UnsavedChanges = false;

            return true;
        }

        #endregion
    }
}
