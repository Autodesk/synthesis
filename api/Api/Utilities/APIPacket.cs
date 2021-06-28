using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.Utilities
{
    public class APIPacket
    {
        public string Id { get; set; }
        public List<DigitalOutput> Data { get; set; }

    }

}
