using System;

public class BXDVector3
{
    public float x, y, z;
    public BXDVector3()
    {
    }
    public BXDVector3(double x, double y, double z)
    {
        this.x = (float)x;
        this.y = (float)y;
        this.z = (float)z;
    }
    public BXDVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public override String ToString()
    {
        return "[" + x + "," + y + "," + z + "]";
    }

    /// <summary>
    /// Multiplies this vector by the given scalar and returns this object (for method chaining)
    /// </summary>
    /// <param name="f">The scalar to multiply by</param>
    /// <returns>This vector.</returns>
    public BXDVector3 Multiply(float f)
    {
        x *= f;
        y *= f;
        z *= f;
        return this;
    }

    /// <summary>
    /// Adds the given vector to this vector and returns this object.  (For method chaining)
    /// </summary>
    /// <param name="f">The vector to add</param>
    /// <returns>This vector.</returns>
    public BXDVector3 Add(BXDVector3 f)
    {
        x += f.x;
        y += f.y;
        z += f.z;
        return this;
    }
}
