using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Synthesis.Proto;

/// <summary>
/// Utility class defined in the main scope
/// </summary>
internal static class ImporterUtil {
    public static XmlNode Find(this XmlNodeList nodes, Predicate<XmlNode> condition) {
        var enumerator = nodes.GetEnumerator();
        enumerator.Reset();
        while (enumerator.MoveNext()) {
            if (condition((XmlNode)enumerator.Current))
                return (XmlNode)enumerator.Current;
        }

        return null;
    }

    public static List<XmlNode> FindAll(this XmlNodeList nodes, Predicate<XmlNode> condition) {
        var matchingNodes = new List<XmlNode>();
        var enumerator = nodes.GetEnumerator();
        enumerator.Reset();
        while (enumerator.MoveNext()) {
            if (condition((XmlNode)enumerator.Current))
                matchingNodes.Add((XmlNode)enumerator.Current);
        }

        return matchingNodes;
    }

    public static (Vec3 min, Vec3 max) Bounds(this IEnumerable<Vec3> verts) {
        Vec3 min = new Vec3 { X = float.MaxValue, Y = float.MaxValue, Z = float.MaxValue },
            max = new Vec3 { X = float.MinValue, Y = float.MinValue, Z = float.MinValue };
        foreach (var v in verts) {
            if (v.X < min.X)
                min.X = v.X;
            if (v.Y < min.Y)
                min.Y = v.Y;
            if (v.Z < min.Z)
                min.Z = v.Z;

            if (v.X > max.X)
                max.X = v.X;
            if (v.Y > max.Y)
                max.Y = v.Y;
            if (v.Z > max.Z)
                max.Z = v.Z;
        }
        return (min, max);
    }

    public static List<T> Map<T>(this IEnumerable<T> collection, Func<T, T> modification) {
        List<T> modCollection = new List<T>();
        foreach (var x in collection) {
            modCollection.Add(modification(x));
        }
        return modCollection;
    }

    public static List<XmlNode> ToList(this XmlNodeList nodes) {
        var nodeList = new List<XmlNode>();
        foreach (XmlNode n in nodes) {
            nodeList.Add(n);
        }
        return nodeList;
    }
}
