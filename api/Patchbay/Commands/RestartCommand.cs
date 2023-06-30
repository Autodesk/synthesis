using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer
{
    [Verb("restart", HelpText = "Restart the Synthesis Server if it is already running")]
    class RestartCommand
    {
        [Option('t', "timeout", Required = false, HelpText = "Sets the time (ms) that the server will wait for all other instances to die (Defaults to 5000 milliseconds)")]
        public int Timeout { get; set; } = 5000;

        [Option('f', "force", Required = false, HelpText = "Server will start regaurdless of whether all other instances are alive (Defaults to false which is strongly recommended)")]
        public bool Force { get; set; } = false;

        public Command CommandType { get; } = Command.RESTART;
    }
}
