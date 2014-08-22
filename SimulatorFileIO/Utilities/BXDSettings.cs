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

    public string LastSkeletonDirectory = null;

    private BXDSettings()
    {

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
        Console.WriteLine(path);
        if (File.Exists(path))
        {
            FileStream fStream = null;
            try
            {
                fStream = new FileStream(path, FileMode.Open);
                var writer = new System.Xml.Serialization.XmlSerializer(typeof(BXDSettings));
                object result = writer.Deserialize(fStream);
                if (result != null && result is BXDSettings)
                {
                    Instance = (BXDSettings) result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load settings: " + e);
            }
            finally
            {
                if (fStream != null)
                    fStream.Close();
            }
        }
    }

    public static void Save()
    {
        string path = GetPath();
        FileStream fStream = null;
        try
        {
            fStream = new FileStream(path, FileMode.Create);
            var writer = new System.Xml.Serialization.XmlSerializer(typeof(BXDSettings));
            writer.Serialize(fStream, Instance);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to load settings: " + e);
        }
        finally
        {
            if (fStream != null)
                fStream.Close();
        }
    }
}
