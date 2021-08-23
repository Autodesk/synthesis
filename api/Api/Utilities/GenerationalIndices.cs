using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;

namespace SynthesisAPI.Utilities
{
    /*
    struct GenerationalIndex
    {
        public int Index { get; set; }
        public int Generation { get; set; }
    }

    class GenerationalIndexAllocator
    {
        class AllocatorEntry
        {
            public bool IsLive { get; set; }
            public int Generation { get; set; }
        }

        List<AllocatorEntry> Entries { get; set; }
        List<int> FreeIndices { get; set; }

        
        public GenerationalIndex Allocate()
        {
            if (FreeIndices.Count > 0)
            {
                int index = FreeIndices[FreeIndices.Count - 1];
                FreeIndices.RemoveAt(FreeIndices.Count - 1);

                Entries[index].Generation += 1;
                Entries[index].IsLive = true;

                return new GenerationalIndex() { Index = index, Generation = Entries[index].Generation };
            }
            else
            {
                Entries.Add(new AllocatorEntry()
                {
                    IsLive = true,
                    Generation = 0
                });
                return new GenerationalIndex()
                {
                    Index = Entries.Count - 1,
                    Generation = 0
                };
            }
        }

        public void Deallocate(GenerationalIndex index)
        {
            if (IsLive(index))
            {
                Entries[index.Index].IsLive = false;
                FreeIndices.Add(index.Index);
            }
        }

        public bool IsLive(GenerationalIndex index)
        {
            return index.Index < Entries.Count && Entries[index.Index].Generation == index.Generation && Entries[index.Index].IsLive;
        }
        
        public struct GenerationalEntry<T>
        {
            public int Generation { get; set; }
            public T Value { get; set; }
        }

        
        public class GenerationalIndexArray<Entry> : IList<Entry>
        {
            private Entry[] _contents = new Entry[8];
            private int _count;

            public GenerationalIndexArray()
            {
                _count = 0;
            }

            // IList Members
            public void Add(Entry value)
            {
                if (_count < _contents.Length)
                {
                    _contents[_count] = value;
                    _count++;
                }
            }

            public void Clear()
            {
                _count = 0;
            }

            public bool Contains(Entry value)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (_contents[i].Equals(value))
                    {
                        return true;
                    }
                }
                return false;
            }

            public int IndexOf(Entry value)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (_contents[i].Equals(value))
                    {
                        return i;
                    }
                }
                return -1;
            }

            public void Insert(int index, Entry value)
            {
                if ((_count + 1 <= _contents.Length) && (index < Count) && (index >= 0))
                {
                    _count++;

                    for (int i = Count - 1; i > index; i--)
                    {
                        _contents[i] = _contents[i - 1];
                    }
                    _contents[index] = value;
                }
            }

            public bool IsFixedSize
            {
                get
                {
                    return true;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public bool Remove(Entry value)
            {
                try
                {
                    RemoveAt(IndexOf(value));
                }
                catch (IndexOutOfRangeException)
                {
                    return false;
                }
                return true;
            }

            public void RemoveAt(int index)
            {
                if ((index >= 0) && (index < Count))
                {
                    for (int i = index; i < Count - 1; i++)
                    {
                        _contents[i] = _contents[i + 1];
                    }
                    _count--;
                }
            }

            public Entry this[int index]
            {
                get
                {
                    return _contents[index];
                }
                set
                {
                    _contents[index] = value;
                }
            }

            // ICollection members.

            public void CopyTo(Entry[] array, int index)
            {
                for (int i = 0; i < Count; i++)
                {
                    array.SetValue(_contents[i], index++);
                }
            }

            public int Count
            {
                get
                {
                    return _count;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            // Return the current instance since the underlying store is not
            // publicly available.
            public object SyncRoot
            {
                get
                {
                    return this;
                }
            }

            // IEnumerable Members

            public IEnumerator<Entry> GetEnumerator()
            {
                return _contents.Cast<Entry>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _contents.GetEnumerator();
            }

            public void PrintContents()
            {
                Console.WriteLine($"List has a capacity of {_contents.Length} and currently has {_count} elements.");
                Console.Write("List contents:");
                for (int i = 0; i < Count; i++)
                {
                    Console.Write($" {_contents[i]}");
                }
                Console.WriteLine();
            }

            
        }
        
    }
    */
}
