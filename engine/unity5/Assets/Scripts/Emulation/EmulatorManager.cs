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
        private static string EMULATION_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Emulator";

        private const int DEFAULT_SSH_PORT_CPP = 10022;
        private const int DEFAULT_SSH_PORT_JAVA = 10023;

        private const string USER = "lvuser";
        private const string PASSWORD = "";

        private const string REMOTE_LOG_NAME = "/home/lvuser/logs/log.log";

        private const string STOP_COMMAND = "sudo killall frc_program_chooser.sh >/dev/null 2>&1; sudo killall java >/dev/null 2>&1; sudo killall FRCUserProgram >/dev/null 2>&1;";
        private const string START_COMMAND = "nohup /home/lvuser/frc_program_chooser.sh</dev/null >/dev/null 2>&1 &";
        private const string CHECK_EXISTS_COMMAND = "[ -f /home/lvuser/FRCUserProgram ] || [ -f /home/lvuser/FRCUserProgram.jar ]";
        private const string CHECK_RUNNING_COMMAND = "pidof frc_program_chooser.sh &> /dev/null";
        private const string RECEIVE_PRINTS_COMMAND = "tail -f " + REMOTE_LOG_NAME;

        private static System.Diagnostics.Process qemuNativeProcess = null;
        private static System.Diagnostics.Process qemuJavaProcess = null;
        private static System.Diagnostics.Process grpcBridgeProcess = null;

        public static UserProgram.UserProgramType programType = UserProgram.UserProgramType.JAVA;
        
		// Last connection status
        public static bool UseEmulation = false;
        private static bool VMConnected = false;
        private static bool isUserProgramFree = true;
        private static bool frcUserProgramPresent = false;
        private static bool isTryingToRunRobotCode = false;
        private static bool isRunningRobotCode = false;

        private static System.Diagnostics.Process qemuProcess = null;
        private static bool updatingStatus = false;

        private static SshClient Client
        {
            get
            {
                if (SSHClientInternal.instance == null)
                    SSHClientInternal.instance = new SshClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD);
                if (!SSHClientInternal.instance.IsConnected)
                    SSHClientInternal.instance.Connect();
                return SSHClientInternal.instance;
            }
        }

        private class SSHClientInternal
        {
            static SSHClientInternal() { }
            internal static SshClient instance = null;
        }

        public static bool IsVMInstalled()
        {
            return Directory.Exists(EMULATION_DIR) &&
                File.Exists(EMULATION_DIR + Path.DirectorySeparatorChar + "zImage") &&
                File.Exists(EMULATION_DIR + Path.DirectorySeparatorChar + "rootfs.ext4") &&
                File.Exists(EMULATION_DIR + Path.DirectorySeparatorChar + "zynq-zed.dtb");
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

        public static bool IsVMConnected()
        {
            return VMConnected;
        }

        public static bool IsFRCUserProgramPresent()
        {
            return frcUserProgramPresent;
        }

        public static bool IsTryingToRunRobotCode()
        {
            return isTryingToRunRobotCode;
        }

        public static bool IsUserProgramFree()
        {
            return isUserProgramFree || !IsFRCUserProgramPresent();
        }

        public static bool IsRunningRobotCode()
        {
            return isRunningRobotCode;
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
                    Arguments = " -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel " + EMULATION_DIR + Path.DirectorySeparatorChar + "zImage " +
                            "-dtb " + EMULATION_DIR + Path.DirectorySeparatorChar + "zynq-zed.dtb " +
                            "-display none -serial null -serial mon:stdio -append \"console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0 rw\" " +
                            "-net user,hostfwd=tcp::" + DEFAULT_SSH_PORT_CPP + "-:22,hostfwd=tcp::" + EmulatorNetworkConnection.DEFAULT_NATIVE_PORT + "-:" + EmulatorNetworkConnection.DEFAULT_NATIVE_PORT + "," +
                            "hostfwd=tcp::2354-:2354 -net nic -sd " + EMULATION_DIR + Path.DirectorySeparatorChar + "rootfs.ext4",
                    Verb = "runas"
                });
                qemuJavaProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = @"C:\Program Files\qemu\qemu-system-x86_64.exe",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    Arguments = " -m 2048 -kernel " + EMULATION_DIR + "kernel-java -nographic -append \"console=ttyPS0 root=/dev/sda rw\" -net user,hostfwd=tcp::" + DEFAULT_SSH_PORT_JAVA + "-:22,hostfwd=tcp::" + EmulatorNetworkConnection.DEFAULT_JAVA_PORT + "-:" + EmulatorNetworkConnection.DEFAULT_PORT + " -net nic -hda " + EMULATION_DIR + "rootfs-java.ext4",
                    Verb = "runas"
                });

                grpcBridgeProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = EMULATION_DIR + "grpc-bridge.exe",
                    Verb = "runas"
                });
            }
            catch (Exception)
            {
                exception = true;
            }
            return !exception && IsVMRunning();
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

        private static void StatusUpdater()
        {
            while (updatingStatus)
            {
                try
                {
                    VMConnected = Client.IsConnected;
                    if (VMConnected)
                    {
                        frcUserProgramPresent = Client.RunCommand(CHECK_EXISTS_COMMAND).ExitStatus == 0;
                        isRunningRobotCode = Client.RunCommand(CHECK_RUNNING_COMMAND).ExitStatus == 0;
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
        }

        public static void StartUpdatingStatus()
        {
            if (!updatingStatus)
            {
                updatingStatus = true;
                Task.Run(StatusUpdater);
            }
        }

        public static void StopUpdatingStatus()
        {
            updatingStatus = false;
        }

        public static Task SCPFileSender(UserProgram userProgram, bool autorun = true)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (IsRunningRobotCode() || IsTryingToRunRobotCode())
                        await StopRobotCode();

                    isUserProgramFree = false;
                    Client.RunCommand("rm FRCUserProgram FRCUserProgram.jar"); // Delete existing files so the frc program chooser knows which to run
                    frcUserProgramPresent = false;

                    using (ScpClient scpClient = new ScpClient(EmulatorNetworkConnection.DEFAULT_HOST, programType == UserProgram.UserProgramType.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                    {
                        scpClient.Connect();
                        using (Stream localFile = File.OpenRead(userProgram.fullFileName))
                        {
                            scpClient.Upload(localFile, @"/home/lvuser/" + userProgram.targetFileName);
                            frcUserProgramPresent = true;
                        }
                        scpClient.Disconnect();
                    }
                    isUserProgramFree = true;
                    if(UseEmulation && autorun)
                        await RestartRobotCode();
                }
                catch (Exception) {
                    isUserProgramFree = true;
                }
            });
        }

        public static Task StopRobotCode()
        {
            return Task.Run(() =>
            {
                isTryingToRunRobotCode = false;
                isUserProgramFree = false;
                if (isRunningRobotCode)
                    Client.RunCommand(STOP_COMMAND);
                isRunningRobotCode = false;
                isUserProgramFree = true;
            });
        }

        public static Task RestartRobotCode()
        {
            return Task.Run(() =>
            {
                isTryingToRunRobotCode = true;
                isUserProgramFree = false;
                if (isRunningRobotCode)
                {
                    isTryingToRunRobotCode = false; // To reset the gRPC connection
                    Client.RunCommand(STOP_COMMAND);
                }
                isTryingToRunRobotCode = true;
                isRunningRobotCode = true;
                EmulatorNetworkConnection.Instance.OpenConnection();
                Client.RunCommand(START_COMMAND);
                isUserProgramFree = true;
            });
        }

        public static StreamReader CreateRobotOutputStream()
        {
            var command = Client.CreateCommand(RECEIVE_PRINTS_COMMAND);
            command.BeginExecute();
            return new StreamReader(command.OutputStream);
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
                        client.Download(REMOTE_LOG_NAME, localLogFile);
                        localLogFile.Close();
                        client.Disconnect();
                    }
                }
            });
        }
    }
}
