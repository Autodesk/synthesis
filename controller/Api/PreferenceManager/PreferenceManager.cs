using Newtonsoft.Json;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace SynthesisAPI.PreferenceManager
{
    public static class PreferenceManager
    {
        private static Dictionary<string, Dictionary<string, object>> preferences;

        public static string BasePath {
            get => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar
                + "Autodesk" + System.IO.Path.DirectorySeparatorChar + "Synthesis" + System.IO.Path.DirectorySeparatorChar;
        }

        static PreferenceManager()
        {
            preferences = new Dictionary<string, Dictionary<string, object>>();
        }

        #region Accessing Preferences

        public static void SetPreference<TValueType>(string owner, string key, TValueType value)
        {
            if (!preferences.ContainsKey(owner)) 
                preferences[owner] = new Dictionary<string, object>();
            preferences[owner][key] = value;
            modified = true;
        }

        public static TValueType GetPreference<TValueType>(string owner, string key)
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

        public static void ClearPreferences(string owner)
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
        /// <param name="file">Filename of the JSON file containing the data</param>
        /// <param name="overrideChanges">Load regardless of unsaved data</param>
        /// <returns>Whether or not the load executed successfully</returns>
        public static bool Load(string file, bool overrideChanges = false)
        {
            if (!overrideChanges && modified)
                return false;

            if (!File.Exists(BasePath + file))
                return false;

            string jsonData;
            
            try
            {
                FileStream fs = new FileStream(BasePath + file, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                jsonData = sr.ReadToEnd();
                sr.Close();
                fs.Close();
            } catch (Exception e) { return false; }

            Dictionary<string, string> firstWaveDeserialization = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

            preferences.Clear();

            foreach (var kvp in firstWaveDeserialization)
            {
                preferences.Add(kvp.Key, JsonConvert.DeserializeObject<Dictionary<string, object>>(kvp.Value));
            }
            
            SavedOrReset();

            return true;
        }

        /// <summary>
        /// Saves a JSON file with preference data
        /// TODO: Attempt serializing all of the data at once
        /// TODO: Re-evalute the visibility of this function
        /// </summary>
        /// <param name="file">Filename to write JSON data to</param>
        /// <returns>Whether or not the save executed successfully</returns>
        public static bool Save(string file)
        {
            Dictionary<string, string> firstWaveSerialization = new Dictionary<string, string>();

            foreach (var kvp in preferences)
            {
                string a = JsonConvert.SerializeObject(kvp.Value);
                firstWaveSerialization.Add(kvp.Key, a);
            }

            try
            {
                FileStream fs = new FileStream(BasePath + file, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(sw, firstWaveSerialization);
                sw.Flush();
                sw.Close();
                fs.Close();
            } catch (Exception e)
            {
                return false;
            }

            SavedOrReset();

            return true;
        }

        #endregion

        private static void SavedOrReset() => modified = false;
        private static bool modified = false;
    }
    
}
