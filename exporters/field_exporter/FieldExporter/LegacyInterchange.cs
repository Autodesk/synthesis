using System;
using System.Collections.Generic;
using Inventor;

namespace FieldExporter
{
    /// <summary>
    /// This class is used for transfering data from the Inventor Addin to the old exporter.
    /// </summary>
    public static class LegacyInterchange
    {
        public static List<PropertySet> PropSets;

        internal static Dictionary<string, List<string>> AssemblyDictionary = new Dictionary<string, List<string>>();

        /// <summary>
        /// Key is ComponentOccurence.Name and Value is PropertySet.ID. WARNING: Do not use IDictionary.Add(). While it will work for parts, assemblies will not work
        /// </summary>
        internal static Dictionary<string, string> CompPropertyDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Pretty much the same as using CompPropertyDictionary, except it just returns null if the ComponentName is not 
        /// </summary>
        /// <param name="ComponentName"></param>
        /// <returns></returns>
        public static string GetCompFromDictionary(string ComponentName)
        {
            try
            {
                return CompPropertyDictionary[ComponentName];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Adds a component to the Component Property Dictionary.
        /// </summary>
        /// <param name="ComponentID"></param>
        /// <param name="Component"></param>
        public static void AddComponents(string ComponentID, ComponentOccurrence Component)
        {
            switch (Component.DefinitionDocumentType)
            {
                case DocumentTypeEnum.kPartDocumentObject:
                    CompPropertyDictionary.Add(Component.Name, ComponentID);
                    break;
                case DocumentTypeEnum.kAssemblyDocumentObject:
                    AssemblyComponentDefinition AsmDef = (AssemblyComponentDefinition)Component.Definition;
                    List<string> AssemblyPartList = new List<string>();
                    foreach(ComponentOccurrence Comp in AsmDef.Occurrences.AllLeafOccurrences)
                    {
                        try
                        {
                            AssemblyPartList.Add(Comp.Name);
                            CompPropertyDictionary.Add(Comp.Name, ComponentID);
                        }
                        catch (ArgumentException) { }
                    }
                    try
                    {
                        AssemblyDictionary.Add(Component.Name, AssemblyPartList);
                    }
                    catch (ArgumentException)
                    {
                        AssemblyDictionary.Remove(Component.Name);
                        AssemblyDictionary.Add(Component.Name, AssemblyPartList);
                    }
                    break;
                default:
                    throw new ArgumentException("ERROR: Component not an assembly or part", "Component");
            }
        }

        /// <summary>
        /// Removes all Dictionary entries that contain the specified ComponentID
        /// </summary>
        /// <param name="ComponentID"></param>
        /// <remarks>
        /// Not really sure this is efficient, but I thought that it would be bad to remove an item from the dictionary, while reading through it with a foreach loop. I don't think that it would use indices, but who knows.
        /// </remarks>
        public static void RemoveComponent(string ComponentID)
        {
            List<string> RemoveQueue = new List<string>();
            foreach(KeyValuePair<string, string> Pair in CompPropertyDictionary)
            {
                if(Pair.Value == ComponentID)
                {
                    RemoveQueue.Add(Pair.Key);
                }
            }
            foreach(var key in RemoveQueue)
            {
                CompPropertyDictionary.Remove(key);
            }
        }

        /// <summary>
        /// Removes a part from the CompPropertyDictionary
        /// </summary>
        /// <param name="PartName"></param>
        public static void RemovePart(string PartName)
        {
            CompPropertyDictionary.Remove(PartName);
        }

        /// <summary>
        /// Removes all the parts of an assembly from CompPropertyDictionary
        /// </summary>
        /// <param name="AssemblyName"></param>
        public static void RemoveAssembly(string AssemblyName)
        {
            foreach(string PartName in AssemblyDictionary[AssemblyName])
            {
                RemovePart(PartName);
            }
        }
    }
}
