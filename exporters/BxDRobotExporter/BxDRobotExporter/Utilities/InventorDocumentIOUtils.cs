using System;
using BxDRobotExporter.Exporter;
using Inventor;

namespace BxDRobotExporter
{
    public class InventorDocumentIOUtils
    {
        public const string SYNTHESIS_PATH = @"C:\Program Files\Autodesk\Synthesis\Synthesis\Synthesis.exe";
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

        public static Inventor.PropertySet GetPropertySet(Inventor.PropertySets sets, string name, bool createIfDoesNotExist = true)
        {
            foreach (Inventor.PropertySet set in sets)
            {
                if (set.Name == name)
                {
                    return set;
                }
            }

            if (createIfDoesNotExist)
                return sets.Add(name);
            else
                return null;
        }

        public static bool HasProperty(Inventor.PropertySet set, string name)
        {
            // Inventor API provides no easy way to check if a property already exists. This try-catch is necessary.
            try
            {
                // Try to add new property. This will result in an exception if the property already exists.
                set[name].Value = 0;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void SetProperty<T>(Inventor.PropertySet set, string name, T value)
        {
            // Inventor API provides no easy way to check if a property already exists. This try-catch is necessary.
            try
            {
                // Try to add new property. This will result in an exception if the property already exists.
                set.Add(value, name);
            }
            catch (ArgumentException)
            {
                // Property already exists, update value
                set[name].Value = value;
            }
        }

        public static void RemoveProperty(Inventor.PropertySet set, string name)
        {
            // Inventor API provides no easy way to check if a property already exists. This try-catch is necessary.
            try
            {
                // Try to add new property. This will result in an exception if the property already exists.
                set[name].Delete();
            }
            catch (Exception)
            {
            }
        }

        public static T GetProperty<T>(Inventor.PropertySet set, string name, T defaultValue)
        {
            // Inventor API provides no easy way to check if a property already exists. This try-catch is necessary.
            try
            {
                // Try to add new property with default value. This will result in an exception if the property already exists.
                set.Add(defaultValue, name);
                return defaultValue;
            }
            catch (ArgumentException)
            {
                // Property already exists, get existing value
                return (T)set[name].Value;
            }
        }
    }

    public static class LegacyEvents
    {
        public static event Action RobotModified;
        
        public static void OnRobotModified()
        {
            RobotModified?.Invoke();
        }
    }
}