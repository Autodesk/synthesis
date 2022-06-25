using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer
{
    [Verb("restart", HelpText = "Restart the Synthesis Server if it is already running")]
    class RestartCommand : StartCommand
    {
        public Command CommandType { get; } = Command.RESTART;
        public new Command Execute(DaemonConfig currentConfig)
        {
            return Command.RESTART;
        }
    }
}
