using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Synthesis.FEA
{
    /// <summary>
    /// Used for describing the state of a RigidBody for a given frame.
    /// </summary>
    [Serializable]
    public struct StateDescriptor
    {
        /// <summary>
        /// The position of the RigidBody.
        /// </summary>
        public BulletSharp.Math.Vector3 Position { get; set; }

        /// <summary>
        /// The rotation of the RigidBody.
        /// </summary>
        public BulletSharp.Math.Matrix Rotation { get; set; }

        /// <summary>
        /// The linear velocity of the RigidBody.
        /// </summary>
        public BulletSharp.Math.Vector3 LinearVelocity { get; set; }

        /// <summary>
        /// The angular velocity of the RigidBody.
        /// </summary>
        public BulletSharp.Math.Vector3 AngularVelocity { get; set; }
    }
}
