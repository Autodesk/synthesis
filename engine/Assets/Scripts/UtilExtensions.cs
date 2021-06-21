using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UtilExtensions {
    public static void ForEachIndex<T>(this IEnumerable<T> arr, Action<int, T> act) {
        for (int i = 0; i < arr.Count(); i++) {
            act(i, arr.ElementAt(i));
        }
    }

    public static void ForEach<T, U>(this IDictionary<T, U> dict, Action<T, U> act) {
        foreach (var kvp in dict) {
            act(kvp.Key, kvp.Value);
        }
    }

    public static IEnumerable<U> Select<T, U>(this IEnumerable<T> e, Func<T, U> c) {
        List<U> l = new List<U>();
        foreach (var a in e) {
            l.Add(c(a));
        }
        return l;
    }

    public static T Find<T>(this IEnumerable<T> e, Func<T, bool> c) {
        foreach (var a in e) {
            if (c(a))
                return a;
        }
        return default;
    }
}
