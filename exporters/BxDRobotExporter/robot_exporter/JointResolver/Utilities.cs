using Inventor;
using System.IO;
using System;
using System.Diagnostics;
using System.Windows.Forms;


public class Utilities
{
    public const string SYNTHESIS_PATH = @"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Synthesis.exe";
    public static string VIEWER_PATH = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\RobotViewer\RobotViewer.exe";
    
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

    public static string CapitalizeFirstLetter(string str, bool onlyFirst = false)
    {
        if (str.Length < 2)
            return str.ToUpperInvariant();
        else if (onlyFirst)
            return str.Substring(0, 1).ToUpperInvariant() + str.Substring(1).ToLowerInvariant();
        else
            return str.Substring(0, 1).ToUpperInvariant() + str.Substring(1);
    }
}

namespace LegacyInterchange
{
    public static class LegacyEvents
    {
        public static event Action RobotModified;
        
        public static void OnRobotModified()
        {
            RobotModified?.Invoke();
        }
    }
}
