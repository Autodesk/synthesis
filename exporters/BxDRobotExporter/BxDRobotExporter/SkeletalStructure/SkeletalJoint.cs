using System;
using BxDRobotExporter.RigidAnalyzer;
using Inventor;

namespace BxDRobotExporter.SkeletalStructure
{
    public class SkeletalJoint
    {
        public CustomRigidGroup ChildGroup;
        public CustomRigidGroup ParentGroup;
        public CustomRigidJoint RigidJoint;
        public AssemblyJointDefinition AsmJoint;
        public AssemblyJoint AsmJointOccurrence;
        public bool ChildIsTheOne;

        // public bool childIsTheOne;


        public SkeletalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
        {
            if (rigidJoint.Joints.Count != 1)
                throw new Exception("Not a proper joint");

            AsmJoint = rigidJoint.Joints[0].Definition;
            AsmJointOccurrence = rigidJoint.Joints[0];
            ChildGroup = null;
            ParentGroup = parent;
            this.RigidJoint = rigidJoint;
            if (rigidJoint.GroupOne.Equals(parent))
            {
                ChildGroup = rigidJoint.GroupTwo;
            }
            else if (rigidJoint.GroupTwo.Equals(parent))
            {
                ChildGroup = rigidJoint.GroupOne;
            }
            else
            {
                throw new Exception("Couldn't match parent group");
            }
            if (ChildGroup == null)
                throw new Exception("Not a proper joint: No child joint found");

            /*childIsTheOne = childGroup.Contains(asmJointOccurrence.AffectedOccurrenceOne);
        if (!childIsTheOne && !childGroup.Contains(asmJointOccurrence.AffectedOccurrenceTwo))
        {
            throw new Exception("Expected child not found inside assembly joint.");
        }*/
        }

        public CustomRigidGroup GetChild()
        {
            return ChildGroup;
        }

        public CustomRigidGroup GetParent()
        {
            return ParentGroup;
        }

        public static SkeletalJoint_Base Create(CustomRigidJoint rigidJoint, CustomRigidGroup parent)
        {
            if (RotationalJoint.IsRotationalJoint(rigidJoint))
                return new RotationalJoint(parent, rigidJoint);
            if (LinearJoint.IsLinearJoint(rigidJoint))
                return new LinearJoint(parent, rigidJoint);
            if (CylindricalJoint.IsCylindricalJoint(rigidJoint))
                return new CylindricalJoint(parent, rigidJoint);
            if (PlanarJoint.IsPlanarJoint(rigidJoint))
                return new PlanarJoint(parent, rigidJoint);
            if (BallJoint.IsBallJoint(rigidJoint))
                return new BallJoint(parent, rigidJoint);
            return null;
        }
    }

    public static class SkeletalJointTypeExtensions
    {

        public static SkeletalJointType ToSkeletalJointType(this AssemblyJointTypeEnum assemblyJoint)
        {
            SkeletalJointType jointType = SkeletalJointType.DEFAULT;

            switch (assemblyJoint)
            {
                case AssemblyJointTypeEnum.kBallJointType:
                    jointType = SkeletalJointType.BALL;
                    break;
                case AssemblyJointTypeEnum.kCylindricalJointType:
                    jointType = SkeletalJointType.CYLINDRICAL;
                    break;
                case AssemblyJointTypeEnum.kPlanarJointType:
                    jointType = SkeletalJointType.PLANAR;
                    break;
                case AssemblyJointTypeEnum.kRotationalJointType:
                    jointType = SkeletalJointType.ROTATIONAL;
                    break;
                case AssemblyJointTypeEnum.kSlideJointType:
                    jointType = SkeletalJointType.LINEAR;
                    break;
            }

            return jointType;
        }

        public static AssemblyJointTypeEnum ToAssemblyJointType(this SkeletalJointType skeletalJoint)
        {
            AssemblyJointTypeEnum jointType = AssemblyJointTypeEnum.kRigidJointType;

            switch (skeletalJoint)
            {
                case SkeletalJointType.BALL:
                    jointType = AssemblyJointTypeEnum.kBallJointType;
                    break;
                case SkeletalJointType.CYLINDRICAL:
                    jointType = AssemblyJointTypeEnum.kCylindricalJointType;
                    break;
                case SkeletalJointType.LINEAR:
                    jointType = AssemblyJointTypeEnum.kSlideJointType;
                    break;
                case SkeletalJointType.PLANAR:
                    jointType = AssemblyJointTypeEnum.kPlanarJointType;
                    break;
                case SkeletalJointType.ROTATIONAL:
                    jointType = AssemblyJointTypeEnum.kRotationalJointType;
                    break;
            }

            return jointType;
        }

    }
}