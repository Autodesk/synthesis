using System;
using System.Collections.Generic;

namespace InventorServices.Utilities
{
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public TupleList()
        {
        }

        public TupleList(IEnumerable<Tuple<T1, T2>> IEnumTuple)
        {
            foreach (Tuple<T1, T2> pair in IEnumTuple)
            {
                Add(pair.Item1, pair.Item2);
            }
        }

        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }
}
