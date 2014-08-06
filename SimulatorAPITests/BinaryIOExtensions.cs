using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
public class BinaryIOExtensionTesting
{

    private static void TestRWArrayHelper<T>(int len, TestUtils.CreateObjectFromValue<T> create)
    {
        MemoryStream stream = new MemoryStream();
        BinaryReader reader = new BinaryReader(stream);
        BinaryWriter writer = new BinaryWriter(stream);
        T[] arr = TestUtils.MakeRandomArray(len, create);
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
    public void BinaryIOExtensions_ReadWriteArray_Primitive()
    {
        TestRWArrayHelper<byte>(100, (double v) =>
        {
            return (byte) v;
        });
        TestRWArrayHelper<short>(100, (double v) =>
        {
            return (short) v;
        });
        TestRWArrayHelper<int>(100, (double v) =>
        {
            return (int) v;
        });
        TestRWArrayHelper<long>(100, (double v) =>
        {
            return (long) v;
        });
        TestRWArrayHelper<bool>(100, (double v) =>
        {
            return (((int) v) & 1) == 0;
        });
        TestRWArrayHelper<char>(100, (double v) =>
        {
            return (char) v;
        });
    }

    private class TestRWObject : RWObject
    {
        public double val = new Random().NextDouble();
        public void WriteData(BinaryWriter wr)
        {
            wr.Write(val);
        }
        public void ReadData(BinaryReader rd)
        {
            val = rd.ReadDouble();
        }
        public override bool Equals(object obj)
        {
            return obj is TestRWObject && ((TestRWObject) obj).val == val;
        }
    }

    private class TestRWObject_Constructor : TestRWObject
    {
        public TestRWObject_Constructor()
        {
            val *= 2.0;
        }
    }

    [TestMethod]
    public void BinaryIOExtensions_ReadWriteObject_Delegate()
    {
        MemoryStream stream = new MemoryStream();
        BinaryReader reader = new BinaryReader(stream);
        BinaryWriter writer = new BinaryWriter(stream);
        TestRWObject obj = new TestRWObject();
        writer.Write(obj);
        stream.Position = 0;
        TestRWObject res = reader.ReadRWObject<TestRWObject>((BinaryReader rdr) =>
        {
            TestRWObject got = new TestRWObject();
            got.ReadData(rdr);
            return got;
        });
        Assert.AreEqual(obj, res);
        stream.Close();
    }

    [TestMethod]
    public void BinaryIOExtensions_ReadWriteObject_DefaultConstructor()
    {
        MemoryStream stream = new MemoryStream();
        BinaryReader reader = new BinaryReader(stream);
        BinaryWriter writer = new BinaryWriter(stream);
        TestRWObject obj = new TestRWObject();
        writer.Write(obj);
        stream.Position = 0;
        TestRWObject res = reader.ReadRWObject<TestRWObject>();
        Assert.AreEqual(obj, res);
        stream.Close();
    }

    [TestMethod]
    public void BinaryIOExtensions_ReadWriteObject_NoArgumentConstructor()
    {
        MemoryStream stream = new MemoryStream();
        BinaryReader reader = new BinaryReader(stream);
        BinaryWriter writer = new BinaryWriter(stream);
        TestRWObject_Constructor obj = new TestRWObject_Constructor();
        writer.Write(obj);
        stream.Position = 0;
        TestRWObject_Constructor res = reader.ReadRWObject<TestRWObject_Constructor>();
        Assert.AreEqual(obj, res);
        stream.Close();
    }

    [TestMethod]
    public void BinaryIOExtensions_ReadWriteObject_IntoExisting()
    {
        MemoryStream stream = new MemoryStream();
        BinaryReader reader = new BinaryReader(stream);
        BinaryWriter writer = new BinaryWriter(stream);
        TestRWObject obj = new TestRWObject();
        writer.Write(obj);
        stream.Position = 0;
        TestRWObject res = new TestRWObject();
        reader.ReadRWInto(res);
        Assert.AreEqual(obj, res);
        stream.Close();
    }

    [TestMethod]
    public void BinaryIOExtensions_ReadWriteArray_Object()
    {
        TestRWArrayHelper<TestRWObject>(100, (double val) =>
        {
            TestRWObject obj = new TestRWObject();
            obj.val = val;
            return obj;
        });
    }
}
