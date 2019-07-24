using System;
using BxDRobotExporter.RigidAnalyzer;
using Inventor;

namespace BxDRobotExporter.SkeletalStructure
{
    public class RotationalJoint : RotationalJoint_Base, INventorSkeletalJoint
    {
        private SkeletalJoint wrapped;

        public SkeletalJoint GetWrapped()
        {
            return wrapped;
        }
        public void DetermineLimits()
        {
            MotionLimits cache = new MotionLimits();
            DriveSettings driver = wrapped.AsmJointOccurrence.DriveSettings;
            driver.DriveType = DriveTypeEnum.kDriveAngularPositionType;
            driver.CollisionDetection = true;
            driver.OnCollision += MotionLimits.OnCollisionEvent;
            driver.FrameRate = 1000;
            float step = 0.05f;
            driver.SetIncrement(IncrementTypeEnum.kAmountOfValueIncrement, step + " rad");

            cache.DoContactSetup(true, wrapped.ChildGroup, wrapped.ParentGroup);

            driver.StartValue = currentAngularPosition + " rad";
            driver.EndValue = (currentAngularPosition + 6.5) + " rad";

            // Forward
            driver.GoToStart();
            MotionLimits.DidCollide = false;
            driver.PlayForward();
            if (MotionLimits.DidCollide)
            {
                angularLimitHigh = (float) wrapped.AsmJoint.AngularPosition.Value - step;
                hasAngularLimit = true;
            }

            // Reverse
            driver.EndValue = currentAngularPosition + " rad";
            driver.StartValue = (currentAngularPosition - 6.5) + " rad";
            driver.GoToEnd();
            MotionLimits.DidCollide = false;
            driver.PlayReverse();
            if (MotionLimits.DidCollide)
            {
                angularLimitLow = (float) wrapped.AsmJoint.AngularPosition.Value + step;
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
            cache.DoContactSetup(false, wrapped.ChildGroup, wrapped.ParentGroup);

            wrapped.AsmJoint.AngularPosition.Value = currentAngularPosition;

            Console.WriteLine(hasAngularLimit + "; high: " + angularLimitHigh + "; low: " + angularLimitLow);

            // Stash results
            wrapped.AsmJoint.HasAngularPositionLimits = hasAngularLimit;
            if (hasAngularLimit)
            {
                wrapped.AsmJoint.AngularPositionStartLimit.Value = angularLimitLow;
                wrapped.AsmJoint.AngularPositionEndLimit.Value = angularLimitHigh;
            }
        }

        public void ReloadInventorJoint()
        {
            try
            {
                axis = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomOne.Normal);
                basePoint = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomOne.Center);
            }
            catch
            {
                axis = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomOne.Direction);
                basePoint = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomOne.RootPoint);
            }

            hasAngularLimit = wrapped.AsmJoint.HasAngularPositionLimits;
            if ((hasAngularLimit))
            {
                angularLimitLow = (float)wrapped.AsmJoint.AngularPositionStartLimit.Value;
                angularLimitHigh = (float)wrapped.AsmJoint.AngularPositionEndLimit.Value;
            }
            currentAngularPosition = (wrapped.AsmJoint.AngularPosition != null) ? (float)wrapped.AsmJoint.AngularPosition.Value : 0;
        }

        public static bool IsRotationalJoint(CustomRigidJoint jointI)
        {
            // RigidBodyJointType = kConcentricCircleCircleJoint
            if (jointI.JointBased && jointI.Joints.Count == 1)
            {
                AssemblyJointDefinition joint = jointI.Joints[0].Definition;
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

            ReloadInventorJoint();
        }

        protected override string ToString_Internal()
        {
            return wrapped.ChildGroup + " rotates about " + wrapped.ParentGroup;
        }
    }
}