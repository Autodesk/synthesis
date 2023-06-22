using System;
using System.IO;

namespace Synthesis.Core.Util {
    public class IOHandler {
        public static string FileStorage {
            get {
                string a = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                           Path.PathSeparator + "Autodesk" + Path.PathSeparator + "Synthesis";
                if (!Directory.Exists(a)) {
                    Directory.CreateDirectory(a);
                }

                return a;
            }
        }
    }
}
