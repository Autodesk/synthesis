using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer
{
    [Verb("stop", HelpText = "Stop the Synthesis Server")]
    public class StopCommand : ICommand
    {
        public Command CommandType { get; } = Command.STOP;
        public Command Execute(DaemonConfig currentConfig)
        {
            return Command.STOP;
        }
    }
}
