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

    private static void getRotationalInfo(dynamic geom, out Point groupABase)
    {
        if (geom is EdgeProxy)
        {
            EdgeProxy edge = (EdgeProxy)geom;
            if (edge.GeometryType == CurveTypeEnum.kCircularArcCurve || edge.GeometryType == CurveTypeEnum.kCircleCurve)
            {
                groupABase = geom.Geometry.Center;
            }
            else if (edge.GeometryType == CurveTypeEnum.kLineSegmentCurve || edge.GeometryType == CurveTypeEnum.kLineCurve)
            {
                groupABase = edge.Geometry.MidPoint;
                // mid points look right...
            }
            else
            {
                throw new Exception("Unimplemented " + Enum.GetName(typeof(CurveTypeEnum), edge.GeometryType));
            }
        }
        else if (geom is FaceProxy)
        {
            FaceProxy face = (FaceProxy)geom;
            Console.WriteLine("FaceType: " + Enum.GetName(typeof(SurfaceTypeEnum), face.SurfaceType));
            if (face.SurfaceType == SurfaceTypeEnum.kPlaneSurface)
            {
                groupABase = face.Geometry.RootPoint;
            }
            else if (face.SurfaceType == SurfaceTypeEnum.kCylinderSurface)
            {
                groupABase = face.Geometry.BasePoint;
                // face.Geometry.Radius
            }
            else
            {
                throw new Exception("Unimplemented surface type " + Enum.GetName(typeof(SurfaceTypeEnum), face.SurfaceType));
            }
        }
        else
        {
            throw new Exception("Unimplemented proxy object " + Enum.GetName(typeof(ObjectTypeEnum), geom.Type));
        }
    }

    public RotationalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(isRotationalJoint(rigidJoint)))
            throw new Exception("Not a rotational joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        UnitVector jointInvNormal; //Stores invetors vector of the axis of rotation.
        Point groupABase = null;
        Point groupBBase = null;
        Console.WriteLine("O1: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Geometry.Type));
        Console.WriteLine("O2: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Geometry.Type));
        int translationDegrees;
        ObjectsEnumerator translationAxes;
        int rotationDegrees;
        ObjectsEnumerator rotationAxes;
        Point center;
        IEnumerator axesEnumerator;

        //Tries to find the axis of rotation from the first occurrence.
        wrapped.asmJointOccurrence.AffectedOccurrenceOne.GetDegreesOfFreedom(out translationDegrees, out translationAxes,
            out rotationDegrees, out rotationAxes, out center);

        //Tries to find the axis of rotation from the second occurrence.
        if (rotationDegrees == 0)
        {
            wrapped.asmJointOccurrence.AffectedOccurrenceTwo.GetDegreesOfFreedom(out translationDegrees, out translationAxes,
            out rotationDegrees, out rotationAxes, out center);
        }

        //Tried and failed.  Gives up.
        if (rotationDegrees == 0)
        {
            throw new Exception("No axis of rotation found for rotational joint.");
        }

        if (translationDegrees == 1)
        {
            axesEnumerator = translationAxes.GetEnumerator();
            axesEnumerator.MoveNext();
            jointInvNormal = ((Vector)axesEnumerator.Current).AsUnitVector();
        }
        else if (translationDegrees == 0)
        {
            jointInvNormal = null;
        }
        else
        {
            throw new Exception("More than one linear axis of freedom found on linear joint.");
        }

        getRotationalInfo(wrapped.asmJoint.OriginOne.Geometry, out groupABase);
        getRotationalInfo(wrapped.asmJoint.OriginTwo.Geometry, out groupBBase);

        if (wrapped.childIsTheOne)
        {
            jointNormal = Utilities.toBXDVector(jointInvNormal);
            childBase = Utilities.toBXDVector(groupABase);
            parentBase = Utilities.toBXDVector(groupBBase);
        }
        else
        {
            jointNormal = Utilities.toBXDVector(jointInvNormal);
            childBase = Utilities.toBXDVector(groupBBase);
            parentBase = Utilities.toBXDVector(groupABase);
        }

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