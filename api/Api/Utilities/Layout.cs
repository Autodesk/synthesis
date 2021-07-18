using Google.Protobuf.WellKnownTypes;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.Utilities.Messages
{
    [ProtoContract]
    public class Layout
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public ModifiedFields Fields { get; set; }

        [ProtoContract]
        public class ModifiedFields
        {
            [ProtoMember(1)]
            public Dictionary<string, DigitalOutput> DOEntries { get; set; }
            [ProtoMember(2)]
            public Dictionary<string, DigitalInput> DIEntries { get; set; }
            [ProtoMember(3)]
            public Dictionary<string, AnalogOutput> AOEntries { get; set; }
            [ProtoMember(4)]
            public Dictionary<string, AnalogInput> AIEntries { get; set; }
        }
    }

    [ProtoContract]
    public class DigitalOutputEntry
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Type { get; set; }
    }

    [ProtoContract]
    public class DigitalInputEntry
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Type { get; set; }
    }

    [ProtoContract]
    public class AnalogOutputEntry
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Type { get; set; }
    }

    [ProtoContract]
    public class AnalogInputEntry
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Type { get; set; }
    }

}
