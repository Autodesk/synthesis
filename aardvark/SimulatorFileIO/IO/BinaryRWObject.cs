using System;
using System.IO;

/// <summary>
/// Interface for objects that can read/write binary data to files
/// </summary>
public interface BinaryRWObject
{
    /// <summary>
    /// Serializes this object to the given stream
    /// </summary>
    /// <param name="writer">Output stream</param>
    void WriteBinaryData(BinaryWriter writer);

    /// <summary>
    /// Deserializes this object from the given stream.
    /// </summary>
    /// <param name="reader">Input stream</param>
    void ReadBinaryData(BinaryReader reader);
}

/// <summary>
/// Extension functions for objects implementing the BinaryRWObject interface
/// </summary>
public static class BinaryRWObjectExtensions
{

    /// <summary>
    /// Delegate function to read a RWObject from a binary stream
    /// </summary>
    /// <param name="reader">Binary stream to read object from</param>
    /// <returns>The created object</returns>
    public delegate BinaryRWObject ReadObjectFully(BinaryReader reader);

    /// <summary>
    /// Writes this object to the given output path.
    /// </summary>
    /// <param name="path">Output path</param>
    public static void WriteToFile(this BinaryRWObject obj, String path)
    {
        if (File.Exists(path)) File.Delete(path);
        using(BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate)))
        {
            writer.Write(obj);
        }
    }

    /// <summary>
    /// Reads this object from the given input path.
    /// </summary>
    /// <param name="path">Input path</param>
    public static void ReadFromFile(this BinaryRWObject obj, String path, BXDIO.ProgressReporter progress = null)
    {
        using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
        {
            System.Threading.Thread progressThread = null;
            if (progress != null)
            {
                // Not the most informative, but it is something.
                progressThread = new System.Threading.Thread(() =>
                {
                    while (true)
                    {
                        progress(reader.BaseStream.Position, reader.BaseStream.Length);
                        System.Threading.Thread.Sleep(10);
                    }
                });
                progressThread.Start();
            }
            obj.ReadBinaryData(reader);
            if (progressThread != null)
            {
                progressThread.Abort();
            }
        }
    }

    /// <summary>
    /// Serializes the object into this stream
    /// </summary>
    /// <param name="writer">Output stream</param>
    public static void Write(this BinaryWriter writer, BinaryRWObject obj)
    {
        obj.WriteBinaryData(writer);
    }

    /// <summary>
    /// Deserializes the object from this stream
    /// </summary>
    /// <param name="reader">Input stream</param>
    /// <param name="readInternal">Optional delegate to create the object</param>
    public static T ReadRWObject<T>(this BinaryReader reader, BinaryRWObjectExtensions.ReadObjectFully readInternal = null)
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
                    BinaryRWObject ro = (BinaryRWObject) ctr.Invoke(new object[0]);
                    ro.ReadBinaryData(rdr);
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
    public static T ReadRWInto<T>(this BinaryReader reader, T t) where T : BinaryRWObject
    {
        t.ReadBinaryData(reader);
        return t;
    }
}