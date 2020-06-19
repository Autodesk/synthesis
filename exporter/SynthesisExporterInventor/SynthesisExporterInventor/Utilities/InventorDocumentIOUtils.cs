using System;
using Inventor;

namespace SynthesisExporterInventor.Utilities
{
    public static class InventorDocumentIOUtils
    {
        public static Inventor.PropertySet GetPropertySet(PropertySets sets, string name, bool createIfDoesNotExist = true)
        {
            foreach (Inventor.PropertySet set in sets)
            {
                if (set.Name == name)
                {
                    return set;
                }
            }

            return createIfDoesNotExist ? sets.Add(name) : null;
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
}