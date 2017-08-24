using System.Collections.Generic;
using System.Xml;

/// <summary>
/// Used for simplifying reading and writing operations.
/// </summary>
public static class IOUtilities
{
    /// <summary>
    /// Used for easy access of all elements read by an XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static IEnumerable<string> AllElements(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.IsStartElement())
                yield return reader.Name;
        }
    }
}
