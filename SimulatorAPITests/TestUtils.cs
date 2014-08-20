using System;
using System.Collections.Generic;
using System.IO;

class TestUtils
{
    public delegate T CreateObjectFromValue<T>(double val);
    public static T[] MakeRandomArray<T>(int len, CreateObjectFromValue<T> create)
    {
        T[] arr = new T[len];
        Random rand = new Random();
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = create(rand.NextDouble() * 1000.0);
        }
        return arr;
    }

    public static T WriteReadObject<T>(T obj, RWObjectExtensions.ReadObjectFully ext = null) where T : RWObject
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        BinaryReader reader = new BinaryReader(stream);
        obj.WriteData(writer);
        stream.Position = 0;
        T result = reader.ReadRWObject<T>(ext);
        stream.Close();
        return result;
    }
}
