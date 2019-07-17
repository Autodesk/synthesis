using System;
using Renci.SshNet;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Synthesis
{
    public static class EmulatorManager
    {
        public static string emulationDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\Synthesis\Emulator\");

        private const int DEFAULT_SSH_PORT = 10022;

        private const string USER = "lvuser";
        private const string PASSWORD = "";

        private const string STOP_COMMAND = "sudo killall frc_program_chooser.sh >/dev/null 2>&1; sudo killall java >/dev/null 2>&1; sudo killall FRCUserProgram >/dev/null 2>&1;";
        private const string START_COMMAND = "nohup /home/lvuser/frc_program_chooser.sh</dev/null >/dev/null 2>&1 &";

        private static bool isRunningRobotCode = false;
        private static bool frcUserProgramPresent = false;

        private static Process qemuProcess = null;

        public static void StartEmulator()
        {
            qemuProcess = Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = @"C:\Program Files\qemu\qemu-system-arm.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = " -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel " + emulationDir + "zImage" + " -dtb " + emulationDir + "zynq-zed.dtb" + " -display none -serial null -serial mon:stdio -append \"console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0 rw\" -net user,hostfwd=tcp::10022-:22,hostfwd=tcp::" + EmulatorNetworkConnection.DEFAULT_PORT + "-:" + EmulatorNetworkConnection.DEFAULT_PORT + ",hostfwd=tcp::2354-:2354 -net nic -sd " + emulationDir + "rootfs.ext4",
                Verb = "runas"
            });
        }

        public static bool IsVMRunning()
        {
            if (qemuProcess != null && qemuProcess.HasExited)
                qemuProcess = null;
            return qemuProcess != null;
        }

        public static void KillEmulator()
        {
            if (qemuProcess != null)
            {
                qemuProcess.Kill();
                qemuProcess = null;
            }
        }

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
                if (IsRunningRobotCode())
                    StopRobotCode();
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, DEFAULT_SSH_PORT, USER, PASSWORD))
                {
                    client.Connect();
                    client.RunCommand("rm FRCUserProgram FRCUserProgram.jar"); // Delete existing files so the frc program chooser knows which to run
                    frcUserProgramPresent = false;
                    client.Disconnect();
                }

                using (ScpClient client = new ScpClient(EmulatorNetworkConnection.DEFAULT_HOST, DEFAULT_SSH_PORT, USER, PASSWORD))
                {
                    client.Connect();
                    using (Stream localFile = File.OpenRead(userProgram.fullFileName))
                    {
                        client.Upload(localFile, @"/home/lvuser/" + userProgram.targetFileName);
                        frcUserProgramPresent = true;
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
                    using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, DEFAULT_SSH_PORT, USER, PASSWORD))
                    {
                        client.Connect();
                        VMConnected = client.IsConnected;
                        frcUserProgramPresent = client.RunCommand("[ -f FRCUserProgram ] || [ -f FRCUserProgram.jar ]").ExitStatus == 0;
                        client.Disconnect();
                    }
                }
                catch
                {
                    VMConnected = false;
                    frcUserProgramPresent = false;
                    isRunningRobotCode = false;
                }
                Thread.Sleep(1000); // ms
            }
        });

        public static void KillTestVMConnectionThread()
        {
            if (TestVMConnectionThread.IsAlive)
                TestVMConnectionThread.Abort();
        }

        public static bool IsVMConnected()
        {

            if (!TestVMConnectionThread.IsAlive)
                TestVMConnectionThread.Start();
            return VMConnected;
        }

        public static bool IsFRCUserProgramPresent()
        {
            return frcUserProgramPresent;
        }

        public static void StopRobotCode()
        {
            isRunningRobotCode = false;
            new Thread(() =>
            {
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, DEFAULT_SSH_PORT, USER, PASSWORD))
                {
                    client.Connect();
                    client.RunCommand(STOP_COMMAND);
                    client.Disconnect();
                }
            }).Start();
        }

        public static void StartRobotCode()
        {
            isRunningRobotCode = true;
            EmulatorNetworkConnection.Instance.OpenConnection();
            new Thread(() =>
            {
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, DEFAULT_SSH_PORT, USER, PASSWORD))
                {
                    client.Connect();
                    client.RunCommand(STOP_COMMAND + START_COMMAND);
                    client.Disconnect();
                }
            }).Start();
        }

        public static bool IsRunningRobotCode()
        {
            return isRunningRobotCode;
        }
    }
}