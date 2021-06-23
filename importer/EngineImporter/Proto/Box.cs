using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;

namespace Synthesis.Proto {
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class Box: IMessage<Box> {
        public static Box CreateFromVerts(IEnumerable<Vec3> verts, FieldNode node) {
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
            
            return new Box { Center = ((min + max) / 2) - ((Vec3)node.Position), Size = max - min,
                Position = (Vec3)node.Position, Rotation = (Quat)node.Rotation};
        }
    }
}