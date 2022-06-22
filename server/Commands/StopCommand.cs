using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer
{
    public class StopCommand : ICommand
    {
        public Command Execute()
        {
            return Command.STOP;
        }
    }
}
