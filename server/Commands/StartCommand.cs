using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;


namespace SynthesisServer
{
    [Verb("start", HelpText = "Start the Synthesis Server")]
    class StartCommand : ICommand
    {
        public Command Execute()
        {
            return Command.START;
        }
    }
}
