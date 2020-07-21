using NUnit.Framework;
using SynthesisAPI.EnvironmentManager.Components;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace TestApi
{
    [TestFixture]
    public static class TestTransform
    {
        [Test]
        public static void TestRotateQuaternion()
        {
            var a = new Transform();
            var rotation = new Quaternion(1, 1, 0.5, 0.2).Normalized;
            a.Rotate(rotation);
            TestMathUtil.AssertApproxEquals(rotation, a.Rotation);
        }

        [Test]
        public static void TestRotateAngleAxis()
        {
            var a = new Transform();
            var rotation = Angle.FromDegrees(30);
            a.Rotate(rotation, UnitVector3D.XAxis);
            Assert.AreEqual(rotation, a.Rotation.ToEulerAngles().Alpha);

            var b = new Transform();
            var rotation2 = Angle.FromDegrees(30);
            for (var i = 1; i < 13; i++)
            {
                b.Rotate(rotation2, UnitVector3D.XAxis);
                TestMathUtil.AssertApproxEquals(rotation2 * i, b.Rotation.ToEulerAngles().Alpha);
            }
        }

        [Test]
        public static void TestRotateEulerAngles()
        {
            var a = new Transform();
            var rotation = new EulerAngles(Angle.FromDegrees(30), Angle.FromDegrees(40), Angle.FromDegrees(50));
            a.Rotate(rotation);
            TestMathUtil.AssertApproxEquals(rotation, a.Rotation.ToEulerAngles());
        }
    }
}
