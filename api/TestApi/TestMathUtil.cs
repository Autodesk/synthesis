using System;
using NUnit.Framework;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using SynthesisAPI.Utilities;

namespace TestApi
{
    [TestFixture]
    public static class TestMathUtil
    {
        public static double Delta = 0.001;

        public static void AssertApproxEquals(EulerAngles expected, EulerAngles actual)
        {
            //Assert.True(expected.Equals(actual, Delta));
            Assert.AreEqual(expected.Alpha.Degrees, actual.Alpha.Degrees, Delta);
            Assert.AreEqual(expected.Beta.Degrees, actual.Beta.Degrees, Delta);
            Assert.AreEqual(expected.Gamma.Degrees, actual.Gamma.Degrees, Delta);
        }

        public static void AssertApproxEquals(Quaternion expected, Quaternion actual)
        {
            Assert.AreEqual(expected.Real, actual.Real, Delta);
            Assert.AreEqual(expected.ImagX, actual.ImagX, Delta);
            Assert.AreEqual(expected.ImagY, actual.ImagY, Delta);
            Assert.AreEqual(expected.ImagZ, actual.ImagZ, Delta);
        }

        private static double NormalizeAngle(double angle)
        {
            while (angle < 0)
                angle += 2 * System.Math.PI;

            while (angle >= (2 * System.Math.PI - Delta))
                angle -= 2 * System.Math.PI;

            return angle;
        }

        public static void AssertApproxEquals(Angle expected, Angle actual)
        {
            var e = NormalizeAngle(expected.Radians);
            var a = NormalizeAngle(actual.Radians);
            Assert.AreEqual(e, a, Delta);
        }

        [Test]
        public static void TestFromEuler()
        {
            var q1 = new Quaternion(1, 0, 0, 0);
            var q2 = MathUtil.FromEuler(q1.ToEulerAngles());
            AssertApproxEquals(q1, q2);

            var e = new EulerAngles(Angle.FromDegrees(60), Angle.FromDegrees(40), Angle.FromDegrees(30));
            var q = MathUtil.FromEuler(e);
            AssertApproxEquals(e, q.ToEulerAngles());
        }
    }
}
