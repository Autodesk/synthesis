using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer
{
    [Verb("restart", HelpText = "Restart the Synthesis Server if it is already running")]
    class RestartCommand : StartCommand
    {
        public new Command CommandType { get; } = Command.RESTART;
        public new void Execute(DaemonConfig currentConfig)
        {

        }
    }
}
