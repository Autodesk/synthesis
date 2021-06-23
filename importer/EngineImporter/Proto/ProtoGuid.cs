using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Synthesis.Proto {
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class ProtoGuid {
        public static implicit operator Guid(ProtoGuid g) => new Guid(g.B.ToByteArray());
        public static implicit operator ProtoGuid(Guid g) => new ProtoGuid() { B = ByteString.CopyFrom(g.ToByteArray()) };
    }
}
