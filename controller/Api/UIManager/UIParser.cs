using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Reflection;
using SynthesisAPI.UIManager.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityVisualElement = UnityEngine.UIElements.VisualElement;
using SynVisualElement = SynthesisAPI.UIManager.VisualElements.VisualElement;
using SynthesisAPI.Runtime;
// using SynButton = SynthesisAPI.UIManager.VisualElements.Button;
// using SynLabel = SynthesisAPI.UIManager.VisualElements.Label;

namespace SynthesisAPI.UIManager
{
    // ReSharper disable once InconsistentNaming
    public static class UIParser
    {
        /// <summary>
        /// Parse an XmlNode into a VisualElement.
        /// TODO: Account for other types of VisualElements with special attributes (i.e. Label, Button, etc.)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static SynVisualElement CreateVisualElement(XmlNode node)
        {
            // VisualElement element = new VisualElement();
            Debug.Log("==1==");
            // Type elementType = Array.Find(typeof(SynVisualElement).Assembly.GetTypes(), x => x.Name.Equals(node.Name.Replace("ui:", "")));
            Type elementType = Array.Find(typeof(UnityVisualElement).Assembly.GetTypes(), x => x.Name.Equals(node.Name.Replace("ui:", "")));
            Debug.Log($"Type name: {elementType.FullName}");
            Debug.Log("==2==");
            // dynamic element = Activator.CreateInstance(elementType);
            dynamic element = typeof(ApiProvider).GetMethod("InstantiateFocusable").MakeGenericMethod(elementType).Invoke(null, null);
            Debug.Log(element.style.GetType().FullName);
            if (element == null) throw new Exception("Activator failed to create an instance");
            Debug.Log("==3==");

            foreach (XmlAttribute attr in node.Attributes)
            {
                var property = elementType.GetProperty(attr.Name);
                if (property == null) throw new Exception($"No property found with name \"{attr.Name}\"");
                Debug.Log("==4L==");

                switch (property.PropertyType.Name)
                {
                    case "Boolean":
                        Debug.Log("==5L==");
                        property.SetValue(element, bool.Parse(attr.Value));
                        break;
                    case "String":
                        Debug.Log("==6L==");
                        property.SetValue(element, attr.Value);
                        break;
                    case "IStyle":
                        Debug.Log("==7L==");
                        element = ParseStyle(attr.Value, element);
                        break;
                    default:
                        throw new Exception($"Found no matching type to {property.PropertyType.FullName}");
                }
                Debug.Log("==8L==");
            }

            foreach (XmlNode child in node.ChildNodes)
                elementType.GetMethod("Add", BindingFlags.Public).Invoke(element, new object[] { child }); //.Add(CreateVisualElement(child));
            Debug.Log("==9==");
            return element;
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

        private static dynamic ParseEntry(string entry, dynamic element)
        {
            var entrySplit = entry.Split(':');
            Debug.Log("--1--");
            entrySplit[0] = entrySplit[0].Replace(" ", "");
            if (entrySplit[0] == "") return element;
            Debug.Log("--2--");
            string propertyName;
            propertyName = MapCssName(entrySplit[0]);
            if (propertyName == "unityFontStyle") propertyName = "unityFontStyleAndWeight";
            var property = typeof(IStyle).GetProperty(propertyName);
            if (property == null)
            {
                Debug.Log($"Failed to find property \"{MapCssName(entrySplit[0])}\"");
                Debug.Log($"Type of style: \"{element.style.GetType()}\"");
                // Debug.Log($"Type of style: \"{typeof(element.style).FullName}\"");
            }
            Debug.Log("--3--");

            if (property.PropertyType.GenericTypeArguments.Length > 0)
            {
                Debug.Log("--4--");
                property.SetValue(element.style,
                    typeof(UIParser).GetMethod("ToStyleEnum").MakeGenericMethod(property.PropertyType.GenericTypeArguments[0])
                        .Invoke(null, new object[] { entrySplit[1] }));
                return element;
            }
            Debug.Log("--5--");
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
                    throw new Exception("Unhandled type in USS parser");
            }
            Debug.Log("--6--");
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

        public static StyleBackground ToStyleBackground(string _) => new StyleBackground(StyleKeyword.Null);

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

    }
}
