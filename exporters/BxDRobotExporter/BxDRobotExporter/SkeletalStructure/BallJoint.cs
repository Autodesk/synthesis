using System;
using BxDRobotExporter.RigidAnalyzer;
using Inventor;

namespace BxDRobotExporter.SkeletalStructure
{
    public class BallJoint : BallJoint_Base, INventorSkeletalJoint
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
            basePoint = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomOne);
        }

        public static bool IsBallJoint(CustomRigidJoint jointI)
        {
            if (jointI.Joints.Count == 1)
            {
                AssemblyJointDefinition joint = jointI.Joints[0].Definition;
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
            return wrapped.ChildGroup + " rotates about " + wrapped.ParentGroup;
        }
    }
}