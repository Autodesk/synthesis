using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public interface RWObject
{
    /// <summary>
    /// Serializes this object to the given stream
    /// </summary>
    /// <param name="writer">Output stream</param>
    void WriteData(BinaryWriter writer);

    /// <summary>
    /// Deserializes this object from the given stream.
    /// </summary>
    /// <param name="reader">Input stream</param>
    void ReadData(BinaryReader reader);
}

public static class RWObjectExtensions
{
    /// <summary>
    /// Writes this object to the given output path.
    /// </summary>
    /// <param name="path">Output path</param>
    public static void WriteToFile(this RWObject obj, String path)
    {
        BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));
        writer.Write(obj);
        writer.Close();
    }

    /// <summary>
    /// Reads this object from the given input path.
    /// </summary>
    /// <param name="path">Input path</param>
    public static void ReadFromFile(this RWObject obj, String path)
    {
        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open));
        obj.ReadData(reader);
        reader.Close();
    }

    /// <summary>
    /// Serializes the object into this stream
    /// </summary>
    /// <param name="writer">Output stream</param>
    public static void Write(this BinaryWriter writer, RWObject obj)
    {
        obj.WriteData(writer);
    }
}