using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UVector3 = UnityEngine.Vector3;

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
    
    public static void ApplyMatrix(this Transform trans, Matrix4x4 m) {
        // m.Print();
        trans.localPosition = m.GetPosition();
        var rot = m.rotation;
        rot = new Quaternion(-rot.x, rot.y, rot.z, -rot.w);
        trans.localRotation = rot;
        trans.localScale = m.lossyScale;
    }

    public static void Print(this Matrix4x4 m) {
        Debug.Log($"{m[0, 0]}, {m[0, 1]}, {m[0, 2]}, {m[0, 3]}\n{m[1, 0]}, {m[1, 1]}, {m[1, 2]}, {m[1, 3]}"
                  + $"\n{m[2, 0]}, {m[2, 1]}, {m[2, 2]}, {m[2, 3]}\n{m[3, 0]}, {m[3, 1]}, {m[3, 2]}, {m[3, 3]}");
    }

    // public static IEnumerable<Node> UnravelNodes(this IEnumerable<Edge> edges) {
    //     var nodes = new Node[edges.Count()];
    //     for (int i = 0; i < nodes.Length; i++) {
    //         nodes[i] = edges.ElementAt(i).Node;
    //     }
    //     return nodes;
    // }
    
    public static UVector3 GetPosition(this Matrix4x4 m)
        => new UVector3(m.m03 * -0.01f, m.m13 * 0.01f, m.m23 * 0.01f);
}
