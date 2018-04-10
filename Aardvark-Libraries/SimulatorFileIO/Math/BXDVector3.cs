using System;

public class BXDVector3 : BinaryRWObject
{
    /// <summary>
    /// If two floating point values have an absolute difference less than this they are considered the same.
    /// </summary>
    private const float EPSILON = 1.0E-6F;

    /// <summary>
    /// The x, y, and z values for the BXDVector3.
    /// </summary>
    public float x, y, z;

    /// <summary>
    /// Initializes a new instance of the BXDVector3 class.
    /// </summary>
    public BXDVector3()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BXDVector3 class from the given x, y, and z values.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public BXDVector3(double x, double y, double z)
    {
        this.x = (float) x;
        this.y = (float) y;
        this.z = (float) z;
    }

    /// <summary>
    /// Initializes a new instance of the BXDVector3 class from the given x, y, and z values.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public BXDVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }   
    
    /// <summary>
    /// Converts the x, y, and z values to a string.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Subtracts the given vector from this vector and returns this object.  (For method chaining)
    /// </summary>
    /// <param name="f">The vector to subtract</param>
    /// <returns>This vector.</returns>
    public BXDVector3 Subtract(BXDVector3 f)
    {
        x -= f.x;
        y -= f.y;
        z -= f.z;
        return this;
    }

    /// <summary>
    /// Gets the magnitude of this vector.
    /// </summary>
    /// <returns>The magnitude of this vector</returns>
    public float Magnitude()
    {
        return (float) Math.Sqrt(x * x + y * y + z * z);
    }

    /// <summary>
    /// Computes the cross product of two vectors.  (lhs x rhs)
    /// </summary>
    /// <param name="lhs">The left hand element</param>
    /// <param name="rhs">The right hand element</param>
    /// <returns>(lhs x rhs)</returns>
    public static BXDVector3 CrossProduct(BXDVector3 lhs, BXDVector3 rhs)
    {
        return new BXDVector3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
    }

    /// <summary>
    /// Computes the dot product of two vectors.
    /// </summary>
    /// <param name="a">One vector</param>
    /// <param name="b">Another vector</param>
    /// <returns>(a · b)</returns>
    public static float DotProduct(BXDVector3 a, BXDVector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public void WriteBinaryData(System.IO.BinaryWriter w)
    {
        w.Write(x);
        w.Write(y);
        w.Write(z);
    }

    public void ReadBinaryData(System.IO.BinaryReader r)
    {
        x = r.ReadSingle();
        y = r.ReadSingle();
        z = r.ReadSingle();
    }

    /// <summary>
    /// Creates an identical copy of this vector.
    /// </summary>
    /// <returns>The copy</returns>
    public BXDVector3 Copy()
    {
        return new BXDVector3(x, y, z);
    }

    public override bool Equals(object obj)
    {
        if (obj is BXDVector3)
        {
            BXDVector3 v = (BXDVector3) obj;
            return Math.Abs(v.x - x) < EPSILON && Math.Abs(v.y - y) < EPSILON && Math.Abs(v.z - z) < EPSILON;
        }
        return false;
    }

    public override int GetHashCode()
    {
        int x = (int) (100 * this.x);
        int y = (int) (100 * this.y);
        int z = (int) (100 * this.z);
        const int p1 = 73856093;
        const int p2 = 19349663;
        const int p3 = 83492791;
        return (x * p1) ^ (y * p2) ^ (z * p3);
    }
}
