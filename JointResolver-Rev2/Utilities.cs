using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

class Utilities
{
    public static string sanatizeFileName(string fName, char sanity = '_')
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        {
            fName = fName.Replace(c, sanity);
        }
        return fName;
    }

    public static BXDVector3 toBXDVector(object pO)
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

    public static string printVector(object pO)
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
