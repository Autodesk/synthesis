using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Synthesis
{
    /// <summary>
    /// Manages metadata associated with a user program file
    /// </summary>
    public class UserProgram
    {
        /// <summary>
        /// The type of user program
        /// </summary>
        public enum Type
        {
            JAVA,
            CPP,
            INVALID
        }

        const string JAR_EXTENSION = ".jar";
        const string DEFAULT_TARGET = "FRCUserProgram"; // Standardize target file name so the frc program chooser knows what to run

        public string FullFileName { get; private set; }
        public string TargetFileName { get; private set; }
        public Type ProgramType { get; private set; }

        public long Size { get; private set; } // Bytes

        /// <summary>
        /// Instantiate a new user program
        /// </summary>
        /// <param name="name">The name including the file path of the user program</param>
        public UserProgram(string name)
        {
            FullFileName = name;

            // string fileName = FullFileName.Substring(FullFileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            Size = (new FileInfo(FullFileName)).Length;

            try
            {
                // Test if the file is a zip file with a manifest entry (i.e. it's a jar file)
                using (ZipFile archive = new ZipFile(new FileStream(FullFileName, FileMode.Open, FileAccess.Read)))
                {
                    if (archive.GetEntry("META-INF/MANIFEST.MF") != null)
                    {
                        TargetFileName = DEFAULT_TARGET + JAR_EXTENSION;
                        ProgramType = Type.JAVA;
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // Not Java
            }
            try
            {
                // Parse the file header to determine that it's both ELF (begins with "\x7F ELF") and ARM (machine type byte = 0x28)
                byte[] buffer = new byte[20];
                using (FileStream fs = new FileStream(FullFileName, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                }
                var data = System.Text.Encoding.UTF8.GetString(buffer);

                if (data.Substring(0, 4) == ("\x7f" + "ELF") && buffer[18] == 0x28)
                {
                    TargetFileName = DEFAULT_TARGET;
                    ProgramType = Type.CPP;
                    return;
                }
            }
            catch (Exception)
            {
                // Not C++
            }

            ProgramType = Type.INVALID;
        }
    }
}
