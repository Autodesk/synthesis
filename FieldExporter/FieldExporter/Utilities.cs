using FieldExporter;
using Inventor;
using System;

public class Utilities
{
    /// <summary>
    /// Converts a BXDVector3 to an Inventor.Vector.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector ToInventorVector(BXDVector3 v)
    {
        return Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(v.x, v.y, v.z);
    }

    /// <summary>
    /// Converts the given object to a BXDVector3.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static BXDVector3 ToBXDVector(dynamic p)
    {
        return new BXDVector3(p.X, p.Y, p.Z);
    }

    /// <summary>
    /// Get a BXDQuaternion from the given Inventor.Matrix.
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    public static BXDQuaternion QuaternionFromMatrix(Matrix m)
    {
        BXDQuaternion q = new BXDQuaternion();

        double tr = m.Cell[1, 1] + m.Cell[2, 2] + m.Cell[3, 3];
        double s;

        if (tr > 0)
        {
            s = Math.Sqrt(tr + 1.0) * 2;
            q.W = (float)(0.25 * s);
            q.X = (float)((m.Cell[3, 2] - m.Cell[2, 3]) / s);
            q.Y = (float)((m.Cell[1, 3] - m.Cell[3, 1]) / s);
            q.Z = (float)((m.Cell[2, 1] - m.Cell[1, 2]) / s);
        }
        else if ((m.Cell[1, 1] > m.Cell[2, 2]) && (m.Cell[1, 1] > m.Cell[3, 3]))
        {
            s = Math.Sqrt(1.0 + m.Cell[1, 1] - m.Cell[2, 2] - m.Cell[3, 3]) * 2;
            q.W = (float)((m.Cell[3, 2] - m.Cell[2, 3]) / s);
            q.X = (float)(0.25 * s);
            q.Y = (float)((m.Cell[1, 2] + m.Cell[2, 1]) / s);
            q.Z = (float)((m.Cell[1, 3] + m.Cell[3, 1]) / s);
        }
        else if (m.Cell[2, 2] > m.Cell[3, 3])
        {
            s = Math.Sqrt(1.0 + m.Cell[2, 2] - m.Cell[1, 1] - m.Cell[3, 3]) * 2;
            q.W = (float)((m.Cell[1, 3] - m.Cell[3, 1]) / s);
            q.X = (float)((m.Cell[1, 2] + m.Cell[2, 1]) / s);
            q.Y = (float)(0.25 * s);
            q.Z = (float)((m.Cell[2, 3] + m.Cell[3, 2]) / s);
        }
        else
        {
            s = Math.Sqrt(1.0 + m.Cell[3, 3] - m.Cell[1, 1] - m.Cell[2, 2]) * 2;
            q.W = (float)((m.Cell[2, 1] - m.Cell[1, 2]) / s);
            q.X = (float)((m.Cell[1, 3] + m.Cell[3, 1]) / s);
            q.Y = (float)((m.Cell[2, 3] + m.Cell[3, 2]) / s);
            q.Z = (float)(0.25 * s);
        }

        return q;
    }
    
    /// <summary>
    /// Converts the given vector to a string.
    /// </summary>
    /// <param name="pO"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets the volume from the given Inventor.Box.
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public static double BoxVolume(Box b)
    {
        double dx = b.MaxPoint.X - b.MinPoint.X;
        double dy = b.MaxPoint.Y - b.MinPoint.Y;
        double dz = b.MaxPoint.Z - b.MinPoint.Z;
        return dx * dy * dz;
    }
}
