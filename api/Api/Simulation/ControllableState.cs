using System;
using System.Collections.Generic;
using Mirabuf.Signal;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.Simulation
{
    /*
     * ControllableState stores a Signals object from mirabuf called CurrentSignalLayout.
     * Whenever the CurrentSignalLayout is set, it will fill the CurrentSignals dictionary with UpdateSignal
     * objects according to the CurrentSignalLayout. The Update method is used to update the CurrentSignals in 
     * the Controllable state and will throw an exception if you try to change an UpdateSignal not in CurrentSignals
     */
    public class ControllableState
    {
        private int _generation;
        private bool _isFree;
        private Signals _currentSignalLayout;
        public Signals CurrentSignalLayout
        {
            get => _currentSignalLayout;
            set
            {
                _currentSignalLayout = value;
                CurrentSignals = new Dictionary<string, UpdateSignal>();
                if (value.Info == null)
                {
                    Guid = System.Guid.NewGuid().ToString();
                }
                else
                {
                    Guid = value.Info.GUID;
                }

                foreach (var kvp in value.SignalMap)
                {
                    CurrentSignals[kvp.Key] = new UpdateSignal
                    {
                        Io = kvp.Value.Io == IOType.Input ? UpdateIOType.Input : UpdateIOType.Output,
                        DeviceType = Enum.GetName(typeof(DeviceType), kvp.Value.DeviceType), // Keeping a string for now
                        Value = Google.Protobuf.WellKnownTypes.Value.ForNumber(0.0)
                    };
                }
            }
        }

        public Dictionary<string, UpdateSignal> CurrentSignals { get; private set; }
        public string Guid { get; set; }

        public int Generation { get => _generation; }

        public bool IsFree
        {
            get => _isFree;
            set
            {
                if (!value) _isFree = false;
                else
                {
                    _generation += 1;
                    _isFree = true;
                }
            }
        }

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
                    Logger.Log("Layout does not contain key: " + kvp.Key, LogLevel.Debug);
                }    
            }
        }
    }
}
