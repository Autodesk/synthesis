/*
 * Stores the data/functions for an Inventor planar joint.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

class PlanarJoint : PlanarJoint_Base
{
    public SkeletalJoint wrapped;

    public static bool isPlanarJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            return joint.JointType == AssemblyJointTypeEnum.kPlanarJointType;
        }
        return false;
    }
 
    private static void getPlanarInfo(dynamic geom, out UnitVector groupANormal, out Point groupABase)
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

    public PlanarJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(isPlanarJoint(rigidJoint)))
            throw new Exception("Not a planar joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        UnitVector groupANormal = null;
        UnitVector groupBNormal = null;
        Point groupABase = null;
        Point groupBBase = null;
        Console.WriteLine("O1: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Geometry.Type));
        Console.WriteLine("O2: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Geometry.Type));


        getPlanarInfo(wrapped.asmJoint.OriginOne.Geometry, out groupANormal, out groupABase);
        getPlanarInfo(wrapped.asmJoint.OriginTwo.Geometry, out groupBNormal, out groupBBase);

        if (wrapped.childIsTheOne)
        {
            childNormal = Utilities.toBXDVector(groupANormal);
            parentNormal = Utilities.toBXDVector(groupBNormal);

            childBase = Utilities.toBXDVector(groupABase);
            parentBase = Utilities.toBXDVector(groupBBase);
        }
        else
        {
            childNormal = Utilities.toBXDVector(groupBNormal);
            parentNormal = Utilities.toBXDVector(groupANormal);

            childBase = Utilities.toBXDVector(groupBBase);
            parentBase = Utilities.toBXDVector(groupABase);
        }
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " rotates about " + wrapped.parentGroup;
    }
}

