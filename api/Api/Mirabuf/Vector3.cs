using System;

namespace Mirabuf {
    public partial class Vector3 {
        public float Magnitude => (float)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        public Vector3 Normalize() {
            var mag = Magnitude;
            X /= mag;
            Y /= mag;
            Z /= mag;
            return this;
        }

        public static implicit operator Vector3(UnityEngine.Vector3 v)
            => new Vector3() { X = v.x * -100, Y = v.y * 100, Z = v.z * 100 };
        public static implicit operator UnityEngine.Vector3(Vector3 v)
            => new UnityEngine.Vector3(v.X * -0.01f, v.Y * 0.01f, v.Z * 0.01f);
        public static implicit operator Vector3(BXDVector3 v)
            => new Vector3() { X = v.x, Y = v.y, Z = v.z };
        public static implicit operator BXDVector3(Vector3 v)
            => new BXDVector3(v.X, v.Y, v.Z);

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3() { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3() { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
        public static Vector3 operator -(Vector3 a) => new Vector3() { X = -a.X, Y = -a.Y, Z = -a.Z };
        public static Vector3 operator /(Vector3 a, float b) => new Vector3 { X = a.X / b, Y = a.Y / b, Z = a.Z / b };
        public static Vector3 operator *(Vector3 a, float b) => new Vector3 { X = a.X * b, Y = a.Y * b, Z = a.Z * b };
        public static Vector3 operator /(Vector3 a, double b) => new Vector3 { X = a.X / (float)b, Y = a.Y / (float)b, Z = a.Z / (float)b };
        public static Vector3 operator *(Vector3 a, double b) => new Vector3 { X = a.X * (float)b, Y = a.Y * (float)b, Z = a.Z * (float)b };
    }
}
