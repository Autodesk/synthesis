using System;
using System.Text;

namespace SynthesisAPI.EventBus
{
    public interface IEvent 
    {
        public string EventType { get; internal set; }

        public byte[] getData();
    }

}
