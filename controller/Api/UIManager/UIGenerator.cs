using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;

namespace SynthesisAPI.UIManager
{
    // ReSharper disable once InconsistentNaming
    public static class UIGenerator
    {
        /// <summary>
        /// Parse an XmlNode into a VisualElement.
        /// TODO: Account for other types of VisualElements with special attributes (i.e. Label, Button, etc.)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static VisualElement CreateVisualElement(XmlNode node)
        {
            VisualElement element = new VisualElement();

            foreach (XmlAttribute attr in node.Attributes[0])
            {
                switch (attr.Name)
                {
                    case "name":
                        element.name = attr.Value;
                        break;
                    case "style":
                        ParseStyle(attr.Value, element);
                        break;
                }
            }

            return element;
        }

        private static void ParseStyle(string data, VisualElement element) => data.Replace("&apos;", "\"")
            .Split(';')
            .ToList().ForEach(e => ParseEntry(e, element));

        private static string MapCssName(string cssTag) =>
            (cssTag[0] == '-' ? char.ToLower(cssTag[1]) : char.ToLower(cssTag[0])) + cssTag.Split('-')
                .Select(s => char.ToUpper(s[0]) + s.Substring(1))
                .Aggregate("", (s, s1) => s + s1).Substring(1);

        private static void ParseEntry(string entry, VisualElement element)
        {
            var entrySplit = entry.Split(':');
            entrySplit[0] = entrySplit[0].Replace(" ", "");
            var property = element.style.GetType().GetProperty(MapCssName(entrySplit[0]));

            if (property.PropertyType.IsEnum)
            {
                property.SetValue(element.style,
                    typeof(UIGenerator).GetMethod("ToStyleEnum").MakeGenericMethod(property.PropertyType)
                        .Invoke(null, new object[] {entrySplit[1]}));
                return;
            }
            switch (property.PropertyType.ToString())
            {
                case "StyleFloat":
                    property.SetValue(element.style, ToStyleFloat(entrySplit[1]));
                    break;
                case "StyleInt":
                    property.SetValue(element.style, ToStyleInt(entrySplit[1]));
                    break;
                case "StyleLength":
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
        }

        private static StyleFloat ToStyleFloat(string str) => new StyleFloat(float.Parse(str));

        private static StyleInt ToStyleInt(string str) => new StyleInt(int.Parse(str));

        private static StyleLength ToStyleLength(string str)
        {
            var length = new StyleLength();
            str = str.Replace(" ", "");
            if (str.Contains("auto"))
            {
                length.keyword = StyleKeyword.Auto;
            }
            else
            {
                length.keyword = StyleKeyword.None;
                length.value = new Length(float.Parse(str.Replace("%", "")),
                    str.Contains("%") ? LengthUnit.Percent : LengthUnit.Pixel);
            }
            return length;
        }

        private static StyleFont ToStyleFont(string _) => new StyleFont(StyleKeyword.Null);

        private static StyleBackground ToStyleBackground(string _) => new StyleBackground(StyleKeyword.Null);

        private static StyleCursor ToStyleCursor(string _) => new StyleCursor(StyleKeyword.Null);

        private static StyleColor ToStyleColor(string str)
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
            {value = (T) Enum.Parse(typeof(T), str.Replace(" ", ""))};

    }
}
