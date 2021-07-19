using Google.Protobuf.WellKnownTypes;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.Utilities.Messages
{
    [ProtoContract]
    public class UpdateMessage
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public ModifiedFields Fields { get; set; }

        [ProtoContract]
        public class ModifiedFields
        {
            [ProtoMember(1)]
            public Dictionary<string, DigitalOutput> DOs { get; set; }
            [ProtoMember(2)]
            public Dictionary<string, DigitalInput> DIs { get; set; }
            [ProtoMember(3)]
            public Dictionary<string, AnalogOutput> AOs { get; set; }
            [ProtoMember(4)]
            public Dictionary<string, AnalogInput> AIs { get; set; }
        }
    }

    [ProtoContract]
    public class DigitalOutput
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Type { get; set; }
        [ProtoMember(3)]
        public Value Value { get; set; }
    }

    [ProtoContract]
    public class DigitalInput
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Type { get; set; }
        [ProtoMember(3)]
        public Value Value { get; set; }
    }

    [ProtoContract]
    public class AnalogOutput
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Type { get; set; }
        [ProtoMember(3)]
        public Value Value { get; set; }
    }

    [ProtoContract]
    public class AnalogInput
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Type { get; set; }
        [ProtoMember(3)]
        public Value Value { get; set; }
    }

}
