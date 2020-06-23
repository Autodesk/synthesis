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
        private static Dictionary<Guid, Dictionary<string, object>> preferences;

        private static (string path, string file) VirtualFilePath = ("/modules", "preferences.json");
        private static string ActualFilePath = FileSystem.BasePath + "files" + Path.DirectorySeparatorChar + "preferences.json";
        private static Guid MyGuid = Guid.NewGuid();

        public static string BasePath {
            get => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar
                + "Autodesk" + System.IO.Path.DirectorySeparatorChar + "Synthesis" + System.IO.Path.DirectorySeparatorChar;
        }

        static PreferenceManager()
        {
            preferences = new Dictionary<Guid, Dictionary<string, object>>();
        }

        #region Accessing Preferences

        public static void SetPreference<TValueType>(Guid owner, string key, TValueType value)
        {
            if (!preferences.ContainsKey(owner)) 
                preferences[owner] = new Dictionary<string, object>();
            preferences[owner][key] = value;
            modified = true;
        }

        public static object GetPreference(Guid owner, string key)
        {
            if (preferences.ContainsKey(owner))
            {
                if (preferences[owner].ContainsKey(key))
                {
                    return preferences[owner][key];
                }
                else
                {
                    throw new ArgumentException(string.Format("There is no key of value \"{0}\" under owner \"{1}\"", key, owner));
                }
            }
            else
            {
                throw new ArgumentException(string.Format("There is no owner of value \"{0}\"", owner));
            }
        }

        public static TValueType GetPreference<TValueType>(Guid owner, string key)
        {
            if (preferences.ContainsKey(owner))
            {
                if (preferences[owner].ContainsKey(key))
                {
                    return (TValueType)Convert.ChangeType(preferences[owner][key], typeof(TValueType));
                } else
                {
                    throw new ArgumentException(string.Format("There is no key of value \"{0}\" under owner \"{1}\"", key, owner));
                }
            } else
            {
                throw new ArgumentException(string.Format("There is no owner of value \"{0}\"", owner));
            }
        }

        public static void ClearPreferences(Guid owner)
        {
            preferences[owner] = new Dictionary<string, object>();
            modified = true;
        }

        #endregion

        #region IO

        /// <summary>
        /// Loads a JSON file that stores preference data
        /// TODO: Attempt deserializing all of the data at once
        /// TODO: Re-evalute the visibility of this function
        /// </summary>
        /// <param name="overrideChanges">Load regardless of unsaved data</param>
        /// <returns>Whether or not the load executed successfully</returns>
        public static bool Load(bool overrideChanges = false)
        {
            if (!overrideChanges && modified)
                return false;

            JSONAsset asset;
            try
            {
                if (FileSystem.ResourceExists(VirtualFilePath.path, VirtualFilePath.file))
                {
                    asset = AssetManager.AssetManager.GetAsset<JSONAsset>(VirtualFilePath.path + '/' + VirtualFilePath.file);
                }
                else if (File.Exists(ActualFilePath))
                {
                    byte[] data = File.ReadAllBytes(ActualFilePath);
                    asset = AssetManager.AssetManager.Import<JSONAsset>("text/json", VirtualFilePath.path, data, VirtualFilePath.file, MyGuid, Permissions.Private);
                }
                else
                {
                    return false;
                }
            } catch (Exception e) { return false; }

            preferences = asset.Deserialize<Dictionary<Guid, Dictionary<string, object>>>();
            
            SavedOrReset();

            return true;
        }

        /// <summary>
        /// Saves a JSON file with preference data
        /// TODO: Attempt serializing all of the data at once
        /// TODO: Re-evalute the visibility of this function
        /// </summary>
        /// <returns>Whether or not the save executed successfully</returns>
        public static bool Save()
        {
            try
            {
                FileSystem.RemoveResource(VirtualFilePath.path, VirtualFilePath.file, MyGuid);
                FileStream fs = new FileStream(ActualFilePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(sw, preferences);
                sw.Flush();
                sw.Close();
                fs.Close();
            } catch (Exception e) { return false; }

            SavedOrReset();

            return true;
        }

        #endregion

        private static void SavedOrReset() => modified = false;
        private static bool modified = false;
    }
    
}
