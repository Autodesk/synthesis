using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
public class RobotSensorTesting
{
    [TestMethod]
    public void RobotSensor_TestRW()
    {
        RobotSensor sensor = new RobotSensor(RobotSensorType.ENCODER);
        sensor.equation = new Polynomial(new float[] { 1, 2, 3 });
        sensor.module = 72;
        sensor.port = 42;
        sensor.useSecondarySource = false;
        
        RobotSensor result = TestUtils.WriteReadObject(sensor);

        Assert.AreEqual(72, result.module, "Robot sensor module not expected value");
        Assert.AreEqual(42, result.port, "Robot sensor port not expected value");
        Assert.AreEqual(false, result.useSecondarySource, "Robot sensor use secondary source not expected value");
        AssertExtensions.AreEqual(new float[] { 1, 2, 3 }, result.equation.coeff, 0, "Robot sensor polynomial not expected value");
    }
}