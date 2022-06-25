using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer
{
    public enum Command
    {
        INVALID,
        START,
        STOP,
        RESTART
    }
    public interface ICommand
    {
        public Command CommandType { get; }
        public void Execute(DaemonConfig currentConfig);
    }
}
