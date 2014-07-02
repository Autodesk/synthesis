using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class LinearJoint : SkeletalJoint
{

    UnitVector parentNormal;
    UnitVector childNormal;
    Point parentBase;
    Point childBase;
    double currentLinearPosition;
    bool hasUpperLimit, hasLowerLimit;
    double linearLimitLow, linearLimitHigh;

    public static bool isLinearJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            return joint.JointType == AssemblyJointTypeEnum.kSlideJointType;
        }
        return false;
    }

    public LinearJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
        : base(parent, rigidJoint)
    {
        if (!(isLinearJoint(rigidJoint)))
            throw new Exception("Not a linear joint");

        UnitVector groupANormal;
        UnitVector groupBNormal;
        Point groupABase;
        Point groupBBase;
        groupANormal = asmJoint.AlignmentOne.Normal;
        groupABase = asmJoint.AlignmentOne.RootPoint;
        groupBNormal = asmJoint.AlignmentTwo.Normal;
        groupBBase = asmJoint.AlignmentTwo.RootPoint;
        if (childIsTheOne)
        {
            childNormal = groupANormal;
            childBase = groupABase;
            parentNormal = groupBNormal;
            parentBase = groupBBase;
        }
        else
        {
            childNormal = groupBNormal;
            childBase = groupBBase;
            parentNormal = groupANormal;
            parentBase = groupABase;
        }

        currentLinearPosition = !((asmJoint.LinearPosition == null)) ? asmJoint.LinearPosition.Value : 0;
        if (hasUpperLimit = asmJoint.HasLinearPositionEndLimit)
        {
            linearLimitHigh = asmJoint.LinearPositionEndLimit.Value;
        }
        if (hasLowerLimit = asmJoint.HasLinearPositionStartLimit)
        {
            linearLimitLow = asmJoint.LinearPositionStartLimit.Value;
        }
    }

    public override string ExportData()
    {
        return "LINEAR:" + Program.printVector(parentBase) + ":" + Program.printVector(parentNormal) + ":" + Program.printVector(childBase) + ":" + Program.printVector(childNormal);
    }

    public override SkeletalJointType getJointType()
    {
        return SkeletalJointType.LINEAR;
    }

    public override void writeJoint(System.IO.BinaryWriter writer)
    {
        writer.Write(parentBase.X);
        writer.Write(parentBase.Y);
        writer.Write(parentBase.Z);
        writer.Write(parentNormal.X);
        writer.Write(parentNormal.Y);
        writer.Write(parentNormal.Z);

        writer.Write(childBase.X);
        writer.Write(childBase.Y);
        writer.Write(childBase.Z);
        writer.Write(childNormal.X);
        writer.Write(childNormal.Y);
        writer.Write(childNormal.Z);

        writer.Write((byte)((hasLowerLimit ? 1 : 0) | (hasUpperLimit ? 2 : 0)));
        if (hasLowerLimit)
        {
            writer.Write(linearLimitLow);
        }
        if (hasUpperLimit)
        {
            writer.Write(linearLimitHigh);
        }
    }
    protected override string ToString_Internal()
    {
        string info =  "Moves " + childGroup.ToString() + " along " + parentGroup.ToString();
        return info;
    }
}