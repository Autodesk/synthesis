using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.DevelopmentTools
{
    public class ProfilerMarker
    {
        public DateTime _startTimestamp { get; private set; }

        public ProfilerMarker()
        {
            _startTimestamp = DateTime.Now;
        }

        public TimeSpan TimeSinceCreation => DateTime.Now.Subtract(_startTimestamp);
    }
}
