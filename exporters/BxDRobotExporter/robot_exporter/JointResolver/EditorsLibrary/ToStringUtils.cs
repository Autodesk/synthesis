using System;

public static class ToStringUtils
{
    public static string SensorCountString(SkeletalJoint_Base joint)
    {
        return joint.attachedSensors.Count.ToString();
    }

    public static string WheelTypeString(SkeletalJoint_Base joint)
    {
        WheelDriverMeta wheelData = null;
        if (joint.cDriver != null)
        {
            wheelData = joint.cDriver.GetInfo<WheelDriverMeta>();
        }
        return wheelData!=null ? wheelData.GetTypeString() : "No Wheel";
    }

    public static string DriverString(SkeletalJoint_Base joint)
    {
        return joint.cDriver != null ? joint.cDriver.ToString() : "No Driver";
    }

    public static string NodeNameString(RigidNode_Base node)
    {
        return Utilities.CapitalizeFirstLetter(node.ModelFileName.Replace('_', ' ').Replace(".bxda", ""));
    }

    public static string ParentNameString(RigidNode_Base node)
    {
        return Utilities.CapitalizeFirstLetter(node.GetParent().ModelFileName.Replace('_', ' ').Replace(".bxda", ""));
    }

    public static string JointTypeString(SkeletalJoint_Base joint)
    {
        return Utilities.CapitalizeFirstLetter(Enum.GetName(typeof(SkeletalJointType),joint.GetJointType()), true);
    }
}