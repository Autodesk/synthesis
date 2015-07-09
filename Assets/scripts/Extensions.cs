using System;
using System.Collections.Generic;

public static class Extensions
{
	public static void AddAll<TKey, TValue>(this Dictionary<TKey, TValue> d, Dictionary<TKey, TValue> value)
	{
		foreach (KeyValuePair<TKey, TValue> k in value)
		{
			d.Add(k.Key, k.Value);
		}
	}
}

