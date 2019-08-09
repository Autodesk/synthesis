using Synthesis.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis
{
    public class EmulationWarnings
    {
        public const float WARNING_DURATION = 10; // s

        public enum Requirement
        {
            VMConnected,
            UserProgramPresent,
            UserProgramConnected,
        }

        public static bool CheckRequirement(Requirement requirement)
        {
            switch(requirement)
            {
                case Requirement.VMConnected:
                    if (!EmulatorManager.IsVMConnected())
                    {
                        UserMessageManager.Dispatch("Waiting for emulator to boot", WARNING_DURATION);
                        return false;
                    }
                    return true;
                case Requirement.UserProgramPresent:
                    if (!CheckRequirement(Requirement.VMConnected))
                    {
                        return false;
                    }
                    else if(!EmulatorManager.IsFRCUserProgramPresent())
                    {
                        UserMessageManager.Dispatch("User program not found", WARNING_DURATION);
                        return false;
                    }
                    return true;
                case Requirement.UserProgramConnected:
                    if (!CheckRequirement(Requirement.UserProgramPresent))
                    {
                        return false;
                    }
                    else if (!EmulatorManager.IsRunningRobotCode() || !EmulatorNetworkConnection.Instance.IsConnected())
                    {
                        UserMessageManager.Dispatch("User program not connected (is it running?)", WARNING_DURATION);
                        return false;
                    }
                    return true;
                default:
                    throw new NotSupportedException("Unknown emulation requirement");
            }
        }
    }
}
