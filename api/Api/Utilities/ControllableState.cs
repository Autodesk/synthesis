using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Mirabuf.Signal;
using Mirabuf;
using Google.Protobuf;
using System.Net.Sockets;

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
                System.Diagnostics.Debug.WriteLine("Running CurrentSignalLayout set method");
                _currentSignalLayout = value;
                CurrentSignals = new Dictionary<string, UpdateSignal>();
                Owner = null;
                Generation = 1;
                Guid = ByteString.CopyFromUtf8(value.Info.GUID);
                foreach (var kvp in value.SignalMap)
                {
                    CurrentSignals[kvp.Key] = new UpdateSignal
                    {
                        Io = kvp.Value.Io == IOType.Input ? UpdateIOType.Input : UpdateIOType.Output,
                        DeviceType = kvp.Value.DeviceType
                    };
                }
            }
        }

        public Dictionary<string, UpdateSignal> CurrentSignals { get; private set; }
        public int Generation { get; private set; }
        public ByteString Guid { get; set; }
        public TcpClient? Owner { get; set; }
        
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

        public void ReleaseResource()
        {
            Owner = null;
            Generation += 1;
        }
    }
}
