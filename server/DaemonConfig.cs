using System;

namespace SynthesisServer
{
    public class DaemonConfig
    {
        public String Version { get; set; }
        public String DaemonName { get; set; }
        public int Port { get; set; }
        public int Timeout { get; set; }
        public int MaxHosts { get; set; }
        public int HeartbeatInterval { get; set; }
    }

}