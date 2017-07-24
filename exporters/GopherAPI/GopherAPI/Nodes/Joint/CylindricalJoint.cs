﻿using GopherAPI.Other;

namespace GopherAPI.Nodes.Joint
{
    public class CylindricalJoint : GopherJoint_Base
    {
        /// <summary>
        /// Gets the Joint type
        /// </summary>
        /// <returns></returns>
        public override GopherJointType GetJointType()
        {
            return GopherJointType.CYLINDRICAL;
        }

        /// <summary>
        /// Vector normal to the plane of rotation
        /// </summary>
        public Vec3 NormalVector { get; internal set; }

        /// <summary>
        /// Vector parallel to movement
        /// </summary>
        public Vec3 DefiningVector { get; internal set; }

        /// <summary>
        /// Points of axes relative to parent part
        /// </summary>
        public Vec3 RelativePoint { get; internal set; }

        /// <summary>
        /// Point of connection relative to parent part
        /// </summary>
        public Vec3 ConnectionPoint { get; internal set; }

        /// <summary>
        /// Degree of angular freedom
        /// </summary>
        public Vec2 AngularFreedomFactor { get; internal set; }

        /// <summary>
        /// cm of freedom relative to the defining vector
        /// </summary>
        public Vec2 LinearFreedomFactor { get; internal set; }
    }
}