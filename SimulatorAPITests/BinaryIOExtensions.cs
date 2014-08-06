using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
public class BinaryIOExtensions
{
    private delegate T CreateObjectFromValue<T>(double val);

    private static void TestRWArray<T>(int len, CreateObjectFromValue<T> create)
    {
        MemoryStream stream = new MemoryStream();
        BinaryReader reader = new BinaryReader(stream);
        BinaryWriter writer = new BinaryWriter(stream);
        T[] arr = new T[len];
        Random rand = new Random();
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = create(rand.NextDouble() * 1000.0);
        }
        {
            stream.Position = 0;
            writer.WriteArray(arr);
            stream.Position = 0;
            T[] res = reader.ReadArray<T>();
            AssertExtensions.AreEqual(arr, res, 0, typeof(T).Name + "[" + len + "], 0 off, infer length");
        }
        {
            stream.Position = 0;
            writer.WriteArray(arr, 1);
            stream.Position = 0;
            T[] res = reader.ReadArray<T>();
            AssertExtensions.AreEqual(arr, res, 1, typeof(T).Name + "[" + len + "], 1 off, infer length");
        }
        stream.Close();
    }

    [TestMethod]
    public void BinaryIOExtensions_ReadWriteArray()
    {
        TestRWArray<byte>(100, (double v) =>
        {
            return (byte) v;
        });
        TestRWArray<short>(100, (double v) =>
        {
            return (short) v;
        });
        TestRWArray<int>(100, (double v) =>
        {
            return (int) v;
        });
        TestRWArray<long>(100, (double v) =>
        {
            return (long) v;
        });
        TestRWArray<bool>(100, (double v) =>
        {
            return (((int) v) & 1) == 0;
        });
        TestRWArray<char>(100, (double v) =>
        {
            return (char) v;
        });
    }
}
