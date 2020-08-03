using System;
using System.Collections;
using System.Collections.Generic;

namespace SynthesisAPI.Utilities
{
    public class UniqueTypeList<T> : IEnumerable<T>
    {
        private Dictionary<Type, T> entries;
        public UniqueTypeList()
        {
            entries = new Dictionary<Type, T>();
        }
        public U Get<U>() where U : T
        {
            if (entries.ContainsKey(typeof(U)))
                return (U) entries[typeof(U)];
            return default;
        }
        public bool Add<U>(U val) where U : T
        {
            if (entries.ContainsKey(typeof(U)))
                return false;
            entries[typeof(U)] = val;
            return true;
        }
        public void ForceAdd<U>(U val) where U : T
        {
            entries[typeof(U)] = val;
        }
        public bool Remove<U>()
        {
            return Remove(typeof(U));
        }
        public bool Remove(Type type)
        {
            return entries.Remove(type);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return entries.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<T> Values { get => entries.Values; }
    }
}
