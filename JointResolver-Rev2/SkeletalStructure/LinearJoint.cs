using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class LinearJoint : LinearJoint_Base
{
    public SkeletalJoint wrapped;

    public static bool isLinearJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            //Cylindrical joints with no rotaion are effectively sliding joints.
            return joint.JointType == AssemblyJointTypeEnum.kSlideJointType 
                || (joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType 
                && joint.HasAngularPositionLimits && joint.AngularPositionStartLimit == joint.AngularPositionEndLimit);
        }
        return false;
    }

    public LinearJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(isLinearJoint(rigidJoint)))
            throw new Exception("Not a linear joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        UnitVector groupANormal;
        UnitVector groupBNormal;
        Point groupABase;
        Point groupBBase;
        groupANormal = wrapped.asmJoint.AlignmentOne.Normal;
        groupABase = wrapped.asmJoint.AlignmentOne.RootPoint;
        groupBNormal = wrapped.asmJoint.AlignmentTwo.Normal;
        groupBBase = wrapped.asmJoint.AlignmentTwo.RootPoint;
        if (wrapped.childIsTheOne)
        {
            childNormal = Utilities.toBXDVector(groupANormal);
            childBase = Utilities.toBXDVector(groupABase);
            parentNormal = Utilities.toBXDVector(groupBNormal);
            parentBase = Utilities.toBXDVector(groupBBase);
        }
        else
        {
            childNormal = Utilities.toBXDVector(groupBNormal);
            childBase = Utilities.toBXDVector(groupBBase);
            parentNormal = Utilities.toBXDVector(groupANormal);
            parentBase = Utilities.toBXDVector(groupABase);
        }

        currentLinearPosition = !((wrapped.asmJoint.LinearPosition == null)) ? wrapped.asmJoint.LinearPosition.Value : 0;
        if (hasUpperLimit = wrapped.asmJoint.HasLinearPositionEndLimit)
        {
            linearLimitHigh = wrapped.asmJoint.LinearPositionEndLimit.Value;
        }
        if (hasLowerLimit = wrapped.asmJoint.HasLinearPositionStartLimit)
        {
            linearLimitLow = wrapped.asmJoint.LinearPositionStartLimit.Value;
        }
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " translates along " + wrapped.parentGroup;
    }
}