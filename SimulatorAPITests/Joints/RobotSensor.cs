using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
public class RobotSensorTesting
{
    [TestMethod]
    public void RobotSensor_TestRW()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        BinaryReader reader = new BinaryReader(stream);

        RobotSensor sensor = new RobotSensor(RobotSensorType.ENCODER);
        sensor.equation = new Polynomial(new float[] { 1, 2, 3 });
        sensor.module = 72;
        sensor.port = 42;
        sensor.useSecondarySource = false;
        sensor.WriteData(writer);

        stream.Position = 0;
        RobotSensor result = new RobotSensor();
        result.ReadData(reader);

        Assert.AreEqual(72, result.module, "Robot sensor module not expected value");
        Assert.AreEqual(42, result.port, "Robot sensor port not expected value");
        Assert.AreEqual(false, result.useSecondarySource, "Robot sensor use secondary source not expected value");
        AssertExtensions.AreEqual(new float[] { 1, 2, 3 }, result.equation.coeff, 0, "Robot sensor polynomial not expected value");

        stream.Close();
    }
}