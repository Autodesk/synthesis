using Google.Protobuf;
using UnityEngine;

namespace SynthesisAPI.Proto {
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class Quat: IMessage<Quat> {
        public static implicit operator Quat(BXDQuaternion q)
            => new Quat() { X = -q.X, Y = q.Y, Z = q.Z, W = -q.W };
        public static implicit operator BXDQuaternion(Quat q)
            => new BXDQuaternion() { X = -q.X, Y = q.Y, Z = q.Z, W = -q.W };
        public static implicit operator Quat(Quaternion q)
            => new Quat() {X = q.x, Y = q.y, Z = q.z, W = q.w};
        public static implicit operator Quaternion(Quat q)
            => new Quaternion(q.X, q.Y, q.Z, q.W).normalized;
    }
}