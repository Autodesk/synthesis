using System;
using Inventor;

public class SkeletalJoint
{
    public CustomRigidGroup childGroup;
    public CustomRigidGroup parentGroup;
    public CustomRigidJoint rigidJoint;
    public AssemblyJointDefinition asmJoint;
    public AssemblyJoint asmJointOccurrence;
    public bool childIsTheOne;

   // public bool childIsTheOne;


    public SkeletalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (rigidJoint.joints.Count != 1)
            throw new Exception("Not a proper joint");

        asmJoint = rigidJoint.joints[0].Definition;
        asmJointOccurrence = rigidJoint.joints[0];
        childGroup = null;
        parentGroup = parent;
        this.rigidJoint = rigidJoint;
        if (rigidJoint.groupOne.Equals(parent))
        {
            childGroup = rigidJoint.groupTwo;
        }
        else if (rigidJoint.groupTwo.Equals(parent))
        {
            childGroup = rigidJoint.groupOne;
        }
        else
        {
            throw new Exception("Couldn't match parent group");
        }
        if (childGroup == null)
            throw new Exception("Not a proper joint: No child joint found");

        /*childIsTheOne = childGroup.Contains(asmJointOccurrence.AffectedOccurrenceOne);
        if (!childIsTheOne && !childGroup.Contains(asmJointOccurrence.AffectedOccurrenceTwo))
        {
            throw new Exception("Expected child not found inside assembly joint.");
        }*/
    }

    public CustomRigidGroup GetChild()
    {
        return childGroup;
    }

    public CustomRigidGroup GetParent()
    {
        return parentGroup;
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

public static class SkeletalJointType_Extensions
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
