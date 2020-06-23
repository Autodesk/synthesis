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

            Load();
        }

        #region Accessing Preferences

        /// <summary>
        /// Set a preference using an indentifier inside your unique dicitonary
        /// </summary>
        /// <typeparam name="TValueType">Type of preference. No constraints</typeparam>
        /// <param name="owner">GUID of the owner</param>
        /// <param name="key">Identifier for preference</param>
        /// <param name="value">Preference value</param>
        public static void SetPreference<TValueType>(Guid owner, string key, TValueType value)
        {
            if (!preferences.ContainsKey(owner)) 
                preferences[owner] = new Dictionary<string, object>();
            preferences[owner][key] = value;
            modified = true;
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

        /// <summary>
        /// This function will return a specific preference that has loaded in and/or
        /// set using the <see cref="SetPreference{TValueType}(Guid, string, TValueType)"/> method
        /// </summary>
        /// <typeparam name="TValueType">Return type. If useJsonReserialization is true this type must
        /// have a <see cref="JsonObjectAttribute">JsonObjectAttribute</c></typeparam>
        /// <param name="owner">GUID of the owner</param>
        /// <param name="key">Identifier for preference</param>
        /// <param name="useJsonReserialization">Set this to true if you are trying to retrieve an object of a custom type. Be sure
        /// to label everything inside the type with the Attributes Newtonsoft provides</param>
        /// <returns>Preference</returns>
        public static TValueType GetPreference<TValueType>(Guid owner, string key, bool useJsonReserialization = false)
        {
            if (preferences.ContainsKey(owner))
            {
                if (preferences[owner].ContainsKey(key))
                {
                    if (useJsonReserialization)
                    {
                        if (!typeof(TValueType).IsDefined(typeof(JsonObjectAttribute), false))
                            throw new ArgumentException(string.Format("Type \"{0}\" does not have the Newtonsoft.Json.JsonObjectAttribute",
                                typeof(TValueType).FullName));

                        return JsonConvert.DeserializeObject<TValueType>(JsonConvert.SerializeObject(preferences[owner][key]));
                    }
                    else
                    {
                        return (TValueType)Convert.ChangeType(preferences[owner][key], typeof(TValueType));
                    }
                } else
                {
                    throw new ArgumentException(string.Format("There is no key of value \"{0}\" under owner \"{1}\"", key, owner));
                }
            } else
            {
                throw new ArgumentException(string.Format("There is no owner of value \"{0}\"", owner));
            }
        }

        /// <summary>
        /// Clear your dictionary of preferences
        /// </summary>
        /// <param name="owner">Your guid</param>
        public static void ClearPreferences(Guid owner)
        {
            preferences[owner] = new Dictionary<string, object>();
            modified = true;
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
            if (!overrideChanges && modified)
                return false;

            JSONEntry entry;

            if (FileSystem.ResourceExists(VirtualFilePath.path, VirtualFilePath.file))
            {
                entry = FileSystem.Traverse(VirtualFilePath.path + '/' + VirtualFilePath.file) as JSONEntry;
            } else
            {
                entry = new JSONEntry(VirtualFilePath.file, MyGuid, Permissions.Private, ActualFilePath);
                FileSystem.AddResource(VirtualFilePath.path, entry);
            }

            Dictionary<Guid, Dictionary<string, object>> temp = null;
            if (!entry.Deserialize(MyGuid, out temp))
            {
                return false;
            }

            preferences = temp;
            
            SavedOrReset();

            return true;
        }

        /// <summary>
        /// Saves a JSON file with preference data
        /// </summary>
        /// <returns>Whether or not the save executed successfully</returns>
        public static bool Save()
        {
            JSONEntry entry;

            if (FileSystem.ResourceExists(VirtualFilePath.path, VirtualFilePath.file))
            {
                entry = FileSystem.Traverse(VirtualFilePath.path + '/' + VirtualFilePath.file) as JSONEntry;
            }
            else
            {
                entry = new JSONEntry(VirtualFilePath.file, MyGuid, Permissions.Private, ActualFilePath);
                FileSystem.AddResource(VirtualFilePath.path, entry);
            }

            if (!entry.Serialize(preferences, MyGuid))
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
