using System.Collections.Generic;
using System.IO;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Runtime;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager
{
    public class StyleSheetManager
    {
        private static Dictionary<string, StyleSheet> styleSheets = new Dictionary<string,StyleSheet>();

        public static void AttemptRegistryOfNewStyleSheet(UssAsset asset)
        {
            if (!styleSheets.ContainsKey(asset.Name))
            {
                styleSheets.Add(asset.Name, asset._styleSheet);
            }
        }

        internal static UnityEngine.UIElements.VisualElement ApplyClassFromStyleSheets(string className, UnityEngine.UIElements.VisualElement visualElement)
        {
            foreach (StyleSheet styleSheet in styleSheets.Values)
            {
                if (styleSheet.HasClass(className))
                {
                    return styleSheet.ApplyClassToVisualElement(className, visualElement);
                }
            }

            return visualElement;
        }
    }
}