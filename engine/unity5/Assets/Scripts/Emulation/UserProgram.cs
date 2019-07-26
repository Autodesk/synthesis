using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis
{
    public class UserProgram
    {
        public enum UserProgramType
        {
            JAVA,
            CPP
        }

        public string fullFileName { get; private set; }
        public string targetFileName { get; private set; }
        public UserProgramType type { get; private set; }

        public UserProgram(string fullFileName)
        {
            this.fullFileName = fullFileName;

            string fileName = fullFileName.Substring(fullFileName.LastIndexOf('\\') + 1);

            this.targetFileName = "FRCUserProgram"; // Standardize target file name so the frc program chooser knows what to run
            const string JAR_EXTENSION = ".jar";

            if (fileName.Length > JAR_EXTENSION.Length && fileName.Substring(fileName.Length - JAR_EXTENSION.Length) == JAR_EXTENSION)
            {
                this.targetFileName += JAR_EXTENSION;
                this.type = UserProgramType.JAVA;
            }
            else
            {
                this.type = UserProgramType.CPP;
            }
        }
    }
}
