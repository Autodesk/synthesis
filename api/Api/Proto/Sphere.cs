using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;

namespace SynthesisAPI.Proto {
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class Sphere: IMessage<Sphere> {
        public static Sphere CreateFromVerts(IEnumerable<Vec3> verts) {
            Sphere sphere = new Sphere();
            Vec3 min = new Vec3 { X = float.MaxValue, Y = float.MaxValue, Z = float.MaxValue },
                max = new Vec3 { X = float.MinValue, Y = float.MinValue, Z = float.MinValue };
            foreach (var v in verts) {
                if (v.X < min.X)
                    min.X = v.X;
                if (v.Y < min.Y)
                    min.Y = v.Y;
                if (v.Z < min.Z)
                    min.Z = v.Z;
                
                if (v.X > max.X)
                    max.X = v.X;
                if (v.Y > max.Y)
                    max.Y = v.Y;
                if (v.Z > max.Z)
                    max.Z = v.Z;
            }

            sphere.Center = (max + min) / 2;
            // sphere.Ra;
            foreach (var v in verts) {
                if ((v - sphere.Center).Magnitude > sphere.Radius) {
                    sphere.Radius = (v - sphere.Center).Magnitude;
                }
            }

            return sphere;
        }
    }
}