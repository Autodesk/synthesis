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

    private static void getRotationalInfo(dynamic geom, out UnitVector groupANormal, out Point groupABase)
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
            Console.WriteLine("FaceType: " + Enum.GetName(typeof(SurfaceTypeEnum),face.SurfaceType));
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

    public RotationalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
        : base(parent, rigidJoint)
    {
        if (!(isRotationalJoint(rigidJoint)))
            throw new Exception("Not a rotational joint");

        UnitVector groupANormal = null;
        UnitVector groupBNormal = null;
        Point groupABase = null;
        Point groupBBase = null;
        Console.WriteLine("O1: " + Enum.GetName(typeof(ObjectTypeEnum), asmJoint.OriginOne.Type) + "\t" + Enum.GetName(typeof(ObjectTypeEnum), asmJoint.OriginOne.Geometry.Type));
        Console.WriteLine("O2: " + Enum.GetName(typeof(ObjectTypeEnum), asmJoint.OriginTwo.Type) + "\t" + Enum.GetName(typeof(ObjectTypeEnum), asmJoint.OriginTwo.Geometry.Type));

       
        getRotationalInfo(asmJoint.OriginOne.Geometry, out groupANormal, out groupABase);
        getRotationalInfo(asmJoint.OriginTwo.Geometry, out groupBNormal, out groupBBase);

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
    }

    public override string ExportData()
    {
        return "ROTATIONAL:" + Program.printVector(parentBase) + ":" + Program.printVector(parentNormal) + ":" + Program.printVector(childBase) + ":" + Program.printVector(childNormal);
    }

    public override SkeletalJointType getJointType()
    {
        return SkeletalJointType.ROTATIONAL;
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

        writer.Write((byte)(hasAngularLimit ? 1 : 0));
        if (hasAngularLimit)
        {
            writer.Write(angularLimitLow);
            writer.Write(angularLimitHigh);
        }
    }

    protected override string ToString_Internal()
    {
        string info = "Rotates " + childGroup.ToString() + " about " + parentGroup.ToString();
        return info;
    }
}