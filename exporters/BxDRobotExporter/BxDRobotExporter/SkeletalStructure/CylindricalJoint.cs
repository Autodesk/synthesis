using System;
using BxDRobotExporter.RigidAnalyzer;
using Inventor;

namespace BxDRobotExporter.SkeletalStructure
{
    public class CylindricalJoint : CylindricalJoint_Base, INventorSkeletalJoint
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
            driver.CollisionDetection = true;
            driver.OnCollision += MotionLimits.OnCollisionEvent;
            driver.FrameRate = 1000;

            cache.DoContactSetup(true, wrapped.ChildGroup, wrapped.ParentGroup);

            {   // Rotational motion
                driver.DriveType = DriveTypeEnum.kDriveAngularPositionType;
                wrapped.AsmJoint.LinearPosition.Value = currentLinearPosition;

                float step = 0.05f; // rad
                driver.SetIncrement(IncrementTypeEnum.kAmountOfValueIncrement, step + " rad");

                driver.StartValue = currentAngularPosition + " rad";
                driver.EndValue = (currentAngularPosition + 6.5) + " rad";

                // Forward
                driver.GoToStart();
                MotionLimits.DidCollide = false;
                driver.PlayForward();
                if (MotionLimits.DidCollide)
                {
                    angularLimitHigh = (float)wrapped.AsmJoint.AngularPosition.Value - step;
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
                    angularLimitLow = (float)wrapped.AsmJoint.AngularPosition.Value + step;
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

                wrapped.AsmJoint.AngularPosition.Value = currentAngularPosition;

                Console.WriteLine(hasAngularLimit + "; high: " + angularLimitHigh + "; low: " + angularLimitLow);
            }

            {   // Linear motion
                driver.DriveType = DriveTypeEnum.kDriveLinearPositionType;
                wrapped.AsmJoint.AngularPosition.Value = currentAngularPosition;

                float step = 0.1f; // cm
                Box mover = (wrapped.ChildIsTheOne ? wrapped.AsmJointOccurrence.OccurrenceOne : wrapped.AsmJointOccurrence.OccurrenceTwo).RangeBox;
                float maxOffset = (float)mover.MinPoint.VectorTo(mover.MaxPoint).DotProduct(InventorDocumentIoUtils.ToInventorVector(axis));
                Console.WriteLine("Max linear offset: " + maxOffset);

                driver.SetIncrement(IncrementTypeEnum.kAmountOfValueIncrement, step + " cm");

                driver.StartValue = currentLinearPosition + " cm";
                driver.EndValue = (currentLinearPosition + maxOffset) + " cm";

                // Forward
                driver.GoToStart();
                MotionLimits.DidCollide = false;
                driver.PlayForward();
                if (MotionLimits.DidCollide)
                {
                    linearLimitEnd = (float)wrapped.AsmJoint.LinearPosition.Value - step;
                    hasLinearEndLimit = true;
                }

                // Reverse
                driver.EndValue = currentLinearPosition + " cm";
                driver.StartValue = (currentLinearPosition - maxOffset) + " cm";
                driver.GoToEnd();
                MotionLimits.DidCollide = false;
                driver.PlayReverse();
                if (MotionLimits.DidCollide)
                {
                    linearLimitStart = (float)wrapped.AsmJoint.LinearPosition.Value + step;
                    hasLinearStartLimit = true;
                }

                wrapped.AsmJoint.LinearPosition.Value = currentLinearPosition;
                Console.WriteLine(hasLinearStartLimit + " low: " + linearLimitStart + "\t" + hasLinearEndLimit + " high: " + linearLimitEnd);
            }

            driver.OnCollision -= MotionLimits.OnCollisionEvent;
            cache.DoContactSetup(false, wrapped.ChildGroup, wrapped.ParentGroup);

            // Stash results
            wrapped.AsmJoint.HasLinearPositionStartLimit = hasLinearStartLimit;
            wrapped.AsmJoint.HasLinearPositionEndLimit = hasLinearEndLimit;
            if (hasLinearStartLimit)
            {
                wrapped.AsmJoint.LinearPositionStartLimit.Value = linearLimitStart;
            }
            if (hasLinearEndLimit)
            {
                wrapped.AsmJoint.LinearPositionEndLimit.Value = linearLimitEnd;
            }

            wrapped.AsmJoint.HasAngularPositionLimits = hasAngularLimit;
            if (hasAngularLimit)
            {
                wrapped.AsmJoint.AngularPositionStartLimit.Value = angularLimitLow;
                wrapped.AsmJoint.AngularPositionEndLimit.Value = angularLimitHigh;
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

            currentLinearPosition = (wrapped.AsmJoint.LinearPosition != null) ? (float)wrapped.AsmJoint.LinearPosition.Value : 0;

            hasAngularLimit = wrapped.AsmJoint.HasAngularPositionLimits;
            if (hasAngularLimit)
            {
                angularLimitLow = (float)wrapped.AsmJoint.AngularPositionStartLimit.Value;
                angularLimitHigh = (float)wrapped.AsmJoint.AngularPositionEndLimit.Value;
            }
            currentAngularPosition = (wrapped.AsmJoint.AngularPosition != null) ? (float)wrapped.AsmJoint.AngularPosition.Value : 0;

            hasLinearStartLimit = wrapped.AsmJoint.HasLinearPositionStartLimit;
            hasLinearEndLimit = wrapped.AsmJoint.HasLinearPositionEndLimit;

            if (hasLinearStartLimit && hasLinearEndLimit)
            {
                linearLimitStart = (float)wrapped.AsmJoint.LinearPositionStartLimit.Value;
                linearLimitEnd = (float)wrapped.AsmJoint.LinearPositionEndLimit.Value;

            }
            else
            {
                throw new Exception("Joints with linear motion need two limits.");
            }
            wrapped.AsmJoint.LinearPosition = wrapped.AsmJoint.LinearPosition;
        }

        public static bool IsCylindricalJoint(CustomRigidJoint jointI)
        {
            // kMateLineLineJoint
            if (jointI.Joints.Count == 1)
            {
                AssemblyJointDefinition joint = jointI.Joints[0].Definition;
                return joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType;
            }
            return false;
        }

        public CylindricalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
        {
            if (!(IsCylindricalJoint(rigidJoint)))
                throw new Exception("Not a Cylindrical joint");
            wrapped = new SkeletalJoint(parent, rigidJoint);

            ReloadInventorJoint();
        }

        protected override string ToString_Internal()
        {
            return wrapped.ChildGroup + " rotates about and slides along " + wrapped.ParentGroup;
        }
    }
}