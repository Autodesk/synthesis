using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InventorServices.Persistence
{
    public interface IObjectBinder
    {
        ISerializableId<T> GetObjectKey<T>();
        bool GetObjectFromTrace<T>(out T e);
        void SetObjectForTrace<T>(dynamic objectToBind);
        IContextData ContextData { get; }
        IContextManager ContextManager { get; }
        IDocumentManager DocumentManager { get; }
    }
}
