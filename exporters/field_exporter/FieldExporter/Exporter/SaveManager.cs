using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

namespace FieldExporter.Exporter
{
    public class FailedToSaveException : Exception
    {
        public FailedToSaveException(Exception innerException) : base("Failed to save field configuration.", innerException) {}
    }
    public class FailedToLoadException : Exception
    {
        public FailedToLoadException(Exception innerException) : base("Failed to load field configuration.", innerException) { }
    }

    static class SaveManager
    {
        public static void Save(AssemblyDocument document, FieldProperties fieldProps, List<PropertySet> propertySets)
        {
            Inventor.PropertySets inventorPropertySets = document.PropertySets;

            try
            {
                Inventor.PropertySet p = GetPropertySet(inventorPropertySets, "synthesisField");

                // Field Properties
                SetProperty(p, "spawnpointCount", fieldProps.spawnpoints.Length);
                for (int i = 0; i < fieldProps.spawnpoints.Length; i++)
                {
                    SetProperty(p, "spawnpoint" + i.ToString() + ".x", fieldProps.spawnpoints[i].x);
                    SetProperty(p, "spawnpoint" + i.ToString() + ".y", fieldProps.spawnpoints[i].y);
                    SetProperty(p, "spawnpoint" + i.ToString() + ".z", fieldProps.spawnpoints[i].z);
                }

                SetProperty(p, "gamepieceCount", fieldProps.gamepieces.Length);
                for (int i = 0; i < fieldProps.gamepieces.Length; i++)
                {
                    SetProperty(p, "gamepiece" + i.ToString() + ".id", fieldProps.gamepieces[i].id);
                    SetProperty(p, "gamepiece" + i.ToString() + ".spawnX", fieldProps.gamepieces[i].spawnpoint.x);
                    SetProperty(p, "gamepiece" + i.ToString() + ".spawnY", fieldProps.gamepieces[i].spawnpoint.y);
                    SetProperty(p, "gamepiece" + i.ToString() + ".spawnZ", fieldProps.gamepieces[i].spawnpoint.z);
                    SetProperty(p, "gamepiece" + i.ToString() + ".holdingLimit", (int)fieldProps.gamepieces[i].holdingLimit);
                }

                // Property Sets
                SetProperty(p, "propertySetCount", propertySets.Count);
                for (int i = 0; i < propertySets.Count; i++)
                {
                    SetProperty(p, "propertySet" + i.ToString() + ".id", propertySets[i].PropertySetID);
                    SetProperty(p, "propertySet" + i.ToString() + ".collisionType", (int)propertySets[i].Collider.CollisionType);
                    SetProperty(p, "propertySet" + i.ToString() + ".friction", propertySets[i].Friction);
                    SetProperty(p, "propertySet" + i.ToString() + ".mass", propertySets[i].Mass);
                }
            }
            catch (Exception e)
            {
                throw new FailedToSaveException(e);
            }
        }

        public static void Load(AssemblyDocument document, out FieldProperties fieldProps, out List<PropertySet> propertySets)
        {
            Inventor.PropertySets inventorPropertySets = document.PropertySets;

            try
            {
                Inventor.PropertySet p = GetPropertySet(inventorPropertySets, "synthesisField");

                // Field Properties
                BXDVector3[] spawnpoints = new BXDVector3[GetProperty(p, "spawnpointCount", 0)];
                for (int i = 0; i < spawnpoints.Length; i++)
                {
                    spawnpoints[i].x = GetProperty(p, "spawnpoint" + i.ToString() + ".x", 0.0f);
                    spawnpoints[i].y = GetProperty(p, "spawnpoint" + i.ToString() + ".y", 0.0f);
                    spawnpoints[i].z = GetProperty(p, "spawnpoint" + i.ToString() + ".z", 0.0f);
                }

                Gamepiece[] gamepieces = new Gamepiece[GetProperty(p, "gamepieceCount", 0)];
                for (int i = 0; i < gamepieces.Length; i++)
                {
                    gamepieces[i] = new Gamepiece(GetProperty(p, "gamepiece" + i.ToString() + ".id", "unknown"),
                                                  new BXDVector3(GetProperty(p, "gamepiece" + i.ToString() + ".spawnX", 0.0),
                                                                 GetProperty(p, "gamepiece" + i.ToString() + ".spawnY", 0.0),
                                                                 GetProperty(p, "gamepiece" + i.ToString() + ".spawnZ", 0.0)),
                                                  (ushort)GetProperty(p, "gamepiece" + i.ToString() + ".holdingLimit", (int)ushort.MaxValue));
                }

                fieldProps = new FieldProperties(spawnpoints, gamepieces);

                // Property Sets
                propertySets = new List<PropertySet>();

                int propertySetCount = GetProperty(p, "propertySetCount", 0);
                for (int i = 0; i < propertySetCount; i++)
                {
                    propertySets.Add(new PropertySet(GetProperty(p, "propertySet" + i.ToString() + ".id", "unknown"),
                                                     new PropertySet.PropertySetCollider((PropertySet.PropertySetCollider.PropertySetCollisionType)GetProperty(p, "propertySet" + i.ToString() + ".collisionType", 0)),
                                                     GetProperty(p, "propertySet" + i.ToString() + ".friction", 50),
                                                     GetProperty(p, "propertySet" + i.ToString() + ".mass", 0.0f)));
                }

            }
            catch (Exception e)
            {
                throw new FailedToLoadException(e);
            }
        }

        #region Property Utilities
        private static Inventor.PropertySet GetPropertySet(Inventor.PropertySets sets, string name, bool createIfDoesNotExist = true)
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

        private static bool HasProperty(Inventor.PropertySet set, string name)
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

        private static void SetProperty<T>(Inventor.PropertySet set, string name, T value)
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

        private static void RemoveProperty(Inventor.PropertySet set, string name)
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

        private static T GetProperty<T>(Inventor.PropertySet set, string name, T defaultValue)
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
        #endregion
    }
}
