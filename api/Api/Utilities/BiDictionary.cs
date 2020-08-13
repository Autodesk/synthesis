using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SynthesisAPI.Utilities
{
    public class BiDictionary<A, B> : IEnumerable<KeyValuePair<A, B>>
    {
        private List<KeyValuePair<A, B>> collection = new List<KeyValuePair<A, B>>();

        public B this[A query] {
            get {
                var i = collection.FindIndex(x => x.Key.Equals(query));
                if (i == -1)
                    throw new ArgumentException($"BiDictionary does not contain matching item for {query}");
                return collection[i].Value;
            }
            set {
                collection.RemoveAll(x => x.Key.Equals(query));
                collection.Add(new KeyValuePair<A, B>(query, value));
            }
        }
        public A this[B query] {
            get {
                var i = collection.FindIndex(x => x.Value.Equals(query));
                if (i == -1)
                    throw new ArgumentException($"BiDictionary does not contain matching item for {query}");
                return collection[i].Key;
            }
            set {
                collection.RemoveAll(x => x.Value.Equals(query));
                collection.Add(new KeyValuePair<A, B>(value, query));
            }
        }

        public bool Contains(A query)
        {
            return collection.FindIndex(x => x.Key.Equals(query)) != -1;
        }

        public bool Contains(B query)
        {
            return collection.FindIndex(x => x.Value.Equals(query)) != -1;
        }

        public void Add(A key, B value)
        {
            if (!Contains(key) && !Contains(value))
            {
                collection.Add(new KeyValuePair<A, B>(key, value));
            }
            else
            {
                throw new ArgumentException($"BiDictionary: Element already exists with key {key} or value {value}");
            }
        }

        public bool TryGetValue(A key, out B value)
        {
            if (Contains(key))
            {
                value = this[key];
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetValue(B value, out A key)
        {
            if (Contains(value))
            {
                key = this[value];
                return true;
            }
            key = default;
            return false;
        }

        public IEnumerator<KeyValuePair<A, B>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<A, B>>)collection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<A, B>>)collection).GetEnumerator();
        }
    }
}