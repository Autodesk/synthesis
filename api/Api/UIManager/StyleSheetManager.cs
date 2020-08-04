using System.Collections.Generic;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager
{
    // TODO: Pathing to Stylesheets
    // TODO: Use ParseEntry method in StyleSheet ParseLines method
    // TODO: Determine where to put ApplyClassFromStyleSheets in UIParser
    
    public class StyleSheetManager
    {
        private static Dictionary<string, StyleSheet> styleSheets = new Dictionary<string,StyleSheet>();

        public static void AttemptRegistryOfNewStyleSheet(string path)
        {
            if (!styleSheets.ContainsKey(path))
            {
                styleSheets.Add(path, new StyleSheet(path));
            }
        }

        public static void ApplyClassFromStyleSheets(string className, VisualElement visualElement)
        {
            foreach (StyleSheet styleSheet in styleSheets.Values)
            {
                if (styleSheet.HasClass(className))
                {
                    styleSheet.ApplyClassToVisualElement(className, visualElement);
                }
            }
        }
    }
}