using Synthesis.GUI;
using System;

namespace Synthesis
{
    public class EmulationWarnings
    {
        public const float WARNING_DURATION = 3; // s

        public enum Requirement
        {
            UseEmulation,
            VMInstalled,
            VMRunning,
            VMConnected,
            UserProgramPresent,
            UserProgramRunnerRunning,
            UserProgramNotRestarting,
            UserProgramConnected,
        }

        public static bool CheckRequirement(Requirement requirement)
        {
            switch(requirement)
            {
                case Requirement.UseEmulation:
                    if (!EmulatorManager.UseEmulation)
                    {
                        UserMessageManager.Dispatch("Not configured to use emulation", WARNING_DURATION);
                        return false;
                    }
                    return true;
                case Requirement.VMInstalled:
                    if (!EmulatorManager.IsVMInstalled())
                    {
                        UserMessageManager.Dispatch("Emulator not installed", WARNING_DURATION);
                        return false;
                    }
                    return true;
                case Requirement.VMRunning:
                    if (!EmulatorManager.IsVMRunning())
                    {
                        if (CheckRequirement(Requirement.VMInstalled))
                        {
                            UserMessageManager.Dispatch("Emulator not running", WARNING_DURATION);
                        }
                        return false;
                    }
                    return true;
                case Requirement.VMConnected:
                    if (!EmulatorManager.IsVMConnected())
                    {
                        if(CheckRequirement(Requirement.VMRunning))
                        {
                            UserMessageManager.Dispatch("Waiting for emulator to boot", WARNING_DURATION);
                        }
                        return false;
                    }
                    return true;
                case Requirement.UserProgramPresent:
                    if(!EmulatorManager.IsFRCUserProgramPresent())
                    {
                        if (CheckRequirement(Requirement.VMConnected))
                        {
                            UserMessageManager.Dispatch("User program not found", WARNING_DURATION);
                        }
                        return false;
                    }
                    return true;
                case Requirement.UserProgramNotRestarting:
                    if (EmulatorManager.IsRobotCodeRestarting())
                    {
                        UserMessageManager.Dispatch("Waiting for user program to start", WARNING_DURATION);
                        return false;
                    }
                    return true;
                case Requirement.UserProgramRunnerRunning:
                    if (!EmulatorManager.IsRunningRobotCodeRunner())
                    {
                        if (CheckRequirement(Requirement.UserProgramPresent))
                        {
                            if(EmulatorManager.IsTryingToRunRobotCode())
                                UserMessageManager.Dispatch("Waiting for user program to start", WARNING_DURATION);
                            else
                                UserMessageManager.Dispatch("User program not running", WARNING_DURATION);
                        }
                        return false;
                    }
                    return true;
                case Requirement.UserProgramConnected:
                    if (!EmulatorNetworkConnection.Instance.IsConnected())
                    {
                        if (CheckRequirement(Requirement.UserProgramRunnerRunning))
                        {
                            UserMessageManager.Dispatch("Connecting to user program (may take a few seconds)", WARNING_DURATION);
                        }
                        return false;
                    }
                    return true;
                default:
                    throw new NotSupportedException("Unknown emulation requirement");
            }
        }
    }
}
