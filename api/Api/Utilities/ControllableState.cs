using SynthesisAPI.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Utilities
{
    class ControllableState
    {
        
        public Layout? currentLayout { get; set; }
        public Dictionary<string, DigitalOutput> DOs { get; set; }
        public Dictionary<string, DigitalInput> DIs { get; set; }
        public Dictionary<string, AnalogOutput> AOs { get; set; }
        public Dictionary<string, AnalogInput> AIs { get; set; }

        public void SetLayout(Layout layout)
        {
            currentLayout = layout;
            DOs.Clear();
            DIs.Clear();
            AOs.Clear();
            AIs.Clear();
            foreach (var entry in layout.Fields.DOEntries)
            {
                DOs[entry.Key] = new DigitalOutput()
                {
                    Name = entry.Value.Name,
                    Type = entry.Value.Type
                };
            }
            foreach (var entry in layout.Fields.DIEntries)
            {
                DIs[entry.Key] = new DigitalInput()
                {
                    Name = entry.Value.Name,
                    Type = entry.Value.Type
                };
            }
            foreach (var entry in layout.Fields.AOEntries)
            {
                AOs[entry.Key] = new AnalogOutput()
                {
                    Name = entry.Value.Name,
                    Type = entry.Value.Type
                };
            }
            foreach (var entry in layout.Fields.AIEntries)
            {
                AIs[entry.Key] = new AnalogInput()
                {
                    Name = entry.Value.Name,
                    Type = entry.Value.Type
                };
            }
        }

        public class DigitalOutput
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public double Value { get; set; }
        }

        public class DigitalInput
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public double Value { get; set; }
        }

        
        public class AnalogOutput
        {
            
            public string Name { get; set; }
            
            public string Type { get; set; }
            
            public double Value { get; set; }
        }

        public class AnalogInput
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public double Value { get; set; }
        }
    }
}
