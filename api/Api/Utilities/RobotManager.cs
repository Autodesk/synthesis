using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SynthesisAPI.Utilities
{
    class RobotManager
    {
        
        public Dictionary<string, ControllableState> Robots { get; set; }

        private static readonly Lazy<RobotManager> lazy = new Lazy<RobotManager>(() => new RobotManager());
        public static RobotManager Instance { get { return lazy.Value; } }
        private RobotManager()
        {
            Robots.Clear();
        }

        
        public void Update(Dictionary<string, UpdateMessage.Types.ModifiedFields> packets, ReaderWriterLockSlim packetLock) 
        {
            packetLock.EnterWriteLock();
            try
            {
                foreach (var kvp in packets)
                {
                    foreach (var digitalOutput in kvp.Value.DOs)
                    {
                        Robots[kvp.Key].Fields.DOs[digitalOutput.Key] = digitalOutput.Value;
                    }
                    foreach (var digitalInput in kvp.Value.DIs)
                    {
                        Robots[kvp.Key].Fields.DIs[digitalInput.Key] = digitalInput.Value;
                    }
                    foreach (var AnalogOutput in kvp.Value.AOs)
                    {
                        Robots[kvp.Key].Fields.AOs[AnalogOutput.Key] = AnalogOutput.Value;
                    }
                    foreach (var AnalogInput in kvp.Value.AIs)
                    {
                        Robots[kvp.Key].Fields.AIs[AnalogInput.Key] = AnalogInput.Value;
                    }

                }
            }
            finally
            {
                packetLock.ExitWriteLock();
            }
            
        }
    }
}
