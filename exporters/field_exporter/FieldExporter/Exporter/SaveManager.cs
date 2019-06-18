using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Inventor;
using Newtonsoft.Json;
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
                SetProperty(p, "spawnpoints", JsonConvert.SerializeObject(fieldProps.spawnpoints));
                SetProperty(p, "gamepieces", JsonConvert.SerializeObject(fieldProps.gamepieces));

                // Property Sets
                SetProperty(p, "propertySets", JsonConvert.SerializeObject(propertySets, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto } ));

                // Occurrences
                for (int i = 0; i < propertySets.Count; i++)
                {
                    var tabPage = (Components.ComponentPropertiesTabPage) Program.MAINWINDOW.GetPropertySetsTabControl().TabPages[propertySets[i].PropertySetID];

                    if (tabPage != null)
                    {
                        Components.InventorTreeView treeView = tabPage.ChildForm.inventorTreeView;
                        List<string> occurrences = new List<string>();
                        foreach (TreeNode node in treeView.Nodes)
                            CreateOccurrenceList(node, "", occurrences);

                        SetProperty(p, "propertySets." + propertySets[i].PropertySetID + ".occurrences", JsonConvert.SerializeObject(occurrences));
                    }
                    else
                        SetProperty(p, "propertySets." + propertySets[i].PropertySetID + ".occurrences", "[]");
                }
            }
            catch (Exception e)
            {
                throw new FailedToSaveException(e);
            }
        }

        public static void CreateOccurrenceList(TreeNode node, string path, List<string> occurrences)
        {
            occurrences.Add(path + node.Name);

            foreach (TreeNode subnode in node.Nodes)
                CreateOccurrenceList(subnode, path + node.Name + '\\', occurrences);
        }

        public static void Load(AssemblyDocument document, out FieldProperties fieldProps, out List<PropertySet> propertySets, out Dictionary<string, List<string>> occurrencePropSets)
        {
            Inventor.PropertySets inventorPropertySets = document.PropertySets;

            try
            {
                Inventor.PropertySet p = GetPropertySet(inventorPropertySets, "synthesisField");

                // Field Properties
                BXDVector3[] spawnpoints = JsonConvert.DeserializeObject<BXDVector3[]>(GetProperty(p, "spawnpoints", "[]"));
                if (spawnpoints == null)
                    spawnpoints = new BXDVector3[0];

                Gamepiece[] gamepieces = JsonConvert.DeserializeObject<Gamepiece[]>(GetProperty(p, "gamepieces", "[]"));
                if (gamepieces == null)
                    gamepieces = new Gamepiece[0];

                fieldProps = new FieldProperties(spawnpoints, gamepieces);

                // Property Sets
                propertySets = JsonConvert.DeserializeObject<List<PropertySet>>(GetProperty(p, "propertySets", "[]"), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

                // Occurrences
                occurrencePropSets = new Dictionary<string, List<string>>();
                
                for (int i = 0; i < propertySets.Count(); i++)
                {
                    occurrencePropSets.Add(propertySets[i].PropertySetID,
                                           JsonConvert.DeserializeObject<List<string>>(GetProperty(p, "propertySets." + propertySets[i].PropertySetID + ".occurrences", "[]")));
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
