using UnityEngine;

public static class BXDExtensions
{
    public static T GetDriverMeta<T>(this RigidNode_Base node) where T : JointDriverMeta
    {
        return node != null && node.GetSkeletalJoint() != null && node.GetSkeletalJoint().cDriver != null ? node.GetSkeletalJoint().cDriver.GetInfo<T>() : null;
    }

    public static bool HasDriverMeta<T>(this RigidNode_Base node) where T : JointDriverMeta
    {
        return node.GetDriverMeta<T>() != null;
    }

    public static Vector3 AsV3(this BXDVector3 v) {
        return new Vector3(v.x * 0.01f, v.y * 0.01f, v.z * 0.01f);
    }

    public static BXDVector3 AsBV3(this Vector3 v)
    {
        return new BXDVector3(v.x / 0.01f, v.y / 0.01f, v.z / 0.01f);
    }
}