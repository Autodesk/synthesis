using System.Collections.Generic;
using SynthesisAPI.AssetManager;

namespace SynthesisAPI.UIManager
{
    public class StyleSheetManager
    {
        private static Dictionary<string, StyleSheet> styleSheets = new Dictionary<string,StyleSheet>();

        public static void AttemptRegistryOfNewStyleSheet(UssAsset asset)
        {
            // ApiProvider.Log("[UI] Attempting registry of new stylesheet with name [" + asset.Name + "]");
            if (!styleSheets.ContainsKey(asset.Name))
            {
                styleSheets.Add(asset.Name, asset._styleSheet);
            }
        }

        internal static UnityEngine.UIElements.VisualElement ApplyClassFromStyleSheets(string className, UnityEngine.UIElements.VisualElement visualElement)
        {
            // ApiProvider.Log("[UI] ApplyClassFromStyleSheets called with [" + styleSheets.Count + "] stylesheets loaded");
            foreach (StyleSheet styleSheet in styleSheets.Values)
            {
                if (styleSheet.HasClass(className))
                {
                    // ApiProvider.Log("[UI] Class [" + className + "] found in stylesheet");
                    return styleSheet.ApplyClassToVisualElement(className, visualElement);
                }
            }

            return visualElement;
        }
    }
}