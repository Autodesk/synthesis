using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InventorServices.Persistence
{
    public interface ISerializableModuleIdManager
    {      
        //Maybe make a new interface that is more client code specific if it reduces the number of 
        //these huge type declarations.
        bool GetTraceData(string key, out ISerializableId<List<Tuple<string, int, int, byte[]>>> id);
        void SetTraceData(string key, ISerializable value);
    }
}
