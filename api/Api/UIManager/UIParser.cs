using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SynthesisAPI.AssetManager;
using VisualElement = SynthesisAPI.UIManager.VisualElements.VisualElement;
using _UnityVisualElement = UnityEngine.UIElements.VisualElement;
using UnityEngine;
using UnityEngine.UIElements;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using Logger = SynthesisAPI.Utilities.Logger;
using System.Reflection;

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
            RecursivelySearchForStyleSheet(nodes);
            _UnityVisualElement root = new _UnityVisualElement() { name = name };
            foreach (XmlNode node in nodes)
            {
                if (node.Name.Replace("ui:", "") != "Style") {
                    root.Add(CreateVisualElement(node).UnityVisualElement);
                }
            }
            return root.GetVisualElement();
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

            // Logger.Log($"Looking for type: {node.Name.Replace("ui:", "")}");
            Type elementType = Array.Find(typeof(_UnityVisualElement).Assembly.GetTypes(), x =>
                x.Name.Equals(node.Name.Replace("ui:", "")));
            if (elementType == null)
            {
                if (!node.Name.Replace("ui:", "").Equals("Style"))
                {
                    Logger.Log($"Couldn't find type \"{node.Name.Replace("ui:", "")}\"\nSkipping...", LogLevel.Warning);
                }
                return null;
            }
            _UnityVisualElement element = (_UnityVisualElement)typeof(ApiProvider).GetMethod("CreateUnityType").MakeGenericMethod(elementType)
                .Invoke(null, new object[] { new object[] {} });
            if (element != null)
            {
                // Logger.Log("Creating element..");

                foreach (XmlAttribute attr in node.Attributes)
                {
                    // Logger.Log("Parsing Attribute");

                    var property = elementType.GetProperty(MapCssName(attr.Name));

                    if (attr.Name.Equals("class"))
                    {
                        //Logger.Log("Class found with value: " + attr.Value);
                        element.AddToClassList(attr.Value);
                        element = StyleSheetManager.ApplyClassFromStyleSheets(attr.Value, element);
                    }

                    if (property == null)
                    {
                        if (!attr.Name.Equals("class"))
                        {
                            Logger.Log($"Skipping attribute \"{attr.Name}\"", LogLevel.Warning);
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
                            // Logger.Log($"Parsing Boolean: {attr.Value}");
                            property.SetValue(element, bool.Parse(attr.Value));
                            break;
                        case "String":
                            // Logger.Log($"Parsing String: {attr.Value}");
                            property.SetValue(element, attr.Value);
                            break;
                        case "IStyle":
                            // Logger.Log("Parsing IStyle");
                            element = ParseStyle(attr.Value, element);
                            break;
                        default:
                            throw new Exception($"Found no matching type to {property.PropertyType.FullName}");
                    }

                    // Logger.Log($"Finished Parsing Attribute: {attr.Name}");
                }
            } else
            {
                element = ApiProvider.CreateUnityType<_UnityVisualElement>();
            }

            // Logger.Log("Finished Creating Element");

            _UnityVisualElement resultElement = (_UnityVisualElement)element;

            foreach (XmlNode child in node.ChildNodes)
            {
                // Logger.Log($"Adding child: {child.Name}");
                var parsedChild = CreateVisualElement(child);
                if (parsedChild != null)
                    resultElement.Add(parsedChild.UnityVisualElement);
                // Logger.Log($"Finished adding child: {child.Name}");
            }

            // Logger.Log("Returning Result");
            return resultElement.GetVisualElement();
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="element">WARNING: Make sure this type is or inherits <see cref="UnityEngine.UIElements.VisualElement" /></param>
        internal static _UnityVisualElement ParseStyle(string data, _UnityVisualElement element)
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

        internal static PropertyInfo? MapProperty(string propertyStr)
        {
            string propertyName = MapCssName(propertyStr);
            if (propertyName == "unityFontStyle") propertyName = "unityFontStyleAndWeight";
            if (propertyName == "textAlign") propertyName = "unityTextAlign";
            return typeof(IStyle).GetProperty(propertyName);
        }

        internal static object? ParseProperty(PropertyInfo property, string value)
        {
            if (property.PropertyType.GenericTypeArguments.Length > 0)
            {
                var result = typeof(UIParser)
                    .GetMethod("ToStyleEnum",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .MakeGenericMethod(property.PropertyType.GenericTypeArguments[0])
                    .Invoke(null, new object[] { value });

                return result;
            }

            switch (property.PropertyType.Name)
            {
                case "StyleFloat":
                    return ToStyleFloat(value);
                case "StyleInt":
                    return ToStyleInt(value);
                case "StyleLength":
                    return ToStyleLength(value);
                case "StyleFont":
                    return ToStyleFont(value);
                case "StyleCursor":
                    return ToStyleCursor(value);
                case "StyleColor":
                    return ToStyleColor(value);
                case "StyleBackground":
                    return ToStyleBackground(value);
                default:
                    Logger.Log("Unhandled type in USS parser", LogLevel.Warning);
                    break;
            }
            return null;
        }

        internal static _UnityVisualElement ParseEntry(string entry, _UnityVisualElement element)
        { 
            if(element == null)
            {
                Logger.Log($"Failed to parse entry, element was null", LogLevel.Warning);
                return null;
            }
            var entrySplit = entry.Split(':');
            entrySplit[0] = entrySplit[0].Replace(" ", "");
            if (entrySplit[0] == "") return element;

            var property = MapProperty(entrySplit[0]);
            if (property == null)
            {
                Logger.Log($"Failed to find property \"{MapCssName(entrySplit[0])}\"", LogLevel.Warning);
                // Debug.Log($"Type of style: \"{typeof(element.style).FullName}\"");
            }

            try
            {
                var value = ParseProperty(property, entrySplit[1]);
                if (value == null)
                {
                    Logger.Log($"Failed to parse entry {property.Name}\n\"{entry}\"", LogLevel.Warning);
                }
                else
                {
                    property.SetValue(element.style, value);
                }
            }
            catch (Exception)
            {
                Logger.Log($"Failed to set property. Skipping {property.Name}\n\"{entry}\"", LogLevel.Warning);
            }

            return element;
        }

        internal static StyleFloat ToStyleFloat(string str) => new StyleFloat(float.Parse(str.Replace("px", "")));

        internal static StyleInt ToStyleInt(string str) => new StyleInt(int.Parse(str.Replace("px", "")));

        internal static StyleLength ToStyleLength(string str)
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

        internal static StyleFont ToStyleFont(string _) => new StyleFont(StyleKeyword.Null);

        internal static StyleBackground ToStyleBackground(string str)
        {
            string path = str.Replace(" ", "");
            if (path.StartsWith("url(\"") || path.StartsWith("url('"))
                path = path.Remove(0, 5);
            if (path.EndsWith("\")") || path.EndsWith("')"))
                path = path.Remove(path.Length - 2, 2);

            try
            {
                SpriteAsset? asset = AssetManager.AssetManager.GetAsset<SpriteAsset>(path);
                if (asset == null)
                {
                    Logger.Log($"Can't find USS background image asset: {path}", LogLevel.Warning);
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
                Logger.Log($"Exception when parsing background texture\n{e}", LogLevel.Warning);
                return new StyleBackground(StyleKeyword.Null);
            }
        }

        internal static StyleCursor ToStyleCursor(string _) => new StyleCursor(StyleKeyword.Null);

        internal static StyleColor ToStyleColor(string str)
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
                        float.Parse(numbers[3]));
                    break;
            }

            return color;
        }

        internal static StyleEnum<T> ToStyleEnum<T>(string str) where T : struct, IConvertible => new StyleEnum<T>
            {value = (T) Enum.Parse(typeof(T), MapCssName(str.Replace(" ", "")), true)};

        /// <summary>
        /// This is an extension method to properly convert a VisualElement type into a Synthesis analog.
        /// The Synthesis analog essentially stores visual element objects and reveals layers of usability.
        /// In order to properly parse they need to be converted to their child form then stored as their
        /// analog.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal static VisualElement GetVisualElement(this _UnityVisualElement element)
        {
            if(element == null)
            {
                throw new SynthesisException("Cannot get visual element of null unity element");
            }

            Type t = default;
            try
            {
                t = typeof(VisualElement).Assembly.GetTypes().First(t =>
                    t.Name == element.GetType().Name && t.FullName != element.GetType().FullName);
            }
            catch(Exception e)
            {
                throw new SynthesisException($"Failed to get visual element of {element.GetType().FullName}, could not find matching type", e);
            }
            try
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                return (VisualElement)Activator.CreateInstance(t, flags, null, new object[] { element }, null);
            }
            catch(Exception e)
            {
                throw new SynthesisException($"Failed to get visual element of {element.GetType().FullName}, attempted type {t.FullName}", e);
            }
        }

        private static void RecursivelySearchForStyleSheet(XmlNodeList nodeList)
        {
            foreach (XmlNode node in nodeList)
            {
                if (node.Name.Equals("Style"))
                {
                    var styleSheetPath = node.Attributes["src"].Value;
                    var ussAsset = AssetManager.AssetManager.GetAsset<UssAsset>(styleSheetPath);
                    if (ussAsset == null)
                    {
                        throw new SynthesisException($"Failed to find style sheet \n{styleSheetPath}\" when loading style sheets");
                    }
                    StyleSheetManager.AttemptRegistryOfNewStyleSheet(ussAsset);
                }

                if (node.HasChildNodes)
                {
                    RecursivelySearchForStyleSheet(node.ChildNodes);
                }
            }
        }

    }
}
