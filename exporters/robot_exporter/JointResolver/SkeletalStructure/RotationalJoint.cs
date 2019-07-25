using System;
using Inventor;

public class RotationalJoint : RotationalJoint_Base, InventorSkeletalJoint
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
        driver.DriveType = DriveTypeEnum.kDriveAngularPositionType;
        driver.CollisionDetection = true;
        driver.OnCollision += MotionLimits.OnCollisionEvent;
        driver.FrameRate = 1000;
        float step = 0.05f;
        driver.SetIncrement(IncrementTypeEnum.kAmountOfValueIncrement, step + " rad");

        cache.DoContactSetup(true, wrapped.childGroup, wrapped.parentGroup);

        driver.StartValue = currentAngularPosition + " rad";
        driver.EndValue = (currentAngularPosition + 6.5) + " rad";

        // Forward
        driver.GoToStart();
        MotionLimits.DID_COLLIDE = false;
        driver.PlayForward();
        if (MotionLimits.DID_COLLIDE)
        {
            angularLimitHigh = (float) wrapped.asmJoint.AngularPosition.Value - step;
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
            angularLimitLow = (float) wrapped.asmJoint.AngularPosition.Value + step;
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

        driver.OnCollision -= MotionLimits.OnCollisionEvent;
        cache.DoContactSetup(false, wrapped.childGroup, wrapped.parentGroup);

        wrapped.asmJoint.AngularPosition.Value = currentAngularPosition;

        Console.WriteLine(hasAngularLimit + "; high: " + angularLimitHigh + "; low: " + angularLimitLow);

        // Stash results
        wrapped.asmJoint.HasAngularPositionLimits = hasAngularLimit;
        if (hasAngularLimit)
        {
            wrapped.asmJoint.AngularPositionStartLimit.Value = angularLimitLow;
            wrapped.asmJoint.AngularPositionEndLimit.Value = angularLimitHigh;
        }
    }

    public static bool IsRotationalJoint(CustomRigidJoint jointI)
    {
        // RigidBodyJointType = kConcentricCircleCircleJoint
        if (jointI.jointBased && jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            //Checks if there is no linear motion allowed.
            return joint.JointType == AssemblyJointTypeEnum.kRotationalJointType
                || (joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType
                && joint.HasLinearPositionStartLimit && joint.HasLinearPositionEndLimit
                && joint.LinearPositionStartLimit.Value == joint.LinearPositionEndLimit.Value);
        }
        return false;
    }

    //private FindPartMatches

    public RotationalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(IsRotationalJoint(rigidJoint)))
            throw new Exception("Not a rotational joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        try
        {
            axis = Utilities.ToBXDVector(rigidJoint.geomOne.Normal);
            basePoint = Utilities.ToBXDVector(rigidJoint.geomOne.Center);
        }
        catch
        {
            axis = Utilities.ToBXDVector(rigidJoint.geomOne.Direction);
            basePoint = Utilities.ToBXDVector(rigidJoint.geomOne.RootPoint);
        }


        hasAngularLimit = wrapped.asmJoint.HasAngularPositionLimits;
        if ((hasAngularLimit))
        {
            angularLimitLow = (float) wrapped.asmJoint.AngularPositionStartLimit.Value;
            angularLimitHigh = (float) wrapped.asmJoint.AngularPositionEndLimit.Value;
        }
        currentAngularPosition = !((wrapped.asmJoint.AngularPosition == null)) ? (float)wrapped.asmJoint.AngularPosition.Value : 0;
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " rotates about " + wrapped.parentGroup;
    }
}