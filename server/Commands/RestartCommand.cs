using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer
{
    class RestartCommand : ICommand
    {
        public Command Execute()
        {
            return Command.RESTART;
        }
    }
}
