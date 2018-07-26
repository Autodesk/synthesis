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
        float maxOffset = (float) mover.MinPoint.VectorTo(mover.MaxPoint).DotProduct(Utilities.ToInventorVector(axis));

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
            linearLimitHigh = (float) wrapped.asmJoint.LinearPosition.Value - step;
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
            linearLimitLow = (float) wrapped.asmJoint.LinearPosition.Value + step;
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
        // kTranslationalJoint
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

    public LinearJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        Console.WriteLine(rigidJoint);
        if (!(IsLinearJoint(rigidJoint)))
            throw new Exception("Not a linear joint");
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

        if ((hasUpperLimit = wrapped.asmJoint.HasLinearPositionEndLimit) && (hasLowerLimit = wrapped.asmJoint.HasLinearPositionStartLimit))
        {
            linearLimitHigh = (float)wrapped.asmJoint.LinearPositionEndLimit.Value;
            linearLimitLow = (float)wrapped.asmJoint.LinearPositionStartLimit.Value;
        }
        else
        {
            throw new Exception("Joints with linear motion need two limits.");
        }
        currentLinearPosition = !((wrapped.asmJoint.LinearPosition == null)) ? ((float)wrapped.asmJoint.LinearPosition.Value) : 0;

    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " translates along " + wrapped.parentGroup;
    }
}