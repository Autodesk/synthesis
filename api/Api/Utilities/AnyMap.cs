using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace SynthesisAPI.Utilities
{
    /// <summary>
    /// Dynamic mapping of any reference type to its corresponding GenIndexArray
    /// </summary>
    public class AnyMap<TValue> where TValue : class
    {
        Dictionary<Type, GenIndexArray> componentDict;
        public AnyMap()
        {
            componentDict = new Dictionary<Type, GenIndexArray>();
        }
        public void Set(ushort index, ushort gen, TValue val)
        {
            if (componentDict.TryGetValue(val.GetType(), out GenIndexArray arr))
                arr.Set(index, gen, val);
            else
            {
                componentDict.Add(val.GetType(), new GenIndexArray());
                Set(index, gen, val);
            }
        }

        public void Remove(ushort index, ushort gen, Type componentType)
        {
            if (componentDict.TryGetValue(componentType, out GenIndexArray arr))
                arr.Set(index, gen, null);
        }

        public TValue? Get(ushort index, ushort gen, Type componentType)
        {
            if (componentDict.TryGetValue(componentType, out GenIndexArray arr))
            {
                return arr.Get(index, gen);
            }
            else
                return null;
        }

        public List<TValue>? GetAll(ushort index, ushort gen)
        {
            var output = new List<TValue>();
            foreach (var type in componentDict.Keys)
            {
                var entry = Get(index, gen, type);
                if (entry != null)
                    output.Add(entry);
            }

            return output;
        }

        public void Clear()
        {
            componentDict.Clear();
        }

        #region GenIndexArray

        /// <summary>
        /// Maps Entity to its component by its index : component index in entries is the same as Entity index
        /// </summary>
        class GenIndexArray
        {
            List<Entry> entries;

            public GenIndexArray()
            {
                entries = new List<Entry>();
            }

            struct Entry
            {
                public Entry(TValue? val, ulong gen)
                {
                    Val = val;
                    Gen = gen;
                }
                public TValue? Val { get; }
                public ulong Gen { get; }
            }

            public void Set(ushort index, ushort gen, TValue? val)
            {
                ushort entityIndex = index;
                ushort entityGen = gen;
                if (entityIndex < entries.Count)
                    entries[entityIndex] = new Entry(val, entityGen);
                else
                {
                    //increase list size by populating "null" Entry so we can add value at index which is outside of bounds
                    for (int i = entries.Count; i < entityIndex; i++)
                        entries.Add(new Entry(null, 0)); //no Entity has gen of 0 so these entries can never be accessed
                    entries.Add(new Entry(val, entityGen));
                }
            }

            public TValue? Get(ushort index, ushort gen)
            {
                ushort entityIndex = index;
                ushort entityGen = gen;
                if (entityIndex >= entries.Count)
                    return null; //prevents IndexOutOfBoundsException
                Entry entry = entries[entityIndex];
                //only get component if generations match - avoids having reallocated Entities point to the components of deallocated Entities
                if (entry.Gen == entityGen)
                    return entry.Val;
                return null;
            }
        }

        #endregion
    }
}
