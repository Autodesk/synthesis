using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Synthesis.Import {
    
    /// <summary>
    /// Logger for debugging.
    /// You need to call Init() to restart the file
    /// </summary>
    public static class Logger {

        public static void Init() => Inner.InnerInstance.Init();
        public static void Log(string m) => Inner.InnerInstance.Log(m);

        private class Inner {
            private static Inner innerInstance = null;
            public static Inner InnerInstance {
                get {
                    if (innerInstance == null)
                        innerInstance = new Inner();
                    return innerInstance;
                }
            }

            private string path = string.Empty; // "C:\\Users\\hunte\\SynLog.txt";
            private FileStream fs;
            private StreamWriter sw;
            private readonly object threadLock = new object();

            public void Init() {

                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.AltDirectorySeparatorChar + "SynLog.txt";

                if (File.Exists(path))
                    File.Delete(path);

                File.Create(path).Close();
            }

            public void Log(string m) {
                lock (threadLock) {
                    
                    if (path == string.Empty)
                        Init();
                    
                    fs = File.OpenWrite(path);
                    fs.Position = fs.Length;
                    sw = new StreamWriter(fs);
                    sw.WriteLine(m);
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                }
            }
        }
    }
}
