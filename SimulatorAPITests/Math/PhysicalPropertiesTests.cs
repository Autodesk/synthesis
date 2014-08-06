using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
public class PhysicalPropertiesTests
{
    [TestMethod]
    public void PhysicalProperties_RW()
    {
        MemoryStream stream = new MemoryStream();
        BinaryReader reader = new BinaryReader(stream);
        BinaryWriter writer = new BinaryWriter(stream);
        PhysicalProperties props = new PhysicalProperties();
        {
            Random rand = new Random();
            props.mass = (float) rand.NextDouble();
            props.centerOfMass = new BXDVector3(rand.NextDouble(), rand.NextDouble(), rand.NextDouble());
        }
        props.WriteData(writer);
        stream.Position = 0;
        PhysicalProperties result = new PhysicalProperties();
        result.ReadData(reader);
        Assert.AreEqual(props.centerOfMass, result.centerOfMass, "Physical properties center of mass inequal");
        Assert.AreEqual(props.mass, result.mass, "Physical properties mass is inequal");
        stream.Close();
    }
    [TestMethod]
    public void PhysicalProperties_Add()
    {
        PhysicalProperties props = new PhysicalProperties();
        props.mass = 1;
        props.centerOfMass = new BXDVector3(1,2,3);
        props.Add(2, new BXDVector3(5,4,3));
        float expectedMass = 1.0F + 2.0F;
        BXDVector3 expectedCOM = new BXDVector3(11.0 / 3.0, 10.0 / 3.0, 9.0 / 3.0);
        Console.WriteLine(expectedCOM);
        Console.WriteLine(props.centerOfMass);
        Assert.AreEqual(props.mass, expectedMass, "Physical properties add failed on mass");
        Assert.AreEqual(props.centerOfMass, expectedCOM, "Physical properties add failed on COM");
    }
}