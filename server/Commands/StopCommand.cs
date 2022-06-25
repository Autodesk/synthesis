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
        public void Execute(DaemonConfig currentConfig)
        {
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                foreach (var x in System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)))
                {
                    x.Kill();
                }
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
}
