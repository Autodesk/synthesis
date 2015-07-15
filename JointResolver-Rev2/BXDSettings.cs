using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

public class BXDSettings
{
    private static BXDSettings _instance = null;
    public static BXDSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                Load();
            }
            if (_instance == null)  // Load failed
            {
                _instance = new BXDSettings();
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    private static Type[] knownTypes = new Type[] { typeof(EditorsLibrary.ViewerSettings.ViewerSettingsValues),
                                                    typeof(EditorsLibrary.ExporterSettings.EditorSettingsValues) };

    public string LastSkeletonDirectory = null;

    public List<SettingsObject> settingsObjects = null;

    private BXDSettings()
    {

    }

    public void AddSettingsObject(string key, object toAdd)
    {
        if (settingsObjects == null) settingsObjects = new List<SettingsObject>();

        if (settingsObjects.ContainsKey(key))
        {
            if (settingsObjects.Get(key).GetType() != toAdd.GetType()) 
                throw new Exception("Key already exists and is associated with an object of a different type");
        }

        settingsObjects.Set(key, toAdd);
    }

    public object GetSettingsObject(string key)
    {
        if (settingsObjects == null) settingsObjects = new List<SettingsObject>();

        if (!settingsObjects.ContainsKey(key)) return null;

        return settingsObjects.Get(key);
    }

    private static string GetPath()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BXD_Aardvark\\";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path + "settings.xml";
    }

    public static void Load()
    {
        string path = GetPath();
        
        if (File.Exists(path))
        {
            try
            {
                using (FileStream fStream = new FileStream(path, FileMode.Open))
                {
                    var writer = new System.Xml.Serialization.XmlSerializer(typeof(BXDSettings), knownTypes);
                    object result = writer.Deserialize(fStream);
                    if (result != null && result is BXDSettings)
                    {
                        Instance = (BXDSettings)result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load settings: " + e);
            }
        }
    }

    public static void Save()
    {
        string path = GetPath();

        try
        {
            using (FileStream fStream = new FileStream(path, FileMode.Create))
            {
                var writer = new System.Xml.Serialization.XmlSerializer(typeof(BXDSettings), knownTypes);
                writer.Serialize(fStream, Instance);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to load settings: " + e);
        }
    }

    public class SettingsObject
    {

        public string Key;

        public object Value;

        public SettingsObject()
        {
            Key = null;
            Value = null;
        }

        public SettingsObject(string key, object value)
        {
            Key = key;
            Value = value;
        }

    }

}

public static class SettingsObjectExtensions
{

    public static bool ContainsKey(this List<BXDSettings.SettingsObject> settingsList, string key)
    {
        foreach (BXDSettings.SettingsObject sObject in settingsList)
        {
            if (key == sObject.Key) return true;
        }

        return false;
    }

    public static object Get(this List<BXDSettings.SettingsObject> settingsList, string key)
    {
        foreach (BXDSettings.SettingsObject sObject in settingsList)
        {
            if (key == sObject.Key) return sObject.Value;
        }

        throw new IndexOutOfRangeException("Settings object" + key + "not found");
    }

    public static void Set(this List<BXDSettings.SettingsObject> settingsList, string key, object value)
    {
        foreach (BXDSettings.SettingsObject sObject in settingsList)
        {
            if (key == sObject.Key)
            {
                sObject.Value = value;
                return;
            }
        }

        settingsList.Add(new BXDSettings.SettingsObject(key, value));
    }

}
