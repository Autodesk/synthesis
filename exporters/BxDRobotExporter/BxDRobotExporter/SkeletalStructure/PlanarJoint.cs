/*
 * Stores the data/functions for an Inventor planar joint.
 */

using System;
using BxDRobotExporter.RigidAnalyzer;
using Inventor;

namespace BxDRobotExporter.SkeletalStructure
{
    internal class PlanarJoint : PlanarJoint_Base, INventorSkeletalJoint
    {
        private SkeletalJoint wrapped;

        public SkeletalJoint GetWrapped()
        {
            return wrapped;
        }

        public void DetermineLimits()
        {
            // TODO
        }

        public void ReloadInventorJoint()
        {
            if (wrapped.ChildGroup == wrapped.RigidJoint.GroupOne)
            {
                normal = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomTwo.Normal);
                basePoint = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomTwo.RootPoint);
            }
            else
            {
                normal = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomOne.Normal);
                basePoint = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomOne.RootPoint);
            }
        }

        public static bool IsPlanarJoint(CustomRigidJoint jointI)
        {
            if (jointI.Joints.Count == 1)
            {
                AssemblyJointDefinition joint = jointI.Joints[0].Definition;
                return joint.JointType == AssemblyJointTypeEnum.kPlanarJointType;
            }
            return false;
        }

        public PlanarJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
        {
            if (!(IsPlanarJoint(rigidJoint)))
                throw new Exception("Not a planar joint");
            wrapped = new SkeletalJoint(parent, rigidJoint);

            ReloadInventorJoint();
        }

        protected override string ToString_Internal()
        {
            return wrapped.ChildGroup + " rotates about " + wrapped.ParentGroup;
        }
    }
}

