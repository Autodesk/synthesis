using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BXDQuaternion
{
    /// <summary>
    /// The X value of the BXDQuaternion.
    /// </summary>
    public float X;

    /// <summary>
    /// The Y value of the BXDQuaternion.
    /// </summary>
    public float Y;

    /// <summary>
    /// The Z value of the BXDQuaternion.
    /// </summary>
    public float Z;

    /// <summary>
    /// The W value of the BXDQuaternion.
    /// </summary>
    public float W;

    /// <summary>
    /// Initializes a new instance of the BXDQuaternion class with an identity value.
    /// </summary>
    public BXDQuaternion()
        : this(0f, 0f, 0f, 0f)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BXDQuaternion class with the values given.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="w"></param>
    public BXDQuaternion(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    /// <summary>
    /// Returns a string containing labeled x, y, z, and w values.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "(X: " + X + ", Y: " + Y + ", Z: " + Z + ", W: " + W + ")";
    }
}