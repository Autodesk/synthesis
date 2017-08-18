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

    public static Vector3 AsV3(this BXDVector3 v)
    {
        return new Vector3(v.x * 0.01f, v.y * 0.01f, v.z * 0.01f);
    }

    public static BXDVector3 AsBV3(this Vector3 v)
    {
        return new BXDVector3(v.x / 0.01f, v.y / 0.01f, v.z / 0.01f);
    }

    public static Material AsMaterial(this BXDAMesh.BXDASurface surf, bool emissive = false)
    {
        uint val = surf.hasColor ? surf.color : 0xFFFFFFFF;
        Color color = new Color32((byte)(val & 0xFF), (byte)((val >> 8) & 0xFF), (byte)((val >> 16) & 0xFF), (byte)((val >> 24) & 0xFF));
        if (surf.transparency != 0)
            color.a = surf.transparency;
        else if (surf.translucency != 0)
            color.a = surf.translucency;
        if (color.a == 0)   // No perfectly transparent things plz.
            color.a = 1;
        Material result = new Material(Shader.Find(emissive ? "Standard" : (color.a != 1 ? "Transparent/" : "") + (surf.specular > 0 ? "Specular" : "Diffuse")));
        result.SetColor("_Color", color);
        if (surf.specular > 0)
        {
            result.SetFloat("_Shininess", surf.specular);
            result.SetColor("_SpecColor", color);
        }

        if (emissive)
        {
            result.EnableKeyword("_EMISSION");
            result.SetColor("_EmissionColor", Color.black);
        }

        return result;
    }
}