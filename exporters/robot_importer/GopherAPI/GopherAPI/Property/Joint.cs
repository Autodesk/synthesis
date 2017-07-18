using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using GopherAPI.Other;

namespace GopherAPI.Properties
{
    public enum JointType { ROTATIONAL, LINEAR }
    public class Joint
    {
        public readonly JointType Type;
        public readonly UInt32 JointID;

        public readonly UInt32 ParentID;
        public readonly UInt32 ChildID;


        /// <summary>
        /// Rotational Joint: Vector normal to the plane of rotation
        /// Linear Joint: Vector parallel to the plane of movement
        /// Should be an array of 3 floats with indices 0, 1, 2 corresponding to X, Y, and Z respectively. 
        /// </summary>
        public readonly Vec3 DefVector;

        /// <summary>
        /// Rotational Joint: Point relative to the parent part
        /// Linear Joint: Point of connection relative to the parent part
        /// Should be an array of 3 floats with indices 0, 1, 2 corresponding to X, Y, and Z respectively. 
        /// </summary>
        public readonly Vec3 RelativePoint;

        /// <summary>
        /// Rotational Joint: Degree of freedom clockwise
        /// Linear Joint: Centimeters of freedom in the direction of the defining vector
        /// </summary>
        public readonly float ProFreedomFactor;

        /// <summary>
        /// Rotational Joint: Degree of freedom counter-clockwise
        /// Linear Joint: Centimeters of freedom opposite the direction of the defining vector
        /// </summary>
        public readonly float RetroFreedomFactor;

        public Joint(byte[] GenericData)
        {
            using (var Reader = new BinaryReader(new MemoryStream(GenericData)))
            {
                JointID = Reader.ReadUInt32();
                Type = (JointType)Reader.ReadUInt16();

                ParentID = Reader.ReadUInt32();
                ChildID = Reader.ReadUInt32();

                DefVector = new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
                RelativePoint = new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());

                ProFreedomFactor = Reader.ReadSingle();
                RetroFreedomFactor = Reader.ReadSingle();
            }
        }
    }
}
