using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis
{
    public class UserProgram
    {
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
