using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class RotationalJoint : RotationalJoint_Base
{
    public SkeletalJoint wrapped;

    public static bool isRotationalJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            //Checks if there is no linear motion allowed.
            return joint.JointType == AssemblyJointTypeEnum.kRotationalJointType
                || (joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType
                && joint.HasLinearPositionEndLimit && joint.HasLinearPositionEndLimit
                && joint.LinearPositionStartLimit() == joint.LinearPositionEndLimit());
        }
        return false;
    }

    private static void getRotationalInfo(AssemblyJoint joint, out UnitVector jointVector, out Point jointBase)
    {
        int translationDegrees;
        ObjectsEnumerator translationAxes;
        int rotationDegrees;
        ObjectsEnumerator rotationAxes;
        Point center;
        IEnumerator axesEnumerator;

        //Tries to find the axis of rotation from the first occurrence.
        joint.AffectedOccurrenceOne.GetDegreesOfFreedom(out translationDegrees, out translationAxes,
            out rotationDegrees, out rotationAxes, out center);

        //Tries to find the axis of rotation from the second occurrence.
        if (rotationDegrees == 0)
        {
            joint.AffectedOccurrenceTwo.GetDegreesOfFreedom(out translationDegrees, out translationAxes,
            out rotationDegrees, out rotationAxes, out center);
        }

        //Tried and failed.  Gives up.
        if (rotationDegrees == 0)
        {
            throw new Exception("No axis of rotation found for rotational joint.");
        }

        axesEnumerator = rotationAxes.GetEnumerator();
        axesEnumerator.MoveNext();
        jointVector = ((Vector)axesEnumerator.Current).AsUnitVector();
        jointBase = center;
    }

    public RotationalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(isRotationalJoint(rigidJoint)))
            throw new Exception("Not a rotational joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        UnitVector jointInvNormal; //Stores invetors vector of the axis of rotation.
        Point jointInvBase = null;
        Console.WriteLine("O1: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Geometry.Type));
        Console.WriteLine("O2: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Geometry.Type));

        getRotationalInfo(wrapped.asmJointOccurrence, out jointInvNormal, out jointInvBase);

        jointNormal = Utilities.toBXDVector(jointInvNormal);
        jointBase = Utilities.toBXDVector(jointInvBase);

        currentAngularPosition = !((wrapped.asmJoint.AngularPosition == null)) ? wrapped.asmJoint.AngularPosition.Value : 0;
        hasAngularLimit = wrapped.asmJoint.HasAngularPositionLimits;
        if ((hasAngularLimit))
        {
            angularLimitLow = wrapped.asmJoint.AngularPositionStartLimit.Value;
            angularLimitHigh = wrapped.asmJoint.AngularPositionEndLimit.Value;
        }
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " rotates about " + wrapped.parentGroup;
    }
}