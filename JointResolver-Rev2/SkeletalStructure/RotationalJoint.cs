using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class RotationalJoint : RotationalJoint_Base, InventorSkeletalJoint
{
    private SkeletalJoint wrapped;

    public SkeletalJoint getWrapped() { return wrapped; }

    public void determineLimits()
    {
        MotionLimits cache = new MotionLimits();
        DriveSettings driver = wrapped.asmJointOccurrence.DriveSettings;
        driver.DriveType = DriveTypeEnum.kDriveAngularPositionType;
        driver.CollisionDetection = true;
        driver.OnCollision += MotionLimits.onCollision;
        driver.FrameRate = 1000;
        float step = 0.05f;
        driver.SetIncrement(IncrementTypeEnum.kAmountOfValueIncrement, step + " rad");

        cache.doContactSetup(true, wrapped.childGroup, wrapped.parentGroup);

        driver.StartValue = currentAngularPosition + " rad";
        driver.EndValue = (currentAngularPosition + 6.5) + " rad";

        // Forward
        driver.GoToStart();
        MotionLimits.didCollide = false;
        driver.PlayForward();
        if (MotionLimits.didCollide)
        {
            angularLimitHigh = (float)wrapped.asmJoint.AngularPosition.Value - step;
            hasAngularLimit = true;
        }

        // Reverse
        driver.EndValue = currentAngularPosition + " rad";
        driver.StartValue = (currentAngularPosition - 6.5) + " rad";
        driver.GoToEnd();
        MotionLimits.didCollide = false;
        driver.PlayReverse();
        if (MotionLimits.didCollide)
        {
            angularLimitLow = (float)wrapped.asmJoint.AngularPosition.Value + step ;
            if (!hasAngularLimit)
            {
                angularLimitHigh = angularLimitLow + 6.28f - (step  * 2.0f);
            }
            hasAngularLimit = true;
        }
        else if (hasAngularLimit)
        {
            angularLimitLow = angularLimitHigh - 6.28f + (step * 2.0f);
        }

        driver.OnCollision -= MotionLimits.onCollision;
        cache.doContactSetup(false, wrapped.childGroup, wrapped.parentGroup);

        wrapped.asmJoint.AngularPosition.Value = currentAngularPosition;

        Console.WriteLine(hasAngularLimit + "; high: " + angularLimitHigh + "; low: " + angularLimitLow);

        // Stash results
        wrapped.asmJoint.HasAngularPositionLimits = hasAngularLimit;
        wrapped.asmJoint.AngularPositionStartLimit.Value = angularLimitLow;
        wrapped.asmJoint.AngularPositionEndLimit.Value = angularLimitHigh;
    }

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

    public RotationalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(isRotationalJoint(rigidJoint)))
            throw new Exception("Not a rotational joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        UnitVector groupANormal = null;
        UnitVector groupBNormal = null;
        Point groupABase = null;
        Point groupBBase = null;
        Console.WriteLine("O1: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginOne.Geometry.Type));
        Console.WriteLine("O2: " + Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Type) + "\t" +
            Enum.GetName(typeof(ObjectTypeEnum), wrapped.asmJoint.OriginTwo.Geometry.Type));


        getRotationalInfo(wrapped.asmJoint.OriginOne.Geometry, out groupANormal, out groupABase);
        getRotationalInfo(wrapped.asmJoint.OriginTwo.Geometry, out groupBNormal, out groupBBase);

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

        currentAngularPosition = !((wrapped.asmJoint.AngularPosition == null)) ? (float)wrapped.asmJoint.AngularPosition.Value : 0;
        hasAngularLimit = wrapped.asmJoint.HasAngularPositionLimits;
        if ((hasAngularLimit))
        {
            angularLimitLow = (float)wrapped.asmJoint.AngularPositionStartLimit.Value;
            angularLimitHigh = (float)wrapped.asmJoint.AngularPositionEndLimit.Value;
        }
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " rotates about " + wrapped.parentGroup;
    }
}