using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer
{
    public enum Command
    {
        START,
        STOP,
        RESTART
    }
    interface ICommand
    {
        
        Command Execute();
    }
}
