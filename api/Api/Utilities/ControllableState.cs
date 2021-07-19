using SynthesisAPI.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.Utilities
{
    class ControllableState
    {
        
        private Layout? _currentLayout;
        public Layout? CurrentLayout 
        { 
            get => _currentLayout;
            set
            {
                _currentLayout = value;
                Fields.DOs.Clear();
                Fields.DIs.Clear();
                Fields.AOs.Clear();
                Fields.AIs.Clear();
                foreach (var entry in value.Fields.DOEntries)
                {
                    Fields.DOs[entry.Key] = new DigitalOutput()
                    {
                        Name = entry.Value.Name,
                        Type = entry.Value.Type
                    };
                }
                foreach (var entry in value.Fields.DIEntries)
                {
                    Fields.DIs[entry.Key] = new DigitalInput()
                    {
                        Name = entry.Value.Name,
                        Type = entry.Value.Type
                    };
                }
                foreach (var entry in value.Fields.AOEntries)
                {
                    Fields.AOs[entry.Key] = new AnalogOutput()
                    {
                        Name = entry.Value.Name,
                        Type = entry.Value.Type
                    };
                }
                foreach (var entry in value.Fields.AIEntries)
                {
                    Fields.AIs[entry.Key] = new AnalogInput()
                    {
                        Name = entry.Value.Name,
                        Type = entry.Value.Type
                    };
                }
            }
        }

        
        public UpdateMessage.Types.ModifiedFields Fields { get; set; }

    }
}
