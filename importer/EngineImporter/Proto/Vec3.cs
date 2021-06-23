using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Synthesis.Proto {
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class Vec3 {
        
        public float Magnitude => (float)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        public Vec3 Normalize() {
            var mag = Magnitude;
            X /= mag;
            Y /= mag;
            Z /= mag;
            return this;
        }

        public static implicit operator Vec3(UnityEngine.Vector3 v)
            => new Vec3() { X = v.x, Y = v.y, Z = v.z };
        public static implicit operator UnityEngine.Vector3(Vec3 v)
            => new UnityEngine.Vector3(v.X, v.Y, v.Z);
        public static implicit operator Vec3(BXDVector3 v)
            => new Vec3() { X = v.x * -0.01f, Y = v.y * 0.01f, Z = v.z * 0.01f };
        public static implicit operator BXDVector3(Vec3 v)
            => new BXDVector3(v.X * -100, v.Y * 100, v.Z * 100);

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3() {X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z};
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3() {X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z};
        public static Vec3 operator -(Vec3 a) => new Vec3() {X = -a.X, Y = -a.Y, Z = -a.Z};
        public static Vec3 operator /(Vec3 a, float b) => new Vec3 {X = a.X / b, Y = a.Y / b, Z = a.Z / b};
        public static Vec3 operator *(Vec3 a, float b) => new Vec3 {X = a.X * b, Y = a.Y * b, Z = a.Z * b};
    }
}
