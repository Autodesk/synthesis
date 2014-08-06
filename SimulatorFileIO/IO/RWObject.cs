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

    public delegate RWObject DoReadRWObject(BinaryReader reader);

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

    /// <summary>
    /// Deserializes the object from this stream
    /// </summary>
    /// <param name="reader">Input stream</param>
    /// <param name="readInternal">Optional delegate to create the object</param>
    public static T ReadRWObject<T>(this BinaryReader reader, RWObjectExtensions.DoReadRWObject readInternal = null)
    {
        if (readInternal == null)
        {
            // Try to find a constructor...
            System.Reflection.ConstructorInfo ctr = typeof(T).GetConstructor(new Type[0]);
            if (ctr == null)
            {
                throw new IOException("Can't read " + typeof(T).Name + " directly!\n");
            }
            else
            {
                readInternal = (BinaryReader rdr) =>
                {
                    RWObject ro = (RWObject) ctr.Invoke(new object[0]);
                    ro.ReadData(rdr);
                    return ro;
                };
            }
        }
        return (T) readInternal(reader);
    }

    /// <summary>
    /// Deserializes the object from this stream
    /// </summary>
    /// <param name="reader">Input stream</param>
    /// <param name="t">Read into</param>
    public static T ReadRWInto<T>(this BinaryReader reader, T t) where T : RWObject
    {
        t.ReadData(reader);
        return t;
    }
}