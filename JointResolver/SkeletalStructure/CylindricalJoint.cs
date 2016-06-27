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
            float maxOffset = (float)mover.MinPoint.VectorTo(mover.MaxPoint).DotProduct(Utilities.ToInventorVector(axis));
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
        if (hasLinearStartLimit)
        {
            wrapped.asmJoint.LinearPositionStartLimit.Value = linearLimitStart;
        }
        if (hasLinearEndLimit)
        {
            wrapped.asmJoint.LinearPositionEndLimit.Value = linearLimitEnd;
        }

        wrapped.asmJoint.HasAngularPositionLimits = hasAngularLimit;
        if (hasAngularLimit)
        {
            wrapped.asmJoint.AngularPositionStartLimit.Value = angularLimitLow;
            wrapped.asmJoint.AngularPositionEndLimit.Value = angularLimitHigh;
        }
    }

    public static bool IsCylindricalJoint(CustomRigidJoint jointI)
    {
        // kMateLineLineJoint
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            return joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType;
        }
        return false;
    }

    public CylindricalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(IsCylindricalJoint(rigidJoint)))
            throw new Exception("Not a Cylindrical joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        if (wrapped.childGroup == rigidJoint.groupOne)
        {
            axis = Utilities.ToBXDVector(rigidJoint.geomTwo.Direction);
            basePoint = Utilities.ToBXDVector(rigidJoint.geomTwo.RootPoint);
        }
        else
        {
            axis = Utilities.ToBXDVector(rigidJoint.geomOne.Direction);
            basePoint = Utilities.ToBXDVector(rigidJoint.geomOne.RootPoint);
        }

        currentLinearPosition = wrapped.asmJoint.LinearPosition != null ? (float)wrapped.asmJoint.LinearPosition.Value : 0;

        hasAngularLimit = wrapped.asmJoint.HasAngularPositionLimits;
        if (hasAngularLimit)
        {
            angularLimitLow = (float)wrapped.asmJoint.AngularPositionStartLimit.Value;
            angularLimitHigh = (float)wrapped.asmJoint.AngularPositionEndLimit.Value;
        }
        currentAngularPosition = wrapped.asmJoint.AngularPosition != null ? (float)wrapped.asmJoint.AngularPosition.Value : 0;

        hasLinearStartLimit = wrapped.asmJoint.HasLinearPositionStartLimit;
        hasLinearEndLimit = wrapped.asmJoint.HasLinearPositionEndLimit;

        if (hasLinearStartLimit && hasLinearEndLimit)
        {
            linearLimitStart = (float)wrapped.asmJoint.LinearPositionStartLimit.Value;
            linearLimitEnd = (float)wrapped.asmJoint.LinearPositionEndLimit.Value;

        }
        else
        {
            throw new Exception("Joints with linear motion need two limits.");
        }
        wrapped.asmJoint.LinearPosition = wrapped.asmJoint.LinearPosition;
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " rotates about and slides along " + wrapped.parentGroup;
    }
}