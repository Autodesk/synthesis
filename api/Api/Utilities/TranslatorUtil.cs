using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;

#nullable enable

/// <summary>
/// Utility class defined in the main scope
/// </summary>

[assembly: InternalsVisibleTo(assemblyName: "TranslatorTest")]

internal static class TranslatorUtil {
    public static XmlNode? Find(this XmlNodeList nodes, Predicate<XmlNode> condition) {
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

    public static List<T> Map<T>(this IEnumerable<T> collection, Func<T, T> modification) {
        List<T> modCollection = new List<T>();
        foreach (var x in collection) {
            modCollection.Add(modification(x));
        }
        return modCollection;
    }
    
    public static List<U> Map<T, U>(this IEnumerable<T> collection, Func<T, U> modification) {
        List<U> modCollection = new List<U>();
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

    public static string ToHexString(this byte[] buf) {
        List<string> strs = buf.Map<byte, string>(x => x.ToHexString());
        string res = string.Empty;
        foreach (var s in strs) {
            res += s;
        }
        return res;
    }
    public static string ToHexString(this byte b)
        => ((byte)((b & 0xF0) >> 4)).ToHexCharacter().ToString()
           + ((byte)(b & 0x0F)).ToHexCharacter();
    public static char ToHexCharacter(this byte b) {
        switch (b) {
            case 10:
                return 'a';
            case 11:
                return 'b';
            case 12:
                return 'c';
            case 13:
                return 'd';
            case 14:
                return 'e';
            case 15:
                return 'f';
            default:
                return b.ToString()[0];
        }
    }
}
