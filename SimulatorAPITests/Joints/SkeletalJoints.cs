using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SkeletalJoints_Testing
{
    private const int LIMIT_LINEAR_LOW = 1;
    private const int LIMIT_LINEAR_HIGH = 2;
    private const int LIMIT_ANGULAR = 4;

    private int[] GetLimitMasks(bool ang, bool linear)
    {
        if (ang && linear)
        {
            return new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        }else if (ang) {
            return new int[] { 0, 4 };
        }else if (linear) {
            return new int[] { 0, 1, 2, 3 };
        }else{
            return new int[] { 0 };
        }
    }

    private static BXDVector3 randVector()
    {
        Random rand = new Random();
        return new BXDVector3(rand.NextDouble(), rand.NextDouble(), rand.NextDouble());
    }

    [TestMethod]
    public void TestCylindricalRW()
    {
        Random rand = new Random();
        // Test each possible limit combo
        foreach (int mask in GetLimitMasks(true, true))
        {
            CylindricalJoint_Base joint = new CylindricalJoint_Base();
            joint.angularLimitHigh =rand.Next();
            joint.angularLimitLow = rand.Next();
            joint.axis = randVector();
            joint.basePoint = randVector();
            joint.currentAngularPosition = rand.Next();
            joint.currentLinearPosition = rand.Next();
            joint.hasAngularLimit = (mask & LIMIT_ANGULAR) == LIMIT_ANGULAR;
            joint.hasLinearEndLimit = (mask & LIMIT_LINEAR_HIGH) == LIMIT_LINEAR_HIGH;
            joint.hasLinearStartLimit = (mask & LIMIT_LINEAR_LOW) == LIMIT_LINEAR_LOW;
            
            string spec = "CylindricalJoint: " + (joint.hasAngularLimit ? "A" : "") + (joint.hasLinearEndLimit ? "EL" : "") + (joint.hasLinearStartLimit ? "SL" : "");

            SkeletalJoint_Base result = TestUtils.WriteReadObject((SkeletalJoint_Base) joint, SkeletalJoint_Base.ReadJointFully);
            Assert.IsNotNull(result, spec + " read null");
            Assert.IsInstanceOfType(result, typeof(CylindricalJoint_Base), spec + " type incorrect");
            CylindricalJoint_Base cjb = (CylindricalJoint_Base) result;
            Assert.AreEqual(joint.axis, cjb.axis, spec + " axis mismatch");
            Assert.AreEqual(joint.basePoint, cjb.basePoint, spec + " base point mismatch");
            Assert.AreEqual(joint.currentLinearPosition, cjb.currentLinearPosition, spec + " current linear pos");
            Assert.AreEqual(joint.currentAngularPosition, cjb.currentAngularPosition, spec + " current angular pos");
            Assert.AreEqual(joint.hasAngularLimit, cjb.hasAngularLimit, spec + " has angular limit");
            Assert.AreEqual(joint.hasLinearEndLimit, cjb.hasLinearEndLimit, spec + " has linear end limit");
            Assert.AreEqual(joint.hasLinearStartLimit, cjb.hasLinearStartLimit, spec + " has linear start limit");
            if (joint.hasAngularLimit)
            {
                Assert.AreEqual(joint.angularLimitLow, cjb.angularLimitLow, spec + " angular limit low");
                Assert.AreEqual(joint.angularLimitHigh, cjb.angularLimitHigh, spec + " angular limit high");
            }
            if (joint.hasLinearStartLimit)
                Assert.AreEqual(joint.linearLimitStart, cjb.linearLimitStart, spec + " linear limit start");
            if (joint.hasLinearEndLimit)
                Assert.AreEqual(joint.linearLimitEnd, cjb.linearLimitEnd, spec + " linear limit end");
            Assert.AreEqual(joint.attachedSensors.Count, cjb.attachedSensors.Count, " attached sensors");
        }
    }

    [TestMethod]
    public void TestRotationalRW()
    {
        Random rand = new Random();
        // Test each possible limit combo
        foreach (int mask in GetLimitMasks(true, false))
        {
            RotationalJoint_Base joint = new RotationalJoint_Base();
            joint.angularLimitHigh = rand.Next();
            joint.angularLimitLow = rand.Next();
            joint.axis = randVector();
            joint.basePoint = randVector();
            joint.currentAngularPosition = rand.Next();
            joint.hasAngularLimit = (mask & LIMIT_ANGULAR) == LIMIT_ANGULAR;

            string spec = "RotationalJoint: " + (joint.hasAngularLimit ? "A" : "");

            SkeletalJoint_Base result = TestUtils.WriteReadObject((SkeletalJoint_Base) joint, SkeletalJoint_Base.ReadJointFully);
            Assert.IsNotNull(result, spec + " read null");
            Assert.IsInstanceOfType(result, typeof(RotationalJoint_Base), spec + " type incorrect");
            RotationalJoint_Base cjb = (RotationalJoint_Base) result;
            Assert.AreEqual(joint.axis, cjb.axis, spec + " axis mismatch");
            Assert.AreEqual(joint.basePoint, cjb.basePoint, spec + " base point mismatch");
            Assert.AreEqual(joint.currentAngularPosition, cjb.currentAngularPosition, spec + " current angular pos");
            Assert.AreEqual(joint.hasAngularLimit, cjb.hasAngularLimit, spec + " has angular limit");
            if (joint.hasAngularLimit)
            {
                Assert.AreEqual(joint.angularLimitLow, cjb.angularLimitLow, spec + " angular limit low");
                Assert.AreEqual(joint.angularLimitHigh, cjb.angularLimitHigh, spec + " angular limit high");
            }
            Assert.AreEqual(joint.attachedSensors.Count, cjb.attachedSensors.Count, " attached sensors");
        }
    }

    [TestMethod]
    public void TestLinearRW()
    {
        Random rand = new Random();
        // Test each possible limit combo
        foreach (int mask in GetLimitMasks(false, true))
        {
            LinearJoint_Base joint = new LinearJoint_Base();
            joint.axis = randVector();
            joint.basePoint = randVector();
            joint.currentLinearPosition = rand.Next();
            joint.hasUpperLimit = (mask & LIMIT_LINEAR_HIGH) == LIMIT_LINEAR_HIGH;
            joint.hasLowerLimit = (mask & LIMIT_LINEAR_LOW) == LIMIT_LINEAR_LOW;

            string spec = "LinearJoint: " + (joint.hasUpperLimit ? "EL" : "") + (joint.hasLowerLimit ? "SL" : "");

            SkeletalJoint_Base result = TestUtils.WriteReadObject((SkeletalJoint_Base) joint, SkeletalJoint_Base.ReadJointFully);
            Assert.IsNotNull(result, spec + " read null");
            Assert.IsInstanceOfType(result, typeof(LinearJoint_Base), spec + " type incorrect");
            LinearJoint_Base cjb = (LinearJoint_Base) result;
            Assert.AreEqual(joint.axis, cjb.axis, spec + " axis mismatch");
            Assert.AreEqual(joint.basePoint, cjb.basePoint, spec + " base point mismatch");
            Assert.AreEqual(joint.currentLinearPosition, cjb.currentLinearPosition, spec + " current linear pos");
            Assert.AreEqual(joint.hasUpperLimit, cjb.hasUpperLimit, spec + " has linear end limit");
            Assert.AreEqual(joint.hasLowerLimit, cjb.hasLowerLimit, spec + " has linear start limit");
            if (joint.hasLowerLimit)
                Assert.AreEqual(joint.linearLimitLow, cjb.linearLimitLow, spec + " linear limit start");
            if (joint.hasUpperLimit)
                Assert.AreEqual(joint.linearLimitHigh, cjb.linearLimitHigh, spec + " linear limit end");
            Assert.AreEqual(joint.attachedSensors.Count, cjb.attachedSensors.Count, " attached sensors");
        }
    }

}
