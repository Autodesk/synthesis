using System;
using Inventor;

public class CylindricalJoint : CylindricalJoint_Base, InventorSkeletalJoint
{
    private SkeletalJoint wrapped;

    public SkeletalJoint GetWrapped()
    {
        return wrapped;
    }

    public void DetermineLimits()
    {
        MotionLimits cache = new MotionLimits();
        DriveSettings driver = wrapped.asmJointOccurrence.DriveSettings;
        driver.CollisionDetection = true;
        driver.OnCollision += MotionLimits.OnCollisionEvent;
        driver.FrameRate = 1000;

        cache.DoContactSetup(true, wrapped.childGroup, wrapped.parentGroup);

        {   // Rotational motion
            driver.DriveType = DriveTypeEnum.kDriveAngularPositionType;
            wrapped.asmJoint.LinearPosition.Value = currentLinearPosition;

            float step = 0.05f; // rad
            driver.SetIncrement(IncrementTypeEnum.kAmountOfValueIncrement, step + " rad");

            driver.StartValue = currentAngularPosition + " rad";
            driver.EndValue = (currentAngularPosition + 6.5) + " rad";

            // Forward
            driver.GoToStart();
            MotionLimits.DID_COLLIDE = false;
            driver.PlayForward();
            if (MotionLimits.DID_COLLIDE)
            {
                angularLimitHigh = (float)wrapped.asmJoint.AngularPosition.Value - step;
                hasAngularLimit = true;
            }

            // Reverse
            driver.EndValue = currentAngularPosition + " rad";
            driver.StartValue = (currentAngularPosition - 6.5) + " rad";
            driver.GoToEnd();
            MotionLimits.DID_COLLIDE = false;
            driver.PlayReverse();
            if (MotionLimits.DID_COLLIDE)
            {
                angularLimitLow = (float)wrapped.asmJoint.AngularPosition.Value + step;
                if (!hasAngularLimit)
                {
                    angularLimitHigh = angularLimitLow + 6.28f - (step * 2.0f);
                }
                hasAngularLimit = true;
            }
            else if (hasAngularLimit)
            {
                angularLimitLow = angularLimitHigh - 6.28f + (step * 2.0f);
            }

            wrapped.asmJoint.AngularPosition.Value = currentAngularPosition;

            Console.WriteLine(hasAngularLimit + "; high: " + angularLimitHigh + "; low: " + angularLimitLow);
        }

        {   // Linear motion
            driver.DriveType = DriveTypeEnum.kDriveLinearPositionType;
            wrapped.asmJoint.AngularPosition.Value = currentAngularPosition;

            float step = 0.1f; // cm
            Box mover = (wrapped.childIsTheOne ? wrapped.asmJointOccurrence.OccurrenceOne : wrapped.asmJointOccurrence.OccurrenceTwo).RangeBox;
            float maxOffset = (float)mover.MinPoint.VectorTo(mover.MaxPoint).DotProduct(Utilities.ToInventorVector(childNormal));
            Console.WriteLine("Max linear offset: " + maxOffset);

            driver.SetIncrement(IncrementTypeEnum.kAmountOfValueIncrement, step + " cm");

            driver.StartValue = currentLinearPosition + " cm";
            driver.EndValue = (currentLinearPosition + maxOffset) + " cm";

            // Forward
            driver.GoToStart();
            MotionLimits.DID_COLLIDE = false;
            driver.PlayForward();
            if (MotionLimits.DID_COLLIDE)
            {
                linearLimitEnd = (float)wrapped.asmJoint.LinearPosition.Value - step;
                hasLinearEndLimit = true;
            }

            // Reverse
            driver.EndValue = currentLinearPosition + " cm";
            driver.StartValue = (currentLinearPosition - maxOffset) + " cm";
            driver.GoToEnd();
            MotionLimits.DID_COLLIDE = false;
            driver.PlayReverse();
            if (MotionLimits.DID_COLLIDE)
            {
                linearLimitStart = (float)wrapped.asmJoint.LinearPosition.Value + step;
                hasLinearStartLimit = true;
            }

            wrapped.asmJoint.LinearPosition.Value = currentLinearPosition;
            Console.WriteLine(hasLinearStartLimit + " low: " + linearLimitStart + "\t" + hasLinearEndLimit + " high: " + linearLimitEnd);
        }

        driver.OnCollision -= MotionLimits.OnCollisionEvent;
        cache.DoContactSetup(false, wrapped.childGroup, wrapped.parentGroup);

        // Stash results
        wrapped.asmJoint.HasLinearPositionStartLimit = hasLinearStartLimit;
        wrapped.asmJoint.HasLinearPositionEndLimit = hasLinearEndLimit;
        wrapped.asmJoint.LinearPositionStartLimit.Value = linearLimitStart;
        wrapped.asmJoint.LinearPositionEndLimit.Value = linearLimitEnd;
        wrapped.asmJoint.HasAngularPositionLimits = hasAngularLimit;
        wrapped.asmJoint.AngularPositionStartLimit.Value = angularLimitLow;
        wrapped.asmJoint.AngularPositionEndLimit.Value = angularLimitHigh;
    }

    public static bool IsCylindricalJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            return joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType;
        }
        return false;
    }

    private static void GetCylindricalInfo(dynamic geom, out UnitVector groupANormal, out Point groupABase)
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
        if (!(IsCylindricalJoint(rigidJoint)))
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

        GetCylindricalInfo(wrapped.asmJoint.OriginOne.Geometry, out groupANormal, out groupABase);
        GetCylindricalInfo(wrapped.asmJoint.OriginTwo.Geometry, out groupBNormal, out groupBBase);

        if (wrapped.childIsTheOne)
        {
            childNormal = Utilities.ToBXDVector(groupANormal);
            childBase = Utilities.ToBXDVector(groupABase);
            parentNormal = Utilities.ToBXDVector(groupBNormal);
            parentBase = Utilities.ToBXDVector(groupBBase);
        }
        else
        {
            childNormal = Utilities.ToBXDVector(groupBNormal);
            childBase = Utilities.ToBXDVector(groupBBase);
            parentNormal = Utilities.ToBXDVector(groupANormal);
            parentBase = Utilities.ToBXDVector(groupABase);
        }

        currentLinearPosition = wrapped.asmJoint.LinearPosition != null ? (float)wrapped.asmJoint.LinearPosition.Value : 0;
        currentAngularPosition = wrapped.asmJoint.AngularPosition != null ? (float)wrapped.asmJoint.AngularPosition.Value : 0;

        hasAngularLimit = wrapped.asmJoint.HasAngularPositionLimits;
        if (hasAngularLimit)
        {
            angularLimitLow = (float)wrapped.asmJoint.AngularPositionStartLimit.Value;
            angularLimitHigh = (float)wrapped.asmJoint.AngularPositionEndLimit.Value;
        }

        hasLinearStartLimit = wrapped.asmJoint.HasLinearPositionStartLimit;
        if (hasLinearStartLimit)
        {
            linearLimitStart = (float)wrapped.asmJoint.LinearPositionStartLimit.Value;
        }

        hasLinearEndLimit = wrapped.asmJoint.HasLinearPositionEndLimit;
        if (hasLinearEndLimit)
        {
            linearLimitEnd = (float)wrapped.asmJoint.LinearPositionEndLimit.Value;
        }
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " rotates about " + wrapped.parentGroup;
    }
}