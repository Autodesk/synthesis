using System;
using Inventor;

public class LinearJoint : LinearJoint_Base, InventorSkeletalJoint
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
        driver.DriveType = DriveTypeEnum.kDriveLinearPositionType;
        driver.CollisionDetection = true;
        driver.OnCollision += MotionLimits.OnCollisionEvent;
        driver.FrameRate = 1;
        float step = 0.1f;
        Box mover = (wrapped.childIsTheOne ? wrapped.asmJointOccurrence.OccurrenceOne : wrapped.asmJointOccurrence.OccurrenceTwo).RangeBox;
        float maxOffset = (float)mover.MinPoint.VectorTo(mover.MaxPoint).DotProduct(Utilities.ToInventorVector(childNormal));

        driver.SetIncrement(IncrementTypeEnum.kAmountOfValueIncrement, step + " cm");

        cache.DoContactSetup(true, wrapped.childGroup, wrapped.parentGroup);

        driver.StartValue = currentLinearPosition + " cm";
        driver.EndValue = (currentLinearPosition + maxOffset) + " cm";

        // Forward
        driver.GoToStart();
        MotionLimits.DID_COLLIDE = false;
        driver.PlayForward();
        if (MotionLimits.DID_COLLIDE)
        {
            linearLimitHigh = (float)wrapped.asmJoint.LinearPosition.Value - step;
            hasUpperLimit = true;
        }

        // Reverse
        driver.EndValue = currentLinearPosition + " cm";
        driver.StartValue = (currentLinearPosition - maxOffset) + " cm";
        driver.GoToEnd();
        MotionLimits.DID_COLLIDE = false;
        driver.PlayReverse();
        if (MotionLimits.DID_COLLIDE)
        {
            linearLimitLow = (float)wrapped.asmJoint.LinearPosition.Value + step;
            hasLowerLimit = true;
        }

        driver.OnCollision -= MotionLimits.OnCollisionEvent;
        cache.DoContactSetup(false, wrapped.childGroup, wrapped.parentGroup);

        wrapped.asmJoint.LinearPosition.Value = currentLinearPosition;

        Console.WriteLine(hasLowerLimit + " low: " + linearLimitLow + "\t" + hasUpperLimit + " high: " + linearLimitHigh);

        // Stash results
        wrapped.asmJoint.HasLinearPositionStartLimit = hasLowerLimit;
        wrapped.asmJoint.HasLinearPositionEndLimit = hasUpperLimit;
        if (hasLowerLimit)
        {
            wrapped.asmJoint.LinearPositionStartLimit.Value = linearLimitLow;
        }
        if (hasUpperLimit)
        {
            wrapped.asmJoint.LinearPositionEndLimit.Value = linearLimitHigh;
        }
    }

    public static bool IsLinearJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            //Cylindrical joints with no rotaion are effectively sliding joints.
            return joint.JointType == AssemblyJointTypeEnum.kSlideJointType
                || (joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType
                && joint.HasAngularPositionLimits && joint.AngularPositionStartLimit.Value == joint.AngularPositionEndLimit.Value);
        }
        return false;
    }

    private static void GetLinearInfo(dynamic geom, out UnitVector groupANormal, out Point groupABase)
    {
        if (geom is EdgeProxy)
        {
            EdgeProxy edge = (EdgeProxy)geom;
            if (edge.GeometryType == CurveTypeEnum.kCircularArcCurve || edge.GeometryType == CurveTypeEnum.kCircleCurve)
            {
                groupABase = geom.Geometry.Center;
                groupANormal = geom.Geometry.Normal;
            }
            else if (edge.GeometryType == CurveTypeEnum.kLineSegmentCurve || edge.GeometryType == CurveTypeEnum.kLineCurve)
            {
                groupABase = edge.Geometry.MidPoint;
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
                groupABase = face.Geometry.RootPoint;
                groupANormal = face.Geometry.Normal;
            }
            else if (face.SurfaceType == SurfaceTypeEnum.kCylinderSurface)
            {
                groupABase = face.Geometry.BasePoint;
                groupANormal = face.Geometry.Normal;
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

    public LinearJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(IsLinearJoint(rigidJoint)))
            throw new Exception("Not a linear joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        UnitVector groupANormal;
        UnitVector groupBNormal;
        Point groupABase;
        Point groupBBase;

        GetLinearInfo(wrapped.asmJoint.AlignmentOne, out groupANormal, out groupABase);
        GetLinearInfo(wrapped.asmJoint.AlignmentTwo, out groupBNormal, out groupBBase);

        if (groupABase == null && groupBBase != null)
        {
            groupANormal = groupBNormal;
        }
        else if (groupBBase == null && groupABase != null)
        {
            groupBNormal = groupANormal;
        }
        else if (groupABase == null && groupBBase == null)
        {
            throw new Exception("Both axes for linear movement are null.");
        }

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

        currentLinearPosition = !((wrapped.asmJoint.LinearPosition == null)) ? ((float)wrapped.asmJoint.LinearPosition.Value) : 0;
        if (hasUpperLimit = wrapped.asmJoint.HasLinearPositionEndLimit)
        {
            linearLimitHigh = (float)wrapped.asmJoint.LinearPositionEndLimit.Value;
        }
        if (hasLowerLimit = wrapped.asmJoint.HasLinearPositionStartLimit)
        {
            linearLimitLow = (float)wrapped.asmJoint.LinearPositionStartLimit.Value;
        }
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " translates along " + wrapped.parentGroup;
    }
}