using System;
using BxDRobotExporter.RigidAnalyzer;
using BxDRobotExporter.Utilities;
using Inventor;

namespace BxDRobotExporter.SkeletalStructure
{
    public class BallJoint : BallJoint_Base, InventorSkeletalJoint
    {
        private SkeletalJoint wrapped;

        public SkeletalJoint GetWrapped()
        {
            return wrapped;
        }

        public void DetermineLimits()
        {
        } // TODO

        public void ReloadInventorJoint()
        {
            basePoint = MathUtils.ToBXDVector(wrapped.rigidJoint.geomOne);
        }

        public static bool IsBallJoint(CustomRigidJoint jointI)
        {
            if (jointI.joints.Count == 1)
            {
                AssemblyJointDefinition joint = jointI.joints[0].Definition;
                //Checks if there is no linear motion allowed.
                return joint.JointType == AssemblyJointTypeEnum.kBallJointType;
            }
            return false;
        }

        public BallJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
        {
            if (!(IsBallJoint(rigidJoint)))
                throw new Exception("Not a rotational joint");
            wrapped = new SkeletalJoint(parent, rigidJoint);

            ReloadInventorJoint();
        }

        protected override string ToString_Internal()
        {
            return wrapped.childGroup + " rotates about " + wrapped.parentGroup;
        }
    }
}