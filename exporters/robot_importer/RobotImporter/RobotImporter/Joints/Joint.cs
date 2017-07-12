using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotImporter.Joints
{
   
    public class Joint
    {
        public readonly UInt32 ID;

        public readonly UInt32 ParentID; 
        public readonly UInt32 ChildID;
        public readonly bool HasJointLimits;
        //if has Joint Limits
        public readonly UInt16 Friction;

        /// <summary>
        /// Rotational Joint: Vector normal to the plane of rotation
        /// Linear Joint: Vector parallel to the plane of movement
        /// Should be an array of 3 floats with indices 0, 1, 2 corresponding to X, Y, and Z respectively. 
        /// </summary>
        public readonly float[] DefVector;

        /// <summary>
        /// Rotational Joint: Point relative to the parent part
        /// Linear Joint: Point of connection relative to the parent part
        /// Should be an array of 3 floats with indices 0, 1, 2 corresponding to X, Y, and Z respectively. 
        /// </summary>
        public readonly float[] RelativePoint;

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

        public Joint(UInt32 id, UInt32 parentID, UInt32 childID, bool hasJointLimits, UInt16 friction,
            float[] defVector, float [] relativePoint, float proFreedomFactor, float retroFreedomFactor)
        {
            ID = id;


            ParentID = parentID;
            ChildID = childID;
            HasJointLimits = hasJointLimits;
            Friction = friction;

            DefVector = defVector;
            RelativePoint = relativePoint;
            ProFreedomFactor = proFreedomFactor;
            RetroFreedomFactor = retroFreedomFactor;
        }
    }
}
