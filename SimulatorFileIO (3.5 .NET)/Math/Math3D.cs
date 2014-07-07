using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

public class BXDVector3
{
    public double x, y, z;
    public BXDVector3() { }
    public BXDVector3(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
    public String toString()
    {
        return "[" + x + "," + y + "," + z + "]";
    }
}
