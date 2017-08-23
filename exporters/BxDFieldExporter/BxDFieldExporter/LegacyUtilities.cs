using System.Collections;
using System.Collections.Generic;
using FieldExporter.Components;

namespace BxDFieldExporter
{
    public static class LegacyUtilities
    {
        /// <summary>
        /// Creates a list of PropertySets (from the old exporter) using the ToLegacy() method I added. (See FieldDataType.cs -> FieldDataComponent)
        /// </summary>
        /// <param name="FieldComponents"></param>
        /// <returns>A list of all the field componets to be used in the export process</returns>
        public static List<PropertySet> GetLegacyProps(ArrayList FieldComponents)
        {
            List<PropertySet> ret = new List<PropertySet>();
            foreach(FieldDataComponent comp in FieldComponents)
            {
                ret.Add(comp.ToLegacy());
            }
            return ret;
        }

        public static ExportForm exporter = new ExportForm();

        /// <summary>
        /// This will contain a list of all of the top level assemblies and parts and the Component with which they are associated
        /// </summary>
        public static Dictionary<string, string> TreeParody = new Dictionary<string, string>();
    }
}
