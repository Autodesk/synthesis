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
 
    private static void getPlanarInfo(dynamic geom, out UnitVector groupANormal)
    {
        if (geom is EdgeProxy)
        {
            EdgeProxy edge = (EdgeProxy)geom;
            if (edge.GeometryType == CurveTypeEnum.kCircularArcCurve || edge.GeometryType == CurveTypeEnum.kCircleCurve)
            {
                groupANormal = geom.Geometry.Normal;
            }
            else if (edge.GeometryType == CurveTypeEnum.kLineSegmentCurve || edge.GeometryType == CurveTypeEnum.kLineCurve)
            {
                groupANormal = edge.Geometry.Direction;
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
            }
            else if (face.SurfaceType == SurfaceTypeEnum.kCylinderSurface)
            {
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
        Console.WriteLine("O1: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Geometry.Type));
        Console.WriteLine("O2: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Geometry.Type));


        getPlanarInfo(wrapped.asmJoint.OriginOne.Geometry, out groupANormal);
        getPlanarInfo(wrapped.asmJoint.OriginTwo.Geometry, out groupBNormal);

        if (wrapped.childIsTheOne)
        {
            childNormal = Utilities.toBXDVector(groupANormal);
            parentNormal = Utilities.toBXDVector(groupBNormal);
        }
        else
        {
            childNormal = Utilities.toBXDVector(groupBNormal);
            parentNormal = Utilities.toBXDVector(groupANormal);
        }
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " rotates about " + wrapped.parentGroup;
    }
}

