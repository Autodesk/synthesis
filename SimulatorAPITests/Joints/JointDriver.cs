using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
public class JointDriverTesting
{
    [TestMethod]
    public void JointDriver_ReadWrite()
    {
        JointDriver driver = new JointDriver(JointDriverType.MOTOR);
        driver.portA = 1;
        driver.portB = 2;
        driver.upperLimit = 1.234f;
        driver.lowerLimit = 1.01f;

        JointDriver result = TestUtils.WriteReadObject(driver);

        Assert.AreEqual(1, result.portA, "Joint driver portA not equal");
        Assert.AreEqual(2, result.portB, "Joint driver portB not equal");
        Assert.AreEqual(1.234f, result.upperLimit, "Joint driver upperLimit not equal");
        Assert.AreEqual(1.01f, result.lowerLimit, "Joint driver lowerLimit not equal");
    }

    [TestMethod]
    public void JointDriverMeta_Registration()
    {
        Type[] types = typeof(JointDriverMeta).Assembly.GetTypes();
        foreach (Type t in types)
        {
            if (t.IsSubclassOf(typeof(JointDriverMeta)))
            {
                Assert.IsTrue(Array.IndexOf(JointDriverMeta.JOINT_DRIVER_META_TYPES, t) >= 0, "Joint driver meta \"" + t.Name + "\" not registered in JointDriverMeta!");
            }
        }
    }

    [TestMethod]
    public void JointDriver_MetaStorage()
    {
        JointDriver storage = new JointDriver();
        foreach (Type t in JointDriverMeta.JOINT_DRIVER_META_TYPES)
        {
            Assert.IsNotNull(t.GetConstructor(new Type[0]), "Joint driver meta \"" + t.Name + "\" doesn't have a default constructor!");
            object o = t.GetConstructor(new Type[0]).Invoke(new object[0]);
            storage.AddInfo((JointDriverMeta) o);
            Assert.AreEqual(o, storage.GetInfo(t), "Joint driver meta \"" + t.Name + "\" retrieval failed.");
        }
    }
}
