using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InventorServices.Persistence
{
    //TODO: Make non-generic interfaces for this if it improves the 
    //readability situation in IObjectBinder implementations.
    public interface ISerializableId<T>
    {
        T Id { get; set; }
    }
}
