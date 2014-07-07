using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class CylindricalJoint : CylindricalJoint_Base, InventorSkeletalJoint
{
    private SkeletalJoint wrapped;

    public SkeletalJoint getWrapped() { return wrapped; }

    public static bool isCylindricalJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            return joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType;
        }
        return false;
    }

    private static void getCylindricalInfo(dynamic geom, out UnitVector groupANormal, out Point groupABase)
    {
        if (geom is EdgeProxy)
        {
            EdgeProxy edge = (EdgeProxy)geom;
            if (edge.GeometryType == CurveTypeEnum.kCircularArcCurve || edge.GeometryType == CurveTypeEnum.kCircleCurve)
            {
                groupANormal = geom.Geometry.Normal;
                groupABase = geom.Geometry.Center;
            }
            else if (edge.GeometryType == CurveTypeEnum.kLineSegmentCurve || edge.GeometryType == CurveTypeEnum.kLineCurve)
            {
                groupANormal = edge.Geometry.Direction;
                groupABase = edge.Geometry.MidPoint;
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
                groupANormal = face.Geometry.Normal;
                groupABase = face.Geometry.RootPoint;
            }
            else if (face.SurfaceType == SurfaceTypeEnum.kCylinderSurface)
            {
                groupABase = face.Geometry.BasePoint;
                groupANormal = face.Geometry.AxisVector;
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

    public CylindricalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(isCylindricalJoint(rigidJoint)))
            throw new Exception("Not a Cylindrical joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        UnitVector groupANormal = null;
        UnitVector groupBNormal = null;
        Point groupABase = null;
        Point groupBBase = null;
        Console.WriteLine("O1: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Geometry.Type));
        Console.WriteLine("O2: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Geometry.Type));

        getCylindricalInfo(wrapped.asmJoint.OriginOne.Geometry, out groupANormal, out groupABase);
        getCylindricalInfo(wrapped.asmJoint.OriginTwo.Geometry, out groupBNormal, out groupBBase);

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

        hasAngularLimit = wrapped.asmJoint.HasAngularPositionLimits;
        if (hasAngularLimit)
        {
            angularLimitLow = (float) wrapped.asmJoint.AngularPositionStartLimit.Value;
            angularLimitHigh = (float) wrapped.asmJoint.AngularPositionEndLimit.Value;
        }

        hasLinearStartLimit = wrapped.asmJoint.HasLinearPositionStartLimit;
        if (hasLinearStartLimit)
        {
            linearLimitStart = (float) wrapped.asmJoint.LinearPositionStartLimit.Value;
        }

        hasLinearEndLimit = wrapped.asmJoint.HasLinearPositionEndLimit;
        if (hasLinearEndLimit)
        {
            linearLimitEnd = (float) wrapped.asmJoint.LinearPositionEndLimit.Value;
        }
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " rotates about " + wrapped.parentGroup;
    }
}