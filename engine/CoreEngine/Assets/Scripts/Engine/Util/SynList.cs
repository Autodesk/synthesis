using System;
using System.Collections;
using System.Collections.Generic;

namespace Synthesis.Util
{
    public class SynList<A, B> : IEnumerable<KeyValuePair<A, B>>
    {
        private List<KeyValuePair<A, B>> colleciton = new List<KeyValuePair<A, B>>();

        public B this[A query] {
            get { return colleciton.Find(x => x.Key.Equals(query)).Value; }
            set {
                colleciton.RemoveAll(x => x.Key.Equals(query));
                colleciton.Add(new KeyValuePair<A, B>(query, value));
            }
        }
        public A this[B query] {
            get { return colleciton.Find(x => x.Value.Equals(query)).Key; }
            set {
                colleciton.RemoveAll(x => x.Value.Equals(query));
                colleciton.Add(new KeyValuePair<A, B>(value, query));
            }
        }

        public IEnumerator<KeyValuePair<A, B>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<A, B>>)colleciton).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<A, B>>)colleciton).GetEnumerator();
        }
    }
}