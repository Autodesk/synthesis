using System;
using Renci.SshNet;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;

namespace Synthesis
{
    public static class EmulatorManager
    {
        public static string emulationDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\Synthesis\Emulator\");

        private const int DEFAULT_SSH_PORT_CPP = 10022;
        private const int DEFAULT_SSH_PORT_JAVA = 10023;

        private const string USER = "lvuser";
        private const string PASSWORD = "";

        private const string STOP_COMMAND = "sudo killall frc_program_chooser.sh >/dev/null 2>&1; sudo killall java >/dev/null 2>&1; sudo killall FRCUserProgram >/dev/null 2>&1;";
        private const string START_COMMAND = "nohup /home/lvuser/frc_program_chooser.sh</dev/null >/dev/null 2>&1 &";

        private static bool isRunningRobotCode = false;
        private static bool frcUserProgramPresent = false;

        private static Process qemuNativeProcess = null;
        private static Process qemuJavaProcess = null;
        private static Process grpcBridgeProcess = null;

        public static UserProgram.UserProgramType programType = UserProgram.UserProgramType.JAVA;

        public static void StartEmulator()
        {
            Enum.TryParse(PlayerPrefs.GetString("UserProgramType"), out programType);
            qemuNativeProcess = Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = @"C:\Program Files\qemu\qemu-system-arm.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = " -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel " + emulationDir + "kernel-native" + " -dtb " + emulationDir + "zynq-zed.dtb" + " -display none -serial null -serial mon:stdio -append \"console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0 rw\" -net user,hostfwd=tcp::" + DEFAULT_SSH_PORT_CPP + "-:22,hostfwd=tcp::" + EmulatorNetworkConnection.DEFAULT_NATIVE_PORT + "-:" + EmulatorNetworkConnection.DEFAULT_PORT + " -net nic -sd " + emulationDir + "rootfs-native.ext4",
                Verb = "runas"
            });
            qemuJavaProcess = Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = @"C:\Program Files\qemu\qemu-system-x86_64.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = " -m 2048 -kernel " + emulationDir + "kernel-java -nographic -append \"console=ttyPS0 root=/dev/sda rw\" -net user,hostfwd=tcp::" + DEFAULT_SSH_PORT_JAVA + "-:22,hostfwd=tcp::" + EmulatorNetworkConnection.DEFAULT_JAVA_PORT + "-:" + EmulatorNetworkConnection.DEFAULT_PORT + " -net nic -hda " + emulationDir + "rootfs-java.ext4",
                Verb = "runas"
            });

            grpcBridgeProcess = Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = emulationDir + "grpc-bridge.exe",
                Verb = "runas"
            });
        }

        public static bool IsVMRunning()
        {
            if (qemuNativeProcess != null && qemuNativeProcess.HasExited)
                qemuNativeProcess = null;
            if (qemuJavaProcess != null && qemuJavaProcess.HasExited)
                qemuJavaProcess = null;
            if (grpcBridgeProcess != null && grpcBridgeProcess.HasExited)
                grpcBridgeProcess = null;

            return qemuNativeProcess != null && qemuJavaProcess != null && grpcBridgeProcess != null;
        }

        public static void KillEmulator()
        {
            if (qemuNativeProcess != null)
            {
                qemuNativeProcess.Kill();
                qemuNativeProcess = null;
                qemuJavaProcess.Kill();
                qemuJavaProcess = null;
                grpcBridgeProcess.Kill();
                grpcBridgeProcess = null;
            }
        }

        public static void SCPFileSender(UserProgram userProgram)
        {
            programType = userProgram.type;
            try
            {
                if (IsRunningRobotCode())
                    StopRobotCode();
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                {
                    client.Connect();
                    client.RunCommand("rm FRCUserProgram FRCUserProgram.jar"); // Delete existing files so the frc program chooser knows which to run
                    frcUserProgramPresent = false;
                    client.Disconnect();
                }

                using (ScpClient client = new ScpClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
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
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
        }

        private static bool VMConnected = false; // Last connection status

        private static Thread TestVMConnectionThread = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
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
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
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
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
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

        public static void ReceiveProgramOutput()
        {
            new Thread(() =>
            {
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                {
                    client.Connect();
                    var command = client.CreateCommand("tail -f /home/lvuser/logs/log.log");
                    command.BeginExecute();
                    var reader = new StreamReader(command.OutputStream);
                    var line = "";
                    while (IsRunningRobotCode())
                    {
                        line = reader.ReadLine(); // Different read function?
                        if (line != null)
                            GUI.UserMessageManager.Dispatch(line, 5);
                    }
                    client.Disconnect();
                }
            }).Start();
        }

        public static void FetchLogFile()
        {
            Task.Run(() =>
            {
                string folder = SFB.StandaloneFileBrowser.OpenFolderPanel("Log file destination", "C:\\", false);
                if (folder == null)
                {
                    UnityEngine.Debug.Log("No folder selected for log file destination");
                }
                else
                {

                    using (ScpClient client = new ScpClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                    {
                        client.Connect();
                        Stream localLogFile = File.Create(folder + "/log.log");
                        client.Download("/home/lvuser/logs/log.log", localLogFile);
                        localLogFile.Close();
                        client.Disconnect();
                    }
                }
            });
        }
    }
}
