using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InventorServices.Persistence
{
    public interface IModuleBinder
    {
        ISerializableId<T> GetObjectKey<T>();
        bool GetObjectFromTrace<T>(out T e);

        void SetObjectForTrace<T>(dynamic objectToBind, Func<List<Tuple<string, int, int, byte[]>>,
                                                             Tuple<string, int, int, byte[]>,
                                                             List<Tuple<string, int, int, byte[]>>> referenceKeysEvaluator);

        IContextData ContextData { get; }
        IContextManager ContextManager { get; }
    }
}
