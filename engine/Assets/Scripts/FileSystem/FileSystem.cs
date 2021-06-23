using System;
using System.IO;
using UnityEngine;

namespace Synthesis.FileSystem
{
    public static class FileSystem
    {
        public static string Root = Application.persistentDataPath;
        public static string Robots = Path.Combine(Root, "Models");
        public static string Fields = Path.Combine(Root, "Fields");
        public static string Preferences = Path.Combine(Root, "Preferences");

        /// <summary>
        /// Create folders in root on runtime
        /// Note: Root does not have write access but folders in Root do
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            if (!Directory.Exists(Robots))
                Directory.CreateDirectory(Robots);
            if (!Directory.Exists(Fields))
                Directory.CreateDirectory(Fields);
            if (!Directory.Exists(Preferences))
                Directory.CreateDirectory(Preferences);
        }
    }
}
