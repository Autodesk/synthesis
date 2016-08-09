using UnityEngine;

public struct MathfExt
{
    public const float DEG_TO_RADIANS = Mathf.PI / 180f;
    public const float RADIANS_TO_DEG = 1f / DEG_TO_RADIANS;

    public static float ToRadians(float deg)
    {
        return deg * DEG_TO_RADIANS;
    }

    public static float ToDegrees(float rad)
    {
        return rad * RADIANS_TO_DEG;
    }
}