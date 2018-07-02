using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synthesis.FEA
{
    public struct ContactDescriptor
    {
        /// <summary>
        /// The applied impulse of the contact.
        /// </summary>
        public float AppliedImpulse { get; set; }

        /// <summary>
        /// The position in between the contact points of object A and object B.
        /// </summary>
        public BulletSharp.Math.Vector3 Position { get; set; }

        /// <summary>
        /// The BRigidBody associated with the robot.
        /// </summary>
        public BRigidBody RobotBody { get; set; }
    }
}