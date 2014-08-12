using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ArrayUtilititesTests
{
    [TestMethod]
    public void ArrayUtilities_WrapArray_Vec2()
    {
        double[] input = { 1, 2, 3, 4 };
        BXDVector3[] output = ArrayUtilities.WrapArray(delegate(double a, double b)
        {
            return new BXDVector3(a, b, 0);
        }, input);
        Assert.AreEqual(2, output.Length, "WrapArray2 length mismatch");
        Assert.AreEqual(new BXDVector3(1, 2, 0), output[0], "WrapArray2 output[0] mismatch");
        Assert.AreEqual(new BXDVector3(3, 4, 0), output[1], "WrapArray2 output[1] mismatch");
    }
    [TestMethod]
    public void ArrayUtilities_WrapArray_Vec3()
    {
        double[] input = { 1, 2, 3, 4, 5, 6 };
        BXDVector3[] output = ArrayUtilities.WrapArray(delegate(double a, double b, double c)
        {
            return new BXDVector3(a, b, c);
        }, input);
        Assert.AreEqual(2, output.Length, "WrapArray3 length mismatch");
        Assert.AreEqual(new BXDVector3(1, 2, 3), output[0], "WrapArray3 output[0] mismatch");
        Assert.AreEqual(new BXDVector3(4, 5, 6), output[1], "WrapArray3 output[1] mismatch");
    }
    [TestMethod]
    public void ArrayUtilities_WrapArray_Color()
    {
        uint[] color = { 0x01234567, 0x76543210 };
        byte[][] output = ArrayUtilities.WrapArray(delegate(byte a, byte b, byte c, byte d)
        {
            return new byte[] { a, b, c, d };
        }, color);
        Assert.AreEqual(2, output.Length, "WrapArrayColor length mismatch");
        AssertExtensions.AreEqual(new byte[]{0x67,0x45,0x23,0x01}, output[0],0, "WrapArrayColor output[0] mismatch");
        AssertExtensions.AreEqual(new byte[]{0x10, 0x32, 0x54, 0x76}, output[1], 0,"WrapArrayColor output[1] mismatch");
    }
}
