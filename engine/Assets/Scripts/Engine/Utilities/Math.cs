using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System;
using SynthesisAPI.Utilities;

namespace Utilities
{
    public static class Math
    {
        public static T Min<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b) < 0 ? a : b;
        }

        public static T Max<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b) < 0 ? b : a;
        }

        public static T Clamp<T>(T value, T min, T max) where T : IComparable
        {
            return Min(Max(value, max), min);
        }

        public static bool ApproxEquals(double a, double b)
        {
            return System.Math.Abs(a - b) < 0.001;
        }

        public static bool ApproxEquals(Quaternion a, Quaternion b)
        {
            return ApproxEquals(a.Real, b.Real) &&
                ApproxEquals(a.ImagX, b.ImagX) &&
                ApproxEquals(a.ImagY, b.ImagY) &&
                ApproxEquals(a.ImagZ, b.ImagZ);
        }

        public static bool SameSign(double a, double b)
        {
            return (a < 0) == (b < 0);
        }

        public static UnityEngine.Vector3 MapVector3D(Vector3D vec) =>
            MathUtil.MapVector3D(vec);
        public static Vector3D MapVector3(UnityEngine.Vector3 vec) =>
            MathUtil.MapVector3(vec);
        public static Quaternion MapUnityQuaternion(UnityEngine.Quaternion q) =>
            MathUtil.MapUnityQuaternion(q);
        public static UnityEngine.Quaternion MapQuaternion(Quaternion q) =>
            MathUtil.MapQuaternion(q);
    }
}
