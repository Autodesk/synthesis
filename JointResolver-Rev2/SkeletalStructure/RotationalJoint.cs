using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class RotationalJoint : SkeletalJoint
{

    UnitVector parentNormal;
    UnitVector childNormal;
    Point parentBase;
    Point childBase;
    double currentAngularPosition;
    bool hasAngularLimit;
    double angularLimitLow;

    double angularLimitHigh;
    public static bool isRotationalJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            return joint.JointType == AssemblyJointTypeEnum.kRotationalJointType || (joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType && (!(joint.HasAngularPositionLimits) || joint.AngularPositionEndLimit.Value != joint.AngularPositionStartLimit.Value));
        }
        return false;
    }

    public RotationalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
        : base(parent, rigidJoint)
    {
        if (!(isRotationalJoint(rigidJoint)))
            throw new Exception("Not a rotational joint");

    }

    public override string ExportData()
    {
        UnitVector groupANormal;
        UnitVector groupBNormal;
        Point groupABase;
        Point groupBBase;
        if ((asmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType))
        {
            groupANormal = asmJoint.OriginOne.Geometry.Geometry.AxisVector;
            groupABase = asmJoint.OriginOne.Geometry.Geometry.BasePoint;
            groupBNormal = asmJoint.OriginTwo.Geometry.Geometry.AxisVector;
            groupBBase = asmJoint.OriginTwo.Geometry.Geometry.BasePoint;
        }
        else if (asmJoint.JointType == AssemblyJointTypeEnum.kRotationalJointType)
        {
            groupANormal = asmJoint.OriginOne.Geometry.Geometry.Normal;
            groupABase = asmJoint.OriginOne.Geometry.Geometry.Center;
            groupBNormal = asmJoint.OriginTwo.Geometry.Geometry.Normal;
            groupBBase = asmJoint.OriginTwo.Geometry.Geometry.Center;
        }
        else
        {
            return null;
        }
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

        currentAngularPosition = !((asmJoint.AngularPosition == null)) ? asmJoint.AngularPosition.Value : 0;
        hasAngularLimit = asmJoint.HasAngularPositionLimits;
        if ((hasAngularLimit))
        {
            angularLimitLow = asmJoint.AngularPositionStartLimit.Value;
            angularLimitHigh = asmJoint.AngularPositionEndLimit.Value;
        }
        return Program.printVector(parentBase) + ":" + Program.printVector(parentNormal) + ":" + Program.printVector(childBase) + ":" + Program.printVector(childNormal);
    }

    public override string ToString()
    {
        return "RotationalJoint";
    }
}