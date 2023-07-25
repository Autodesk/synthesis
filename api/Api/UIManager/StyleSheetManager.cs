using System.Collections.Generic;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Utilities;

#nullable enable

namespace SynthesisAPI.UIManager
{
    public class StyleSheetManager
    {
        private static Dictionary<string, StyleSheet> styleSheets = new Dictionary<string,StyleSheet>();

        public static void AttemptRegistryOfNewStyleSheet(UssAsset asset)
        {
            // Logger.Log("[UI] Attempting registry of new stylesheet with name [" + asset.Name + "]");
            if (!styleSheets.ContainsKey(asset.Name) && asset.StyleSheet != null)
            {
                styleSheets.Add(asset.Name, asset.StyleSheet);
            }
        }

        internal static UnityEngine.UIElements.VisualElement? ApplyClassFromStyleSheets(string className, UnityEngine.UIElements.VisualElement? visualElement)
        {
            // Logger.Log("[UI] ApplyClassFromStyleSheets called with [" + styleSheets.Count + "] stylesheets loaded");
            foreach (StyleSheet styleSheet in styleSheets.Values)
            {
                if (styleSheet.HasClass(className))
                {
                    // Logger.Log("[UI] Class [" + className + "] found in stylesheet");
                    return styleSheet.ApplyClassToVisualElement(className, visualElement);
                }
            }

            string visualElementName = visualElement != null ? visualElement.name : "null";
            Logger.Log("Could not apply class [" + className + "] to [" + visualElementName + "] with [" + styleSheets.Count + "] stylesheets currently loaded", LogLevel.Warning);

            return visualElement;
        }
    }
}