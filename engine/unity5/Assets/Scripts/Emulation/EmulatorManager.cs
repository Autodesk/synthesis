using System;
using Renci.SshNet;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis
{
    public static class EmulatorManager
    {
        public static string emulationDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Emulator";

        private const int DEFAULT_SSH_PORT_CPP = 10022;
        private const int DEFAULT_SSH_PORT_JAVA = 10023;

        private const string USER = "lvuser";
        private const string PASSWORD = "";

        private const string STOP_COMMAND = "sudo killall frc_program_chooser.sh >/dev/null 2>&1; sudo killall java >/dev/null 2>&1; sudo killall FRCUserProgram >/dev/null 2>&1;";
        private const string START_COMMAND = "nohup /home/lvuser/frc_program_chooser.sh</dev/null >/dev/null 2>&1 &";
        private const string CHECK_EXISTS_COMMAND = "[ -f /home/lvuser/FRCUserProgram ] || [ -f /home/lvuser/FRCUserProgram.jar ]";
        private const string CHECK_RUNNING_COMMAND = "pidof frc_program_chooser.sh &> /dev/null";

        private static bool isRunningRobotCode = false;
        private static bool isTryingToRunRobotCode = false;
        private static bool frcUserProgramPresent = false;

        private static System.Diagnostics.Process qemuNativeProcess = null;
        private static System.Diagnostics.Process qemuJavaProcess = null;
        private static System.Diagnostics.Process grpcBridgeProcess = null;

        public static UserProgram.UserProgramType programType = UserProgram.UserProgramType.JAVA;
        private static bool isUserProgramFree = true;

        public static bool IsVMInstalled()
        {
            return Directory.Exists(emulationDir) &&
                File.Exists(emulationDir + Path.DirectorySeparatorChar + "zImage") &&
                File.Exists(emulationDir + Path.DirectorySeparatorChar + "rootfs.ext4") &&
                File.Exists(emulationDir + Path.DirectorySeparatorChar + "zynq-zed.dtb");
        }

        public static bool StartEmulator()
        {
            bool exception = false;
        try
        {
            Enum.TryParse(PlayerPrefs.GetString("UserProgramType"), out programType);
            qemuNativeProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "C:" + Path.DirectorySeparatorChar + "Program Files" + Path.DirectorySeparatorChar + "qemu" + Path.DirectorySeparatorChar + "qemu-system-arm.exe",
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                Arguments = " -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel " + emulationDir + Path.DirectorySeparatorChar + "zImage " +
                        "-dtb " + emulationDir + Path.DirectorySeparatorChar + "zynq-zed.dtb " +
                        "-display none -serial null -serial mon:stdio -append \"console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0 rw\" " +
                        "-net user,hostfwd=tcp::" + DEFAULT_SSH_PORT_CPP + "-:22,hostfwd=tcp::" + EmulatorNetworkConnection.DEFAULT_NATIVE_PORT + "-:" + EmulatorNetworkConnection.DEFAULT_NATIVE_PORT + "," +
                        "hostfwd=tcp::2354-:2354 -net nic -sd " + emulationDir + Path.DirectorySeparatorChar + "rootfs.ext4",
                Verb = "runas"
            });
            qemuJavaProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = @"C:\Program Files\qemu\qemu-system-x86_64.exe",
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                Arguments = " -m 2048 -kernel " + emulationDir + "kernel-java -nographic -append \"console=ttyPS0 root=/dev/sda rw\" -net user,hostfwd=tcp::" + DEFAULT_SSH_PORT_JAVA + "-:22,hostfwd=tcp::" + EmulatorNetworkConnection.DEFAULT_JAVA_PORT + "-:" + EmulatorNetworkConnection.DEFAULT_PORT + " -net nic -hda " + emulationDir + "rootfs-java.ext4",
                Verb = "runas"
            });

            grpcBridgeProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = emulationDir + "grpc-bridge.exe",
                Verb = "runas"
            });
        }
        catch (Exception)
        {
            exception = true;
        }
            return !exception && IsVMRunning();
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

        public static Task SCPFileSender(UserProgram userProgram, bool autorun = true)
        {
            programType = userProgram.type;
            return Task.Run(async () =>
            {
                try
                {
                    if (IsRunningRobotCode() || IsTryingToRunRobotCode())
                        await StopRobotCode();
                    isUserProgramFree = false;
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
                    isUserProgramFree = true;
                    await RestartRobotCode();
                }
                catch (Exception) { }
            });
        }

        private static bool VMConnected = false; // Last connection status

        private static Thread UpdateVMStatusThread = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                    {
                        client.Connect();
                        VMConnected = client.IsConnected;
                        frcUserProgramPresent = client.RunCommand(CHECK_EXISTS_COMMAND).ExitStatus == 0;
                        isRunningRobotCode = client.RunCommand(CHECK_RUNNING_COMMAND).ExitStatus == 0;
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
            if (UpdateVMStatusThread.IsAlive)
                UpdateVMStatusThread.Abort();
        }

        public static bool IsVMConnected()
        {

            if (!UpdateVMStatusThread.IsAlive) // TODO start somewhere else
                UpdateVMStatusThread.Start();
            return VMConnected;
        }

        public static bool IsFRCUserProgramPresent()
        {
            return frcUserProgramPresent;
        }

        public static bool IsUserProgramFree()
        {
            return isUserProgramFree || !IsFRCUserProgramPresent();
        }

        public static Task StopRobotCode()
        {
            return Task.Run(() =>
            {
                isTryingToRunRobotCode = false;
                isUserProgramFree = false;
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                {
                    client.Connect();
                    isRunningRobotCode = client.RunCommand(CHECK_RUNNING_COMMAND).ExitStatus == 0;
                    if (isRunningRobotCode)
                        client.RunCommand(STOP_COMMAND);
                    isRunningRobotCode = false;
                    client.Disconnect();
                }
                isUserProgramFree = true;
            });
        }

        public static Task RestartRobotCode()
        {
            return Task.Run(() =>
            {
                isTryingToRunRobotCode = true;
                isUserProgramFree = false;
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                {
                    client.Connect();
                    if (isRunningRobotCode)
                    {
                        isTryingToRunRobotCode = false; // To reset the gRPC connection
                        client.RunCommand(STOP_COMMAND);
                    }
                    isTryingToRunRobotCode = true;
                    isRunningRobotCode = true;
                    EmulatorNetworkConnection.Instance.OpenConnection();
                    client.RunCommand(START_COMMAND);
                    client.Disconnect();
                }
                isUserProgramFree = true;
            });
        }

        public static bool IsRunningRobotCode()
        {
            return isRunningRobotCode;
        }

        public static bool IsTryingToRunRobotCode()
        {
            return isTryingToRunRobotCode;
        }

        public static Task ReceiveProgramOutput()
        {
            return Task.Run(() =>
            {
                using (SshClient client = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                {
                    client.Connect();
                    var command = client.CreateCommand("tail -f /home/lvuser/logs/log.log");
                    command.BeginExecute();
                    var reader = new StreamReader(command.OutputStream);
                    var line = "";
                    while (IsTryingToRunRobotCode())
                    {
                        line = reader.ReadLine(); // Different read function?
                        if (line != null)
                            GUI.UserMessageManager.Dispatch(line, 5);
                    }
                    client.Disconnect();
                }
            });
        }

        public static Task FetchLogFile()
        {
            return Task.Run(() =>
            {
                string folder = SFB.StandaloneFileBrowser.OpenFolderPanel("Log file destination", "C:\\", false);
                if (folder == null)
                {
                    Debug.Log("No folder selected for log file destination");
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
