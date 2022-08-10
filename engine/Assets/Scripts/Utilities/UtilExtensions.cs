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

    public static Vector3 ToVector3(this float[] arr)
        => new Vector3(arr[0], arr[1], arr[2]);
    public static Quaternion ToQuaternion(this float[] arr)
        => new Quaternion(arr[0], arr[1], arr[2], arr[3]);

    public static float[] ToArray(this Vector3 vec)
        => new float[] { vec.x, vec.y, vec.z };
    public static float[] ToArray(this Quaternion quat)
        => new float[] { quat.x, quat.y, quat.z, quat.w };

    public static Bounds GetBounds(this Transform top) {
        Vector3 min = new Vector3(float.MaxValue,float.MaxValue,float.MaxValue), max = new Vector3(float.MinValue,float.MinValue,float.MinValue);
        top.GetComponentsInChildren<Renderer>().ForEach(x => {
            var b = x.bounds;
            if (min.x > b.min.x) min.x = b.min.x;
            if (min.y > b.min.y) min.y = b.min.y;
            if (min.z > b.min.z) min.z = b.min.z;
            if (max.x < b.max.x) max.x = b.max.x;
            if (max.y < b.max.y) max.y = b.max.y;
            if (max.z < b.max.z) max.z = b.max.z;
        });
        return new UnityEngine.Bounds(((max + min) / 2f) - top.position, max - min);
    }
    
    // public static void ApplyMatrix(this Transform trans, Matrix4x4 m) {
    //     // m.Print();
    //     trans.localPosition = m.GetPosition();
    //     var rot = m.rotation;
    //     rot = new Quaternion(-rot.x, rot.y, rot.z, -rot.w);
    //     trans.localRotation = rot;
    //     trans.localScale = m.lossyScale;
    // }

    public static void Print(this Matrix4x4 m) {
        Debug.Log($"{m[0, 0]}, {m[0, 1]}, {m[0, 2]}, {m[0, 3]}\n{m[1, 0]}, {m[1, 1]}, {m[1, 2]}, {m[1, 3]}"
                  + $"\n{m[2, 0]}, {m[2, 1]}, {m[2, 2]}, {m[2, 3]}\n{m[3, 0]}, {m[3, 1]}, {m[3, 2]}, {m[3, 3]}");
    }

    public static List<Mirabuf.Node> AllTreeElements(this IEnumerable<Mirabuf.Node> ns) {
        var elm = new List<Mirabuf.Node>();
        ns.ForEach(x => elm.AddRange(x.AllTreeElements()));
        return elm;
    }
    /// <summary>
    /// DO NOT USE List<Mirabuf.Node> AllTreeElements(this IEnumerable<Mirabuf.Node> ns)
    /// INSIDE THIS FUNCTION. WILL CREATE RACE CASE
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static List<Mirabuf.Node> AllTreeElements(this Mirabuf.Node n) {
        var elm = new List<Mirabuf.Node>();
        elm.Add(n);
        n.Children.ForEach(x => elm.AddRange(x.AllTreeElements()));
        return elm;
    }

    // public static IEnumerable<Node> UnravelNodes(this IEnumerable<Edge> edges) {
    //     var nodes = new Node[edges.Count()];
    //     for (int i = 0; i < nodes.Length; i++) {
    //         nodes[i] = edges.ElementAt(i).Node;
    //     }
    //     return nodes;
    // }

    public static Vector3 DotProduct(this Vector3 a, Vector3 b)
        => new Vector3(a.y * b.z - a.z * b.y, -(a.x * b.z - a.z * b.x), a.x * b.y - a.y * b.x);
    
    // TODO: This should be done when the matrix is created
    public static UVector3 GetPosition(this Matrix4x4 m)
        => new UVector3(m.m03, m.m13, m.m23);
}
