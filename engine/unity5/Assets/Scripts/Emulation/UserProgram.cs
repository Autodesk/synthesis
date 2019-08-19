using System.IO;

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
            CPP
        }

        const string JAR_EXTENSION = ".jar";
        const string DEFAULT_TARGET = "FRCUserProgram"; // Standardize target file name so the frc program chooser knows what to run

        public string FullFileName { get; private set; }
        public string TargetFileName { get; private set; }
        public Type ProgramType { get; private set; }

        /// <summary>
        /// Instantiate a new user program
        /// </summary>
        /// <param name="name">The name including the file path of the user program</param>
        public UserProgram(string name)
        {
            FullFileName = name;

            string fileName = FullFileName.Substring(FullFileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            if (fileName.Length >= JAR_EXTENSION.Length && fileName.EndsWith(JAR_EXTENSION))
            {
                TargetFileName = DEFAULT_TARGET + JAR_EXTENSION;
                ProgramType = Type.JAVA;
            }
            else
            {
                TargetFileName = DEFAULT_TARGET;
                ProgramType = Type.CPP;
            }
        }
    }
}
