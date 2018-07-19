using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class BulletExtensions
{
    /// <summary>
    /// Serializes this Matrix into an array of floats.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static float[] Serialize(this BulletSharp.Math.Matrix mat)
    {
        return new float[]
        {
            mat.Origin.X,
            mat.Origin.Y,
            mat.Origin.Z,
            mat.Orientation.X,
            mat.Orientation.Y,
            mat.Orientation.Z,
            mat.Orientation.W
        };
    }

    public static BulletSharp.Math.Matrix DeserializeTransform(float[] transform)
    {
        if (transform.Length != 7)
            return BulletSharp.Math.Matrix.Identity;

        return new BulletSharp.Math.Matrix
        {
            Origin = new BulletSharp.Math.Vector3(transform[0], transform[1], transform[2]),
            Orientation = new BulletSharp.Math.Quaternion(transform[3], transform[4], transform[5], transform[6])
        };
    }
}
