using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;

namespace SynthesisAPI.UIManager
{
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
                        ParseStyle(attr.Value, ref element);
                        break;
                }
            }

            return element;
        }

        public static void ParseStyle(string data, ref VisualElement element)
        {
            data.Replace("&apos;", "\"");
            string[] entries = data.Split(';');

            foreach (string entry in entries)
            {
                ParseEntry(entry, ref element);
            }
        }

        #region Parsing entries

        #region "The Switch"

        public static void ParseEntry(string entry, ref VisualElement element)
        {
            var entrySplit = entry.Split(':');
            entrySplit[0].Replace(" ", "");

            switch (entrySplit[0])
            {
                case "width":
                    element.style.width = entrySplit[1].ToStyleLength();
                    break;
                case "height":
                    element.style.height = entrySplit[1].ToStyleLength();
                    break;
                case "max-width":
                    element.style.maxWidth = entrySplit[1].ToStyleLength();
                    break;
                case "max-height":
                    element.style.maxHeight = entrySplit[1].ToStyleLength();
                    break;
                case "min-width":
                    element.style.minWidth = entrySplit[1].ToStyleLength();
                    break;
                case "min-height":
                    element.style.minHeight = entrySplit[1].ToStyleLength();
                    break;
                case "flex-basis":
                    element.style.flexBasis = entrySplit[1].ToStyleLength();
                    break;
                case "flex-grow":
                    element.style.flexGrow = entrySplit[1].ToStyleFloat();
                    break;
                case "flex-shrink":
                    element.style.flexShrink = entrySplit[1].ToStyleFloat();
                    break;
                case "flex-direction":
                    element.style.flexDirection = entrySplit[1].ToStyleEnum<FlexDirection>();
                    break;
                case "flex-wrap":
                    element.style.flexWrap = entrySplit[1].ToStyleEnum<Wrap>();
                    break;
                case "overflow":
                    element.style.overflow = entrySplit[1].ToStyleEnum<Overflow>();
                    break;
                case "unity-overflow-clip-box":
                    element.style.unityOverflowClipBox = entrySplit[1].ToStyleEnum<OverflowClipBox>();
                    break;
                case "left":
                    element.style.left = entrySplit[1].ToStyleLength();
                    break;
                case "top":
                    element.style.top = entrySplit[1].ToStyleLength();
                    break;
                case "right":
                    element.style.right = entrySplit[1].ToStyleLength();
                    break;
                case "bottom":
                    element.style.bottom = entrySplit[1].ToStyleLength();
                    break;
                case "margin-left":
                    element.style.marginLeft = entrySplit[1].ToStyleLength();
                    break;
                case "margin-top":
                    element.style.marginTop = entrySplit[1].ToStyleLength();
                    break;
                case "margin-right":
                    element.style.marginRight = entrySplit[1].ToStyleLength();
                    break;
                case "margin-bottom":
                    element.style.marginBottom = entrySplit[1].ToStyleLength();
                    break;
                case "padding-left":
                    element.style.paddingLeft = entrySplit[1].ToStyleLength();
                    break;
                case "padding-top":
                    element.style.paddingTop = entrySplit[1].ToStyleLength();
                    break;
                case "padding-right":
                    element.style.paddingRight = entrySplit[1].ToStyleLength();
                    break;
                case "padding-bottom":
                    element.style.paddingBottom = entrySplit[1].ToStyleLength();
                    break;
                case "position":
                    element.style.position = entrySplit[1].ToStyleEnum<Position>();
                    break;
                case "align-self":
                    element.style.alignSelf = entrySplit[1].ToStyleEnum<Align>();
                    break;
                case "unity-text-align":
                    element.style.unityTextAlign = entrySplit[1].ToStyleEnum<TextAnchor>();
                    break;
                case "unity-font-style-and-weight":
                    element.style.unityFontStyleAndWeight = entrySplit[1].ToStyleEnum<FontStyle>();
                    break;
                case "unity-font":
                    element.style.unityFont = entrySplit[1].ToStyleFont();
                    break;
                case "font-size":
                    element.style.fontSize = entrySplit[1].ToStyleLength();
                    break;
                case "white-space":
                    element.style.whiteSpace = entrySplit[1].ToStyleEnum<WhiteSpace>();
                    break;
                case "color":
                    element.style.color = entrySplit[1].ToStyleColor();
                    break;
                case "background-color":
                    element.style.backgroundColor = entrySplit[1].ToStyleColor();
                    break;
                case "border-color":
                    element.style.borderColor = entrySplit[1].ToStyleColor(); // TODO: Evaluate if this is ever used
                    break;
                case "background-image":
                    element.style.backgroundImage = entrySplit[1].ToStyleBackground();
                    break;
                case "unity-background-scale-mode":
                    element.style.unityBackgroundScaleMode = entrySplit[1].ToStyleEnum<ScaleMode>();
                    break;
                case "unity-background-image-tint-color":
                    element.style.unityBackgroundImageTintColor = entrySplit[1].ToStyleColor();
                    break;
                case "align-items":
                    element.style.alignItems = entrySplit[1].ToStyleEnum<Align>();
                    break;
                case "align-content":
                    element.style.alignContent = entrySplit[1].ToStyleEnum<Align>();
                    break;
                case "justify-content":
                    element.style.justifyContent = entrySplit[1].ToStyleEnum<Justify>();
                    break;
                case "border-left-color":
                    element.style.borderLeftColor = entrySplit[1].ToStyleColor();
                    break;
                case "border-top-color":
                    element.style.borderTopColor = entrySplit[1].ToStyleColor();
                    break;
                case "border-right-color":
                    element.style.borderRightColor = entrySplit[1].ToStyleColor();
                    break;
                case "border-bottom-color":
                    element.style.borderBottomColor = entrySplit[1].ToStyleColor();
                    break;
                case "border-left-width":
                    element.style.borderLeftWidth = entrySplit[1].ToStyleFloat();
                    break;
                case "border-top-width":
                    element.style.borderTopWidth = entrySplit[1].ToStyleFloat();
                    break;
                case "border-right-width":
                    element.style.borderRightWidth = entrySplit[1].ToStyleFloat();
                    break;
                case "border-bottom-width":
                    element.style.borderBottomWidth = entrySplit[1].ToStyleFloat();
                    break;
                case "border-top-left-radius":
                    element.style.borderTopLeftRadius = entrySplit[1].ToStyleLength();
                    break;
                case "border-top-right-radius":
                    element.style.borderTopRightRadius = entrySplit[1].ToStyleLength();
                    break;
                case "border-bottom-right-radius":
                    element.style.borderBottomRightRadius = entrySplit[1].ToStyleLength();
                    break;
                case "border-bottom-left-radius":
                    element.style.borderBottomLeftRadius = entrySplit[1].ToStyleLength();
                    break;
                case "unity-slice-left":
                    element.style.unitySliceLeft = entrySplit[1].ToStyleInt();
                    break;
                case "unity-slice-top":
                    element.style.unitySliceTop = entrySplit[1].ToStyleInt();
                    break;
                case "unity-slice-right":
                    element.style.unitySliceRight = entrySplit[1].ToStyleInt();
                    break;
                case "unity-slice-bottom":
                    element.style.unitySliceBottom = entrySplit[1].ToStyleInt();
                    break;
                case "opacity":
                    element.style.opacity = entrySplit[1].ToStyleFloat();
                    break;
                case "visibility":
                    element.style.visibility = entrySplit[1].ToStyleEnum<Visibility>();
                    break;
                case "cursor":
                    element.style.cursor = entrySplit[1].ToStyleCursor();
                    break;
                case "display":
                    element.style.display = entrySplit[1].ToStyleEnum<DisplayStyle>();
                    break;
                default:
                    throw new Exception($"{entrySplit[0]} is unrecognized by \"The Switch\". It's probably because " +
                        $"the one inside the UXML file actually has a dash in front of it for some reason.");
            }
        }

        #endregion

        #region Parse Methods

        public static StyleFloat ToStyleFloat(this string str)
        {
            return new StyleFloat(float.Parse(str));
        }

        public static StyleInt ToStyleInt(this string str)
        {
            return new StyleInt(int.Parse(str));
        }

        public static StyleLength ToStyleLength(this string str)
        {
            StyleLength length = new StyleLength();
            str.Replace(" ", "");
            if (str.Contains("auto"))
            {
                length.keyword = StyleKeyword.Auto;
            }
            else
            {
                length.keyword = StyleKeyword.None;
                bool usePercent = str.Contains("%");
                str.Replace("%", "");
                length.value = new Length(float.Parse(str), usePercent ? LengthUnit.Percent : LengthUnit.Pixel);
            }
            return length;
        }

        public static StyleFont ToStyleFont(this string str)
        {
            return new StyleFont(StyleKeyword.Null); // TODO
        }

        public static StyleBackground ToStyleBackground(this string str)
        {
            return new StyleBackground(StyleKeyword.Null); // TODO
        }

        public static StyleCursor ToStyleCursor(this string str)
        {
            return new StyleCursor(StyleKeyword.Null); // TODO
        }

        public static StyleColor ToStyleColor(this string str)
        {
            StyleColor color = new StyleColor();

            if (str[0] == ' ')
                str = str.Substring(1);

            List<string> numbers = new List<string>();
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

        public static StyleEnum<T> ToStyleEnum<T>(this string str) where T : struct, IConvertible
        {
            str.Replace(" ", "");
            StyleEnum<T> e = new StyleEnum<T>();
            e.value = (T)Enum.Parse(typeof(T), str);
            return e;
        }

        #endregion

        #endregion
    }
}
