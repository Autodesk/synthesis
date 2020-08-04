using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SynthesisAPI.AssetManager;
using UnityEngine;
using UnityEngine.UIElements;
using UnityVisualElement = UnityEngine.UIElements.VisualElement;
using VisualElement = SynthesisAPI.UIManager.VisualElements.VisualElement;
using SynthesisAPI.Runtime;

namespace SynthesisAPI.UIManager
{
    // ReSharper disable once InconsistentNaming
    public static class UIParser
    {
        public static VisualElement CreateVisualElement(string name, XmlDocument doc)
        {
            return doc.FirstChild.Name.Replace("ui:", "") == "UXML" ?
                CreateVisualElements(name, doc.FirstChild.ChildNodes) :
                CreateVisualElements(name, doc.ChildNodes);
        }

        public static VisualElement CreateVisualElements(string name, XmlNodeList nodes)
        {
            UnityVisualElement root = new UnityVisualElement() { name = name };
            foreach (XmlNode node in nodes)
            {
                if (node.Name.Replace("ui:", "") != "Style") {
                    root.Add((UnityVisualElement)CreateVisualElement(node));
                }
                else
                {
                    var ussAsset = AssetManager.AssetManager.GetAsset<UssAsset>(node.Attributes["src"].Value);
                    StyleSheetManager.AttemptRegistryOfNewStyleSheet(ussAsset);
                }
            }
            return (VisualElement)root;
        }

        /// <summary>
        /// Parse an XmlNode into a VisualElement.
        /// TODO: Account for other types of VisualElements with special attributes (i.e. Label, Button, etc.)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static VisualElement CreateVisualElement(XmlNode node)
        {
            if (node == null)
                throw new Exception("Node is null");

            // ApiProvider.Log($"Looking for type: {node.Name.Replace("ui:", "")}");
            Type elementType = Array.Find(typeof(UnityVisualElement).Assembly.GetTypes(), x =>
                x.Name.Equals(node.Name.Replace("ui:", "")));
            if (node.Name.Replace("ui:", "").Equals("Style"))
            {
                // ApiProvider.Log("[UI] Style w/ src location " + node.Attributes["src"].Value, LogLevel.Debug);
                var ussAsset = AssetManager.AssetManager.GetAsset<UssAsset>(node.Attributes["src"].Value);
                StyleSheetManager.AttemptRegistryOfNewStyleSheet(ussAsset);
            }
            if (elementType == null)
            {
                if (!node.Name.Replace("ui:", "").Equals("Style"))
                {
                    ApiProvider.Log($"Couldn't find type \"{node.Name.Replace("ui:", "")}\"\nSkipping...", LogLevel.Warning);
                }
                return null;
            }
            dynamic element = typeof(ApiProvider).GetMethod("CreateUnityType").MakeGenericMethod(elementType)
                .Invoke(null, new object[] { new object[] {} });
            if (element != null)
            {
                // ApiProvider.Log("Creating element..");

                foreach (XmlAttribute attr in node.Attributes)
                {
                    // ApiProvider.Log("Parsing Attribute");

                    var property = elementType.GetProperty(MapCssName(attr.Name));

                    if (attr.Name.Equals("class"))
                    {
                        //ApiProvider.Log("Class found with value: " + attr.Value);
                        element = StyleSheetManager.ApplyClassFromStyleSheets(attr.Value, element);
                    }

                    if (property == null)
                    {
                        if (!attr.Name.Equals("class"))
                        {
                            ApiProvider.Log($"Skipping attribute \"{attr.Name}\"", LogLevel.Warning);
                        }
                        // throw new Exception($"No property found with name \"{attr.Name}\"");
                        continue;
                    }

                    if (property.PropertyType.IsEnum)
                    {
                        property.SetValue(element, Enum.Parse(property.PropertyType, attr.Value));
                        continue;
                    }
                    switch (property.PropertyType.Name)
                    {
                        case "Boolean":
                            // ApiProvider.Log($"Parsing Boolean: {attr.Value}");
                            property.SetValue(element, bool.Parse(attr.Value));
                            break;
                        case "String":
                            // ApiProvider.Log($"Parsing String: {attr.Value}");
                            property.SetValue(element, attr.Value);
                            break;
                        case "IStyle":
                            // ApiProvider.Log("Parsing IStyle");
                            element = ParseStyle(attr.Value, element);
                            break;
                        default:
                            throw new Exception($"Found no matching type to {property.PropertyType.FullName}");
                    }

                    // ApiProvider.Log($"Finished Parsing Attribute: {attr.Name}");
                }
            } else
            {
                element = ApiProvider.CreateUnityType<UnityVisualElement>();
            }

            // ApiProvider.Log("Finished Creating Element");

            UnityVisualElement resultElement = (UnityVisualElement)element;

            foreach (XmlNode child in node.ChildNodes)
            {
                // ApiProvider.Log($"Adding child: {child.Name}");
                var parsedChild = CreateVisualElement(child);
                if (parsedChild != null)
                    resultElement.Add((UnityVisualElement)parsedChild);
                // ApiProvider.Log($"Finished adding child: {child.Name}");
            }

            // ApiProvider.Log("Returning Result");
            return (VisualElement)resultElement!;
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="element">WARNING: Make sure this type is or inherits <see cref="UnityEngine.UIElements.VisualElement" /></param>
        private static dynamic ParseStyle(string data, dynamic element)
        {
            var list = data.Replace("&apos;", "\"").Split(';').ToList();
            foreach (string entry in list)
            {
                if (entry != " ")
                {
                    element = ParseEntry(entry, element);
                }
            }
            return element;
        }

        public static string MapCssName(string cssTag) =>
            (cssTag[0] == '-' ? char.ToLower(cssTag[1]) : char.ToLower(cssTag[0])) + cssTag.Split('-')
                .Select(s => s != "" ? char.ToUpper(s[0]) + s.Substring(1) : "")
                .Aggregate("", (s, s1) => s + s1).Substring(1);

        internal static dynamic ParseEntry(string entry, dynamic element)
        {
            var entrySplit = entry.Split(':');
            entrySplit[0] = entrySplit[0].Replace(" ", "");
            if (entrySplit[0] == "") return element;

            string propertyName;
            propertyName = MapCssName(entrySplit[0]);
            if (propertyName == "unityFontStyle") propertyName = "unityFontStyleAndWeight";
            var property = typeof(IStyle).GetProperty(propertyName);
            if (property == null)
            {
                ApiProvider.Log($"Failed to find property \"{MapCssName(entrySplit[0])}\"", LogLevel.Warning);
                // Debug.Log($"Type of style: \"{typeof(element.style).FullName}\"");
            }

            try
            {
                if (property.PropertyType.GenericTypeArguments.Length > 0)
                {
                    property.SetValue(element.style,
                        typeof(UIParser).GetMethod("ToStyleEnum")
                            .MakeGenericMethod(property.PropertyType.GenericTypeArguments[0])
                            .Invoke(null, new object[] {entrySplit[1]}));
                    return element;
                }

                switch (property.PropertyType.Name)
                {
                    case "StyleFloat":
                        property.SetValue(element.style, ToStyleFloat(entrySplit[1]));
                        break;
                    case "StyleInt":
                        property.SetValue(element.style, ToStyleInt(entrySplit[1]));
                        break;
                    case "StyleLength":
                        // StyleLength len = ;
                        property.SetValue(element.style, ToStyleLength(entrySplit[1]));
                        break;
                    case "StyleFont":
                        property.SetValue(element.style, ToStyleFont(entrySplit[1]));
                        break;
                    case "StyleCursor":
                        property.SetValue(element.style, ToStyleCursor(entrySplit[1]));
                        break;
                    case "StyleColor":
                        property.SetValue(element.style, ToStyleColor(entrySplit[1]));
                        break;
                    case "StyleBackground":
                        property.SetValue(element.style, ToStyleBackground(entrySplit[1]));
                        break;
                    default:
                        ApiProvider.Log("Default");
                        break;
                        //throw new Exception("Unhandled type in USS parser");
                }
                // ApiProvider.Log("Successfully set styling for " + propertyName);
                
            }
            catch (Exception e)
            {
                ApiProvider.Log($"Failed to set property. Skipping \"{entrySplit[0]}\"", LogLevel.Warning);
            }

            return element;
        }

        public static StyleFloat ToStyleFloat(string str) => new StyleFloat(float.Parse(str));

        public static StyleInt ToStyleInt(string str) => new StyleInt(int.Parse(str));

        public static StyleLength ToStyleLength(string str)
        {
            str = str.Replace(" ", "");
            if (str.Contains("auto"))
                return new StyleLength(StyleKeyword.Auto);

            bool isPercent = str.Contains("%");
            LengthUnit unit;
            if (isPercent)
                unit = LengthUnit.Percent;
            else
                unit = LengthUnit.Pixel;

            // Debug.Log($"StyleInt from: \"{str}\"");
            // Debug.Log($"Units: \"{unit}\"");

            return new StyleLength(new Length(float.Parse(str.Replace("%", "").Replace("px", "")), unit));
            
        }

        public static StyleFont ToStyleFont(string _) => new StyleFont(StyleKeyword.Null);

        public static StyleBackground ToStyleBackground(string str)
        {
            string path = str.Replace(" ", "").Substring(5, str.Length - 8);
            
            try
            {
                SpriteAsset? asset = AssetManager.AssetManager.GetAsset<SpriteAsset>(path);
                if (asset == null)
                {
                    ApiProvider.Log("Can't find asset", LogLevel.Warning);
                    return new StyleBackground(StyleKeyword.Null);
                }
                else
                {
                    return new StyleBackground(asset.Sprite.texture);
                }
            }
            catch (Exception e)
            {
                // FAIL TO GET TEXTURE
                ApiProvider.Log("Exception when parsing background texture", LogLevel.Warning);
                return new StyleBackground(StyleKeyword.Null);
            }
        }

        public static StyleCursor ToStyleCursor(string _) => new StyleCursor(StyleKeyword.Null);

        public static StyleColor ToStyleColor(string str)
        {
            var color = new StyleColor();

            if (str[0] == ' ')
                str = str.Substring(1);

            var numbers = new List<string>();
            string start = str.Substring(str.IndexOf('(') + 1);
            while (true)
            {
                if (start.IndexOf(',') >= 0)
                {
                    numbers.Add(start.Substring(0, start.IndexOf(',')));
                    start = start.Substring(start.IndexOf(',') + 2);
                } else
                {
                    numbers.Add(start.Substring(0, start.IndexOf(')')));
                    break;
                }
            }

            switch (str.Substring(0, str.IndexOf('(')))
            {
                case "rgb":
                    color.value = new Color(
                        float.Parse(numbers[0]) / 255.0f,
                        float.Parse(numbers[1]) / 255.0f,
                        float.Parse(numbers[2]) / 255.0f);
                    break;
                case "rgba":
                    color.value = new Color(
                        float.Parse(numbers[0]) / 255.0f,
                        float.Parse(numbers[1]) / 255.0f,
                        float.Parse(numbers[2]) / 255.0f,
                        float.Parse(numbers[3]) / 255.0f);
                    break;
            }

            return color;
        }

        public static StyleEnum<T> ToStyleEnum<T>(string str) where T : struct, IConvertible => new StyleEnum<T>
            {value = (T) Enum.Parse(typeof(T), MapCssName(str.Replace(" ", "")), true)};

        /// <summary>
        /// This is an extension method to properly convert a VisualElement type into a Synthesis analog.
        /// The Synthesis analog essentially stores visual element objects and reveals layers of usability.
        /// In order to properly parse they need to be converted to their child form then stored as their
        /// analog.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static VisualElement GetVisualElement(this UnityVisualElement element)
        {
            Type t = Array.Find(typeof(VisualElement).Assembly.GetTypes(), t => 
                t.Name == element.GetType().Name && t.FullName != element.GetType().FullName
                ) ?? typeof(VisualElement);
            return (VisualElement)Activator.CreateInstance(t, new object[] {element});
        }

    }
}
