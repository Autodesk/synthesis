using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SynthesisServer
{
        /* Properties are assigned default values. Then values are updated based on available JSON config. Finally Command Line arguments are applied*/
    public class DaemonConfig
    {
        // Command line arguments
        public string[] Arguments { get; set; }

        // Configurable values
        public string Version { get; set; } = "1.0";
        public string DaemonName { get; set; } = "synthesisd";
        public int Port { get; set; } = 10800;
        public int Timeout { get; set; } = 43200;
        public int MaxLobbies { get; set; } = -1;
        public int HeartbeatInterval { get; set; } = 5;
    }

}