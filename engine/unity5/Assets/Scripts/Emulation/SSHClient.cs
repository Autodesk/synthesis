using System;
using Renci.SshNet;
using System.IO;
using System.Threading;

namespace Synthesis
{
    public class SSHClient
    {
        //@"C:\Program Files (x86)\Autodesk\Synthesis"

        private static int DEFAULT_PORT = 10022;

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

        public static void SCPFileSender(UserProgram userProgram)
        {
            try
            {
                //choofdlog.Multiselect = true

                using (SshClient client = new SshClient(EmulationController.DEFAULT_HOST, DEFAULT_PORT, "lvuser", ""))
                {
                    client.Connect();
                    client.RunCommand("rm FRCUserProgram FRCUserProgram.jar"); // Delete existing files so the frc program chooser knows which to run
                    client.Disconnect();
                }

                using (ScpClient client = new ScpClient(EmulationController.DEFAULT_HOST, DEFAULT_PORT, "lvuser", ""))
                {
                    client.Connect();
                    using (Stream localFile = File.OpenRead(userProgram.fullFileName))
                    {
                        client.Upload(localFile, @"/home/lvuser/" + userProgram.targetFileName);
                    }
                    client.Disconnect();
                }
            }
            catch (Exception) { }
        }

        private static bool VMConnected = false; // Last connection status
        private static Thread TestVMConnectionThread = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    using (SshClient client = new SshClient(EmulationController.DEFAULT_HOST, DEFAULT_PORT, "lvuser", ""))
                    {
                        client.Connect();
                        VMConnected = client.IsConnected;
                        client.Disconnect();
                    }
                }
                catch
                {
                    VMConnected = false;
                }
                if (VMConnected) // Sleep longer if connected since it's less vital to check for disconnects
                {
                    Thread.Sleep(15000); // ms
                }
                else
                {
                    Thread.Sleep(3000); // ms
                }
            }
        });

        public static bool IsVMConnected()
        {
            if (!TestVMConnectionThread.IsAlive)
            {
                TestVMConnectionThread.Start();
            }
            return VMConnected;
        }

        public static void StopRobotCode()
        {
            new Thread(() =>
            {
                using (SshClient client = new SshClient(EmulationController.DEFAULT_HOST, DEFAULT_PORT, "lvuser", ""))
                {
                    client.Connect();
                    client.RunCommand("sudo killall frc_program_chooser.sh >/dev/null 2>&1; sudo killall java >/dev/null 2>&1; sudo killall FRCUserProgram >/dev/null 2>&1;");
                    client.Disconnect();
                }
            }).Start();
        }

        public static void StartRobotCode()
        {
            new Thread(() =>
            {
                using (SshClient client = new SshClient(EmulationController.DEFAULT_HOST, DEFAULT_PORT, "lvuser", ""))
                {
                    client.Connect();
                    //client.RunCommand("sudo killall frc_program_chooser.sh >/dev/null 2>&1; sudo killall java >/dev/null 2>&1; sudo killall FRCUserProgram >/dev/null 2>&1; nohup /home/lvuser/frc_program_chooser.sh </dev/null >/dev/null 2>&1 &");
                    client.Disconnect();
                }
            }).Start();
        }
    }
}