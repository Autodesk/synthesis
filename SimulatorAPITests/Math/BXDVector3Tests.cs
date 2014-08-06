using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
public class BXDVector3Tests
{
    [TestMethod]
    public void BXDVector3_RW()
    {
        MemoryStream stream = new MemoryStream();
        BinaryReader reader = new BinaryReader(stream);
        BinaryWriter writer = new BinaryWriter(stream);
        BXDVector3 vec = new BXDVector3(1, 2, 3);
        vec.WriteData(writer);
        stream.Position = 0;
        BXDVector3 result = new BXDVector3();
        result.ReadData(reader);
        Assert.AreEqual(vec, result, "Vectors are not equal");
        stream.Close();
    }

    [TestMethod]
    public void BXDVector3_Add()
    {
        BXDVector3 vec1 = new BXDVector3(64, 319, 9);
        BXDVector3 vec2 = new BXDVector3(0.1, 0.91, 0.233);
        BXDVector3 expected = new BXDVector3(64.0 + 0.1, 319.0 + 0.91, 9.0 + 0.233);

        BXDVector3 result = vec1.Copy();
        BXDVector3 resultReturn = result.Add(vec2);
        Assert.IsTrue(result == resultReturn, "In place add gave a difference instance");
        Assert.AreEqual(result, expected, "Addition failed");
    }

    [TestMethod]
    public void BXDVector3_Multiply()
    {
        BXDVector3 vec1 = new BXDVector3(10, 20, 30);
        float scalar = 0.1f;
        BXDVector3 expected = new BXDVector3(10.0 * 0.1, 20.0 * 0.1, 30.0 * 0.1);

        BXDVector3 result = vec1.Copy();
        BXDVector3 resultReturn = result.Multiply(scalar);
        Assert.IsTrue(result == resultReturn, "In place scalar multiply gave a difference instance");
        Assert.AreEqual(result, expected, "Multiplication by scalar failed");
    }

    [TestMethod]
    public void BXDVector3_Copy()
    {
        BXDVector3 vec = new BXDVector3(1, 2, 3);
        BXDVector3 copy = vec.Copy();
        copy.x++;
        copy.y++;
        copy.z++;
        Assert.AreNotEqual(copy.x, vec.x, "Copy's x remains a reference");
        Assert.AreNotEqual(copy.y, vec.y, "Copy's x remains a reference");
        Assert.AreNotEqual(copy.z, vec.z, "Copy's x remains a reference");
    }

    [TestMethod]
    public void BXDVector3_Equals()
    {
        BXDVector3 vec = new BXDVector3(1, 2, 3);
        BXDVector3 other = new BXDVector3(1, 2, 3);
        Assert.AreEqual(vec, other, "Equivalent vectors aren't evaluated as equal");
        Assert.AreEqual(vec.GetHashCode(), other.GetHashCode(), "Equivalent vectors aren't hashed as equal");
    }
}
