using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Mirabuf.Signal;
using Mirabuf;
using Google.Protobuf;

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
                CurrentSignals.Clear();
                Generation = 0;
                IsFree = true;
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

        //public Info CurrentInfo { get; private set; }
        public Dictionary<string, UpdateSignal> CurrentSignals { get; private set; } = new Dictionary<string, UpdateSignal>();
        public bool IsFree { get; set; }
        public int Generation { get; set; }
        public ByteString Guid { get; set; }
        
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

        /*
        public class ResourceInfo : IEquatable<ResourceInfo>
        {
            public string ResourceName { get; private set; }
            public ByteString Guid { get; private set; }
            public int? Version { get; private set; }

            public ResourceInfo(string name, ByteString guid) { ResourceName = name; Guid = guid; }
            public ResourceInfo(ByteString guid) { Guid = guid; }
            public ResourceInfo(string name) { ResourceName = name; Guid = ByteString.Empty; }

            public override int GetHashCode()
            {
                return default;
            }
            public bool Equals(ResourceInfo other)
            {
                if (other == null)
                    return false;

                if (this.Guid.Equals(other.Guid) || (other.Guid.IsEmpty && this.ResourceName.Equals(other.ResourceName)))
                    return true;
                else
                    return false;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                ResourceInfo resourceInfoObj = obj as ResourceInfo;
                if (resourceInfoObj == null)
                    return false;
                else
                    return Equals(resourceInfoObj);
            }
        }
        */
    }
}
