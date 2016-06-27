/*
 * Stores the data/functions for an Inventor planar joint.
 */

using System;
using Inventor;

class PlanarJoint : PlanarJoint_Base, InventorSkeletalJoint
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
    public static bool IsPlanarJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            return joint.JointType == AssemblyJointTypeEnum.kPlanarJointType;
        }
        return false;
    }

    public PlanarJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(IsPlanarJoint(rigidJoint)))
            throw new Exception("Not a planar joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);


        if (wrapped.childGroup == rigidJoint.groupOne)
        {
            normal = Utilities.ToBXDVector(rigidJoint.geomTwo.Normal);
            basePoint = Utilities.ToBXDVector(rigidJoint.geomTwo.RootPoint);
        }
        else
        {
            normal = Utilities.ToBXDVector(rigidJoint.geomOne.Normal);
            basePoint = Utilities.ToBXDVector(rigidJoint.geomOne.RootPoint);
        }
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " rotates about " + wrapped.parentGroup;
    }
}

