using System;
using Renci.SshNet;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Synthesis.GUI;

namespace Synthesis
{
    public static class EmulatorManager
    {
        // Directory Info
        private static readonly string EMULATION_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Emulator";

        /// <summary>
        /// The running OS, used to make emulator process management cross-platform
        /// </summary>
        private enum OS
        {
            Windows,
            MacOS,
            Linux
        }

#if UNITY_STANDALONE_OSX
        private static readonly OS os = OS.MacOS;
#elif UNITY_STANDALONE_LINUX
        private static readonly OS os = OS.Linux;
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        private static readonly OS os = OS.Windows;
#endif
        private static readonly string QEMU_DIR = "C:" + Path.DirectorySeparatorChar + "Program Files" + Path.DirectorySeparatorChar + "qemu";

        // File Info
        private static readonly string NATIVE_KERNEL = EMULATION_DIR + Path.DirectorySeparatorChar + "kernel-native";
        private static readonly string ROOTFS_NATIVE = EMULATION_DIR + Path.DirectorySeparatorChar + "rootfs-native.ext4";
        private static readonly string NATIVE_DEVICE_TREE = EMULATION_DIR + Path.DirectorySeparatorChar + "zynq-zed.dtb";

        private static readonly string JAVA_KERNEL = EMULATION_DIR + Path.DirectorySeparatorChar + "kernel-java";
        private static readonly string ROOTFS_JAVA = EMULATION_DIR + Path.DirectorySeparatorChar + "rootfs-java.ext4";

        private static readonly string GRPC_BRIDGE = EMULATION_DIR + Path.DirectorySeparatorChar + "grpc-bridge" + (os == OS.Windows ? ".exe" : (os == OS.MacOS ? "-macos" : ""));
        private static readonly string QEMU_ARM = os == OS.Windows ? (QEMU_DIR + Path.DirectorySeparatorChar + "qemu-system-arm.exe") : "qemu-system-arm";
        private static readonly string QEMU_X86 = os == OS.Windows ? (QEMU_DIR + Path.DirectorySeparatorChar + "qemu-system-x86_64.exe") : "qemu-system-x86_64";

        // SSH Info
        public const string DEFAULT_HOST = "127.0.0.1";
        private const int DEFAULT_SSH_PORT_CPP = 10022;
        private const int DEFAULT_SSH_PORT_JAVA = 10023;

        private const string USER = "lvuser";
        private const string PASSWORD = "";

        // Commands
        private const string REMOTE_LOG_NAME = "/home/lvuser/logs/log.log";

        private const string STOP_COMMAND = "sudo killall frc_program_runner.sh FRCUserProgram java &> /dev/null;";
        private const string START_COMMAND = "nohup /home/lvuser/frc_program_runner.sh </dev/null &> /dev/null &";
        private const string CHECK_EXISTS_COMMAND = "[ -f /home/lvuser/FRCUserProgram ] || [ -f /home/lvuser/FRCUserProgram.jar ]";
        private const string CHECK_RUNNER_RUNNING_COMMAND = "pidof frc_program_runner.sh &> /dev/null";
        private const string CHECK_RUNNING_COMMAND = "pidof FRCUserProgram java &> /dev/null";
        private const string RECEIVE_PRINTS_COMMAND = "tail -F " + REMOTE_LOG_NAME;

        private static SshCommand outputStreamCommand = null;
        private static IAsyncResult outputStreamCommandResult = null;

        // Process Info
        private static System.Diagnostics.Process qemuNativeProcess = null;
        private static System.Diagnostics.Process qemuJavaProcess = null;
        private static System.Diagnostics.Process grpcBridgeProcess = null;

        public static UserProgram.Type programType = UserProgram.Type.JAVA;
        
		// Last connection status
        public static bool UseEmulation = false;
        private static bool VMConnected = false;
        private static bool frcUserProgramPresent = false;
        private static bool isTryingToRunRobotCode = false;
        private static bool isRunningRobotCodeRunner = false;
        private static bool isRunningRobotCode = false;
        private static bool isRobotCodeRestarting = false;

        private static bool updatingStatus = false;

        /// <summary>
        /// Handles the SSH connection into the active virtual machine
        /// 
        /// This allows us to reuse the connection, which makes running commands faster
        /// </summary>
        private static class ClientManager
        {
            /// <summary>
            /// Connect to the active VM if not connected
            /// </summary>
            public static void Connect()
            {
                if (emulatorType != programType && Instance != null) // Reset connection if the active VM switched
                {
                    Close();
                }
                if (Instance == null)
                {
                    emulatorType = programType;
                    Instance = new SshClient(DEFAULT_HOST, (emulatorType == UserProgram.Type.JAVA) ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD);
                }
                if (!Instance.IsConnected)
                {
                    try
                    {
                        Instance.Connect();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            /// <summary>
            /// Clean up connection
            /// </summary>
            public static void Close()
            {
                if (Instance != null)
                {
                    if (Instance.IsConnected)
                    {
                        Instance.Disconnect();
                    }
                    Instance.Dispose();
                    Instance = null;
                }
            }

            public static SshClient Instance { get; private set; }

            private static UserProgram.Type emulatorType = UserProgram.Type.JAVA;
        }

        /// <summary>
        /// Check if all necessary emulation files exist
        /// </summary>
        /// <returns>True if all files exist</returns>
        public static bool IsVMInstalled()
        {
            return Directory.Exists(EMULATION_DIR) &&
                Directory.Exists(QEMU_DIR) == (os == OS.Windows) &&
                File.Exists(NATIVE_KERNEL) &&
                File.Exists(ROOTFS_NATIVE) &&
                File.Exists(NATIVE_DEVICE_TREE) &&
                File.Exists(JAVA_KERNEL) &&
                File.Exists(ROOTFS_JAVA) &&
                File.Exists(GRPC_BRIDGE) &&
                File.Exists(QEMU_ARM) &&
                File.Exists(QEMU_X86);
        }

        /// <summary>
        /// Check if all emulation sub-processes are running
        /// </summary>
        /// <returns>True if if all processes are running</returns>
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

        /// <summary>
        /// Check if connected to the active VM
        /// </summary>
        /// <returns>True if connected</returns>
        public static bool IsVMConnected()
        {
            return VMConnected;
        }

        /// <summary>
        /// Check if a user program exists in the active VM
        /// </summary>
        /// <returns>True if it exists</returns>
        public static bool IsFRCUserProgramPresent()
        {
            return frcUserProgramPresent;
        }

        /// <summary>
        /// Check if we're running or trying to run a user program
        /// </summary>
        /// <returns>True if it's trying to be run</returns>
        public static bool IsTryingToRunRobotCode()
        {
            return isTryingToRunRobotCode || isRunningRobotCode;
        }

        /// <summary>
        /// Check if the user program runner is running
        /// </summary>
        /// <returns>True if it's running</returns>
        public static bool IsRunningRobotCodeRunner()
        {
            return isRunningRobotCodeRunner;
        }

        /// <summary>
        /// Check if the user program itself is running
        /// </summary>
        /// <returns>True if it's running</returns>
        public static bool IsRunningRobotCode()
        {
            return isRunningRobotCode;
        }

        /// <summary>
        /// Check if it's in the middle of restarting the user program
        /// </summary>
        /// <returns>True if it's restarting</returns>
        public static bool IsRobotCodeRestarting()
        {
            return isRobotCodeRestarting;
        }

        /// <summary>
        /// Boot all emulation sub-processes
        /// </summary>
        /// <returns>True if successful</returns>
        public static bool StartEmulator()
        {
            if (IsVMRunning())
            {
                throw new Exception("Emulator already running");
            }
            bool exception = false;
            try
            {
                qemuNativeProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = QEMU_ARM,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    Arguments = " -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel " + NATIVE_KERNEL + " -dtb " + NATIVE_DEVICE_TREE +" " +
                        "-display none -serial null -serial mon:stdio -append \"console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0 rw\" " +
                        "-net user,hostfwd=tcp::" + DEFAULT_SSH_PORT_CPP + "-:22,hostfwd=tcp::" + EmulatorNetworkConnection.DEFAULT_NATIVE_PORT + "-:" + EmulatorNetworkConnection.DEFAULT_PORT + "," +
                        "hostfwd=tcp::2354-:2354 -net nic -sd " + ROOTFS_NATIVE,
                    Verb = "runas"
                });

                qemuJavaProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = QEMU_X86,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    Arguments = " -m 2048 -kernel " + JAVA_KERNEL + " -nographic -append \"console=ttyPS0 root=/dev/sda rw\" " +
                        "-net user,hostfwd=tcp::" + DEFAULT_SSH_PORT_JAVA + "-:22,hostfwd=tcp::" + EmulatorNetworkConnection.DEFAULT_JAVA_PORT + "-:" + EmulatorNetworkConnection.DEFAULT_PORT + " " +
                        "-net nic -hda " + ROOTFS_JAVA,
                    Verb = "runas"
                });

                grpcBridgeProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = GRPC_BRIDGE,
                    Verb = "runas"
                });

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                {
                    KillEmulator();
                };
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                exception = true;
            }

            StartUpdatingStatus();
            return !exception && IsVMRunning();
        }

        /// <summary>
        /// Kill all emulation tasks and sub-processes
        /// </summary>
        public static void KillEmulator()
        {
            if (VMConnected)
            {
                ClientManager.Close();
            }

            StopUpdatingStatus();

            VMConnected = false;
            frcUserProgramPresent = false;
            isTryingToRunRobotCode = false;
            isRunningRobotCode = false;
            isRunningRobotCodeRunner = false;

            if (IsVMRunning())
            {
                qemuNativeProcess.Kill();
                qemuNativeProcess = null;
                qemuJavaProcess.Kill();
                qemuJavaProcess = null;
                grpcBridgeProcess.Kill();
                grpcBridgeProcess = null;
            }
        }

        /// <summary>
        /// Gracefully kill all emulation tasks and sub-processes
        /// </summary>
        public static async void GracefulExit()
        {
            if (IsVMConnected())
            {
                if (IsRunningRobotCodeRunner())
                {
                    await StopRobotCode();
                }
            }
            if (IsVMRunning())
            {
                KillEmulator();
            }
        }

        /// <summary>
        /// Continually update all VM status fields
        /// </summary>
        private static void StatusUpdater()
        {
            var lastProgramType = programType;
            SshClient client = null;
            while (updatingStatus)
            {
                if (programType != lastProgramType)
                {
                    lastProgramType = programType;
                    if(client != null)
                    {
                        client.Disconnect();
                        client.Dispose();
                        client = null;
                    }
                }
                try
                {
                    if (client == null)
                    {
                        client = new SshClient(DEFAULT_HOST, lastProgramType == UserProgram.Type.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD);
                        client.Connect();
                    }

                    VMConnected = client.IsConnected;
                    if (VMConnected)
                    {
                        frcUserProgramPresent = client.RunCommand(CHECK_EXISTS_COMMAND).ExitStatus == 0;
                        if (frcUserProgramPresent)
                        {
                            isRunningRobotCodeRunner = client.RunCommand(CHECK_RUNNER_RUNNING_COMMAND).ExitStatus == 0;
                            isRunningRobotCode = client.RunCommand(CHECK_RUNNING_COMMAND).ExitStatus == 0;
                            if (isRunningRobotCode && !EmulatorNetworkConnection.Instance.IsConnectionOpen())
                            {
                                EmulatorNetworkConnection.Instance.OpenConnection();
                            }
                            if (isRunningRobotCode)
                            {
                                Thread.Sleep(3000); // ms
                            }
                            else
                            {
                                Thread.Sleep(1000); // ms
                            }
                        }
                        else
                        {
                            isRunningRobotCodeRunner = false;
                            isRunningRobotCode = false;
                            Thread.Sleep(1000); // ms
                        }
                    }
                    else
                    {
                        frcUserProgramPresent = false;
                        isRunningRobotCodeRunner = false;
                        isRunningRobotCode = false;
                        Thread.Sleep(3000); // ms
                        client.Connect();
                    }
                }
                catch
                {
                    frcUserProgramPresent = false;
                    isRunningRobotCodeRunner = false;
                    isRunningRobotCode = false;
                    Thread.Sleep(3000); // ms
                }
            }
        }

        /// <summary>
        /// Start background task to update all VM statuses
        /// </summary>
        public static void StartUpdatingStatus()
        {
            if (!updatingStatus)
            {
                updatingStatus = true;
                Task.Run(StatusUpdater);
            }
        }

        /// <summary>
        /// Stop background status-updater task
        /// </summary>
        public static void StopUpdatingStatus()
        {
            updatingStatus = false;
        }

        /// <summary>
        /// Send a user program to the active VM
        /// </summary>
        /// <param name="userProgram">The user program to upload</param>
        /// <param name="autorun">True to run the user program automatically</param>
        /// <returns>True if successful</returns>
        public static Task<bool> SCPFileSender(UserProgram userProgram, bool autorun = true)
        {
            return Task.Run(async () =>
            {
                try
                {
                    isRobotCodeRestarting = true; // Prevent code from auto-restarting until this finishes
                    if (IsRunningRobotCodeRunner() || IsTryingToRunRobotCode())
                    {
                        await StopRobotCode();
                    }
                    if (IsRobotOutputStreamGood())
                    {
                        await CloseRobotOutputStream();
                    }

                    programType = userProgram.ProgramType;
                    ClientManager.Connect();
                    ClientManager.Instance.RunCommand("rm -rf FRCUserProgram FRCUserProgram.jar");
                    frcUserProgramPresent = false;
                    using (ScpClient scpClient = new ScpClient(DEFAULT_HOST, programType == UserProgram.Type.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                    {
                        scpClient.Connect();
                        using (Stream localFile = File.OpenRead(userProgram.FullFileName))
                        {
                            scpClient.Upload(localFile, "/home/lvuser/" + userProgram.TargetFileName);
                            frcUserProgramPresent = true;
                        }
                        scpClient.Disconnect();
                    }
                    if (UseEmulation && autorun)
                    {
                        await RestartRobotCode();
                    }
                    else
                    {
                        isRobotCodeRestarting = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// Stop running the user program in the active VM
        /// </summary>
        /// <returns>True if successful</returns>
        public static Task<bool> StopRobotCode()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (isRunningRobotCodeRunner || isRunningRobotCode)
                    {
                        ClientManager.Connect();
                        ClientManager.Instance.RunCommand(STOP_COMMAND);
                    }
                    isTryingToRunRobotCode = false;
                    isRunningRobotCodeRunner = false;
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// Restart the running user program in the active VM
        /// </summary>
        /// <returns>True if successful</returns>
        public static Task<bool> RestartRobotCode()
        {
            return Task.Run(() =>
            {
                isRobotCodeRestarting = true;

                try
                {
                    ClientManager.Connect();
                    if (isRunningRobotCodeRunner)
                    {
                        isTryingToRunRobotCode = false; // To reset the gRPC connection
                        isRunningRobotCodeRunner = false;
                        ClientManager.Instance.RunCommand(STOP_COMMAND + "&&" + START_COMMAND);
                    }
                    else
                    {
                        ClientManager.Instance.RunCommand(START_COMMAND);
                    }
                    isTryingToRunRobotCode = true;
                    isRunningRobotCodeRunner = true;
                    EmulatorNetworkConnection.Instance.OpenConnection();
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    isRobotCodeRestarting = false;
                    return false;
                }
                isRobotCodeRestarting = false;
                return true;
            });
        }

        /// <summary>
        /// Check if the robot output stream is alive and not completed
        /// </summary>
        /// <returns>True if good</returns>
        public static bool IsRobotOutputStreamGood()
        {
            return outputStreamCommand != null && !outputStreamCommandResult.IsCompleted;
        }

        /// <summary>
        /// Close the robot output stream
        /// </summary>
        /// <returns>True if successful</returns>
        public static Task<bool> CloseRobotOutputStream()
        {
            return Task.Run(() =>
            {
                if (outputStreamCommand != null)
                {
                    try
                    {
                        outputStreamCommand.CancelAsync();
                        outputStreamCommand.Dispose();
                        outputStreamCommand = null;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.ToString());
                        return false;
                    }
                }
                return true;
            });
        }

        /// <summary>
        /// Open the robot output stream
        /// </summary>
        /// <returns>A stream reader to read from the robot output stream</returns>
        public static StreamReader CreateRobotOutputStream()
        {
            if (outputStreamCommand == null) {
                try
                {
                    ClientManager.Connect();
                    outputStreamCommand = ClientManager.Instance.CreateCommand(RECEIVE_PRINTS_COMMAND);
                    outputStreamCommandResult = outputStreamCommand.BeginExecute();
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    outputStreamCommand = null;
                    return null;
                }
            }
            return new StreamReader(outputStreamCommand.OutputStream);
        }

        /// <summary>
        /// Download the robot output log from the active VM
        /// </summary>
        /// <returns>True if successful</returns>
        public static Task<bool> FetchLogFile()
        {
            return Task.Run(() =>
            {
                string folder = SFB.StandaloneFileBrowser.OpenFolderPanel("Log file destination", "C:\\", false);
                if (folder == null)
                {
                    Debug.Log("No folder selected for log file destination");
                    return true;
                }
                else
                {
                    try
                    {
                        using (ScpClient client = new ScpClient(DEFAULT_HOST, programType == UserProgram.Type.JAVA ? DEFAULT_SSH_PORT_JAVA : DEFAULT_SSH_PORT_CPP, USER, PASSWORD))
                        {
                            client.Connect();
                            Stream localLogFile = File.Create(folder + "/log.log");
                            client.Download(REMOTE_LOG_NAME, localLogFile);
                            localLogFile.Close();
                            client.Disconnect();
                        }
                    }
                    catch (Exception)
                    {
                        UserMessageManager.Dispatch("Failed to download log file", EmulationWarnings.WARNING_DURATION);
                        return false;
                    }
                }
                return true;
            });
        }
    }
}
