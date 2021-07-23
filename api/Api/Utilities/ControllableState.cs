using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Mirabuf.Signal;
using Mirabuf;

namespace SynthesisAPI.Utilities
{
    class ControllableState
    {
        private Signals? _currentSignalLayout;
        public Signals? CurrentSignalLayout
        {
            get => _currentSignalLayout;
            set
            {
                _currentSignalLayout = value;
                CurrentSignals.Clear();
                CurrentInfo = value.Info;
                foreach (var kvp in value.SignalMap)
                {
                    CurrentSignals[kvp.Key] = new UpdateSignal
                    {
                        Io = kvp.Value.Io == IOType.Input ? UpdateIOType.Input : UpdateIOType.Output,
                        Class = kvp.Value.Class
                    };
                }
            }
        }

        public Info CurrentInfo { get; private set; }
        public Dictionary<string, UpdateSignal> CurrentSignals { get; private set; }
        
        public void Update(UpdateSignals updateSignals)
        {
            foreach (var kvp in updateSignals.SignalMap)
            {
                CurrentSignals[kvp.Key] = kvp.Value;
            }
        }
    }
}
