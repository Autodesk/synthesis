using Inventor;

public class Utilities
{
    public static Vector ToInventorVector(BXDVector3 v)
    {
        return Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(v.x, v.y, v.z);
    }

    public static BXDVector3 ToBXDVector(object pO)
    {
        if (pO is Vector)
        {
            Vector p = (Vector)pO;
            return new BXDVector3(p.X, p.Y, p.Z);
        }
        else if (pO is UnitVector)
        {
            UnitVector p = (UnitVector)pO;
            return new BXDVector3(p.X, p.Y, p.Z);
        }
        else if (pO is Point)
        {
            Point p = (Point)pO;
            return new BXDVector3(p.X, p.Y, p.Z);
        }
        return new BXDVector3();
    }

    public static string VectorToString(object pO)
    {
        if (pO is Vector)
        {
            Vector p = (Vector)pO;
            return (p.X + "," + p.Y + "," + p.Z);
        }
        else if (pO is UnitVector)
        {
            UnitVector p = (UnitVector)pO;
            return (p.X + "," + p.Y + "," + p.Z);
        }
        else if (pO is Point)
        {
            Point p = (Point)pO;
            return (p.X + "," + p.Y + "," + p.Z);
        }
        return "";
    }
}
