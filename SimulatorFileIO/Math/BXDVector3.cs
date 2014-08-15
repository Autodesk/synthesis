using System;

public class BXDVector3 : RWObject
{
    private const float EPSILON = 1.0E-6F;

    public float x, y, z;
    public BXDVector3()
    {
    }
    public BXDVector3(double x, double y, double z)
    {
        this.x = (float) x;
        this.y = (float) y;
        this.z = (float) z;
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

    /// <summary>
    /// Different from the other operations, as this does not modify one of the given vectors.  It creates a new vector to store the difference.
    /// </summary>
    /// <param name="f">The vector to subtract by.</param>
    /// <returns>A newly created vector that is the difference.</returns>
    public BXDVector3 Subtract(BXDVector3 f)
    {
        return new BXDVector3(this.x - f.x, this.y - f.y, this.z - f.z);
    }

    public float GetMagnitude()
    {
        return (float)Math.Sqrt((double)(Math.Pow(this.x, 2) + Math.Pow(this.y, 2) + Math.Pow(this.z, 2)));
    }

    public void WriteData(System.IO.BinaryWriter w)
    {
        w.Write(x);
        w.Write(y);
        w.Write(z);
    }

    public void ReadData(System.IO.BinaryReader r)
    {
        x = r.ReadSingle();
        y = r.ReadSingle();
        z = r.ReadSingle();
    }

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
