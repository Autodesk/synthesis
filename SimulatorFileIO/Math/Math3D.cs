using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BXDVector3
{
    public float x, y, z;
    public BXDVector3() { }
    public BXDVector3(double x, double y, double z) { this.x = (float)x; this.y = (float)y; this.z = (float)z; }
    public BXDVector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    public override String ToString()
    {
        return "[" + x + "," + y + "," + z + "]";
    }

    public BXDVector3 Multiply(float f)
    {
        x *= f;
        y *= f;
        z *= f;
        return this;
    }

    public BXDVector3 Add(BXDVector3 f)
    {
        x += f.x;
        y += f.y;
        z += f.z;
        return this;
    }
}
