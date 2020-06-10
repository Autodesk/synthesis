using System;
using System.IO;
using System.Xml;
using UnityEngine;

namespace Synthesis.Util
{

    public class IOHandler
    {

        public static string FileStorage {
            get {
                string a = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.PathSeparator + "Autodesk" + Path.PathSeparator + "Synthesis";
                if (!Directory.Exists(a)) Directory.CreateDirectory(a);
                return a;
            }
        }

        public static string ModuleFolder { get => "Modules"; }
        public static string StyleFolder { get => "Styles"; }

        public static XmlDocument ReadXmlDocument(string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File doesn't exist. Returning null XmlDocument");
                return null;
            }
            XmlDocument d = new XmlDocument();
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                d.Load(fs);
                fs.Close();
            } catch (Exception e)
            {
                Debug.Log("Exception while reading file to XmlDocument:\n" + e.StackTrace);
            }
            return d;
        }

    }

}