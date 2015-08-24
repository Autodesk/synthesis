using Inventor;

public class Utilities
{
    public static Vector ToInventorVector(BXDVector3 v)
    {
        if (InventorManager.Instance == null) return null;
        return InventorManager.Instance.TransientGeometry.CreateVector(v.x, v.y, v.z);
    }

    public static BXDVector3 ToBXDVector(dynamic p)
    {
        return new BXDVector3(p.X, p.Y, p.Z);
    }

    public static string VectorToString(object pO)
    {
        if (pO is Vector)
        {
            Vector p = (Vector) pO;
            return (p.X + "," + p.Y + "," + p.Z);
        }
        else if (pO is UnitVector)
        {
            UnitVector p = (UnitVector) pO;
            return (p.X + "," + p.Y + "," + p.Z);
        }
        else if (pO is Point)
        {
            Point p = (Point) pO;
            return (p.X + "," + p.Y + "," + p.Z);
        }
        return "";
    }

    public static double BoxVolume(Box b)
    {
        double dx = b.MaxPoint.X - b.MinPoint.X;
        double dy = b.MaxPoint.Y - b.MinPoint.Y;
        double dz = b.MaxPoint.Z - b.MinPoint.Z;
        return dx * dy * dz;
    }
}
