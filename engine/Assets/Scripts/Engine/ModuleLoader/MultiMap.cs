using System.Collections;
using System.Collections.Generic;

namespace Synthesis.Core.ModuleLoader
{
	public class MultiMap<TKey, TValue> : IDictionary<TKey, List<TValue>>
	{
		private Dictionary<TKey, List<TValue>> elements;

		public MultiMap()
		{
			elements = new Dictionary<TKey, List<TValue>>();
		}

		public void Add(TKey key, TValue value)
		{
			if (elements.ContainsKey(key))
			{
				elements[key].Add(value);
			}
			else
			{
				elements[key] = new List<TValue> {value};
			}
		}

		public void Add(TKey key, List<TValue> value)
		{
			elements.Add(key, value);
		}

		public bool ContainsKey(TKey key)
		{
			return elements.ContainsKey(key);
		}

		public bool Remove(TKey key)
		{
			return elements.Remove(key);
		}

		public bool TryGetValue(TKey key, out List<TValue> value)
		{
			return elements.TryGetValue(key, out value);
		}

		public List<TValue> this[TKey k]
		{
			get => elements[k];
			set => elements[k] = value;
		}

		public ICollection<TKey> Keys => elements.Keys;
		public ICollection<List<TValue>> Values => elements.Values;

		IEnumerator<KeyValuePair<TKey, List<TValue>>> IEnumerable<KeyValuePair<TKey, List<TValue>>>.GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<List<TValue>> GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		public void Add(KeyValuePair<TKey, List<TValue>> item)
		{
			throw new System.NotImplementedException();
		}

		public void Clear()
		{
			elements.Clear();
		}

		public bool Contains(KeyValuePair<TKey, List<TValue>> item)
		{
			throw new System.NotImplementedException();
		}

		public void CopyTo(KeyValuePair<TKey, List<TValue>>[] array, int arrayIndex)
		{
			throw new System.NotImplementedException();
		}

		public bool Remove(KeyValuePair<TKey, List<TValue>> item)
		{
			throw new System.NotImplementedException();
		}

		public int Count => elements.Count;
		public bool IsReadOnly { get; }
	}
}