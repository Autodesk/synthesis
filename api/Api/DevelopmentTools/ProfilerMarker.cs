using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.DevelopmentTools
{
    public class ProfilerMarker
    {
        private DateTime _startTimestamp { get; set; }

        public ProfilerMarker()
        {
            _startTimestamp = DateTime.Now;
        }

        public TimeSpan TimeSinceCreation => DateTime.Now.Subtract(_startTimestamp);
    }
}
