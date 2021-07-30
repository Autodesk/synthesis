using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Mirabuf.Signal;
using Mirabuf;

namespace SynthesisAPI.Utilities
{
    /*
     * ControllableState stores a Signals object from mirabuf called CurrentSignalLayout.
     * Whenever the CurrentSignalLayout is set, it will fill the CurrentSignals dictionary with UpdateSignal
     * objects according to the CurrentSignalLayout. The Update method is used to update the CurrentSignals in 
     * the Controllable state and will throw an exception if you try to change an UpdateSignal not in CurrentSignals
     */
    public class ControllableState
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
        public Dictionary<string, UpdateSignal> CurrentSignals { get; private set; } = new Dictionary<string, UpdateSignal>();
        
        public void Update(UpdateSignals updateSignals)
        {
            foreach (var kvp in updateSignals.SignalMap)
            {
                if (CurrentSignals.ContainsKey(kvp.Key))
                {
                    CurrentSignals[kvp.Key] = kvp.Value;
                }
                else
                {
                    throw new SynthesisException("Layout does not contain key: " + kvp.Key);
                }    
            }
        }
    }
}
