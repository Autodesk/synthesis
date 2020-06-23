using System;
using System.Text;

namespace SynthesisAPI.EventBus
{
    public interface IEvent 
    {
        public string Type { get; internal set; }

        public byte[] getData();
    }

}
