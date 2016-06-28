using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InventorServices.Persistence
{
    public interface ISerializableIdManager
    {
        bool GetTraceData(string key, out ISerializableId<byte[]> id);
        void SetTraceData(string key, ISerializable value);
    }
}
