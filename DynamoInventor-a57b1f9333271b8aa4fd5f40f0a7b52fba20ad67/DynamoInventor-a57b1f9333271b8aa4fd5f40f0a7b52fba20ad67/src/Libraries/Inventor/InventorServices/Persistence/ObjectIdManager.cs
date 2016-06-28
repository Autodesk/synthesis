using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using DSNodeServices;

namespace InventorServices.Persistence
{
    public class ObjectIdManager : ISerializableIdManager
    {
        public bool GetTraceData(string key, out ISerializableId<byte[]> id)
        {
            id = TraceUtils.GetTraceData(key) as ISerializableId<byte[]>;
            if (id != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetTraceData(string key, System.Runtime.Serialization.ISerializable value)
        {
            TraceUtils.SetTraceData(key, value);
        }
    }
}
