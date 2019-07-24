using System;
using BxDRobotExporter.RigidAnalyzer;
using Inventor;

namespace BxDRobotExporter.SkeletalStructure
{
    public class LinearJoint : LinearJoint_Base, INventorSkeletalJoint
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
            driver.DriveType = DriveTypeEnum.kDriveLinearPositionType;
            driver.CollisionDetection = true;
            driver.OnCollision += MotionLimits.OnCollisionEvent;
            driver.FrameRate = 1;
            float step = 0.1f;
            Box mover = (wrapped.ChildIsTheOne ? wrapped.AsmJointOccurrence.OccurrenceOne : wrapped.AsmJointOccurrence.OccurrenceTwo).RangeBox;
            float maxOffset = (float) mover.MinPoint.VectorTo(mover.MaxPoint).DotProduct(InventorDocumentIoUtils.ToInventorVector(axis));

            driver.SetIncrement(IncrementTypeEnum.kAmountOfValueIncrement, step + " cm");

            cache.DoContactSetup(true, wrapped.ChildGroup, wrapped.ParentGroup);

            driver.StartValue = currentLinearPosition + " cm";
            driver.EndValue = (currentLinearPosition + maxOffset) + " cm";

            // Forward
            driver.GoToStart();
            MotionLimits.DidCollide = false;
            driver.PlayForward();
            if (MotionLimits.DidCollide)
            {
                linearLimitHigh = (float) wrapped.AsmJoint.LinearPosition.Value - step;
                hasUpperLimit = true;
            }

            // Reverse
            driver.EndValue = currentLinearPosition + " cm";
            driver.StartValue = (currentLinearPosition - maxOffset) + " cm";
            driver.GoToEnd();
            MotionLimits.DidCollide = false;
            driver.PlayReverse();
            if (MotionLimits.DidCollide)
            {
                linearLimitLow = (float) wrapped.AsmJoint.LinearPosition.Value + step;
                hasLowerLimit = true;
            }

            driver.OnCollision -= MotionLimits.OnCollisionEvent;
            cache.DoContactSetup(false, wrapped.ChildGroup, wrapped.ParentGroup);

            wrapped.AsmJoint.LinearPosition.Value = currentLinearPosition;

            Console.WriteLine(hasLowerLimit + " low: " + linearLimitLow + "\t" + hasUpperLimit + " high: " + linearLimitHigh);

            // Stash results
            wrapped.AsmJoint.HasLinearPositionStartLimit = hasLowerLimit;
            wrapped.AsmJoint.HasLinearPositionEndLimit = hasUpperLimit;
            if (hasLowerLimit)
            {
                wrapped.AsmJoint.LinearPositionStartLimit.Value = linearLimitLow;
            }
            if (hasUpperLimit)
            {
                wrapped.AsmJoint.LinearPositionEndLimit.Value = linearLimitHigh;
            }
        }

        public void ReloadInventorJoint()
        {
            if (wrapped.ChildGroup == wrapped.RigidJoint.GroupOne)
            {
                axis = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomTwo.Direction);
                basePoint = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomTwo.RootPoint);
            }
            else
            {
                axis = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomOne.Direction);
                basePoint = InventorDocumentIoUtils.ToBxdVector(wrapped.RigidJoint.GeomOne.RootPoint);
            }

            if ((hasUpperLimit = wrapped.AsmJoint.HasLinearPositionEndLimit) && (hasLowerLimit = wrapped.AsmJoint.HasLinearPositionStartLimit))
            {
                linearLimitHigh = (float)wrapped.AsmJoint.LinearPositionEndLimit.Value;
                linearLimitLow = (float)wrapped.AsmJoint.LinearPositionStartLimit.Value;
            }
            else
            {
                throw new Exception("Joints with linear motion need two limits.");
            }
            currentLinearPosition = (wrapped.AsmJoint.LinearPosition != null) ? ((float)wrapped.AsmJoint.LinearPosition.Value) : 0;
        }

        public static bool IsLinearJoint(CustomRigidJoint jointI)
        {
            // kTranslationalJoint
            if (jointI.Joints.Count == 1)
            {
                AssemblyJointDefinition joint = jointI.Joints[0].Definition;
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

            ReloadInventorJoint();
        }

        protected override string ToString_Internal()
        {
            return wrapped.ChildGroup + " translates along " + wrapped.ParentGroup;
        }
    }
}