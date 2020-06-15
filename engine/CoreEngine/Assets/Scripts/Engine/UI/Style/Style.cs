using System;
using System.Xml;
using System.IO;
using System.Globalization;
using UnityEngine;

namespace Synthesis.UI.Style
{
    /// <summary>
    /// A class used to load style information from an <see cref="XmlDocument"/>
    /// </summary>
    public class Style // TODO: Probably add some sort of system to check if the style was properly parsed
    {
        public string Name { get; private set; }
        public struct UIColors
        {
            public Color Primary, PrimaryAccent, Secondary, SecondaryAccent, TextColor;
        }
        public struct EnvironmentColors
        {
            public Color GridFill, Skybox;
        }
        public UIColors UiColor { get; private set; }
        public EnvironmentColors EnvColor { get; private set; }
        public XmlDocument DataDoc { get; private set; }

        /// <summary>
        /// Constructs a <see cref="Style"/> object using a file at <paramref name="filePath"/>
        /// Currently I'm ignoring validation using a schema so I can focus on UI
        /// </summary>
        /// <param name="filePath">The path of the XML file</param>
        public Style(string filePath, string schema = "") {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            XmlDocument doc = new XmlDocument();
            doc.Load(fs);
            Set(doc);
        }

        /// <summary>
        /// Constructs a <see cref="Style"/> object using <paramref name="doc"/>
        /// </summary>
        /// <param name="doc"><see cref="XmlDocument"/> to get data from</param>
        public Style(XmlDocument doc)
        {
            Set(doc);
        }

        private void Set(XmlDocument doc) {
            // Stores a copy of the data for potential use later
            DataDoc = doc;

            // Gets the main node of the doc which is a style node. This contains all the data for one style
            XmlNode styleNode = doc.DocumentElement;

            // Stores data into member variables
            Name = styleNode.Attributes[0].Value;
            XmlNode uiNode = styleNode["ui"];
			XmlNode envNode = styleNode["environment"];

            UiColor = new UIColors()
            {
                Primary = GetColor(uiNode["primary"], true),
                PrimaryAccent = GetColor(uiNode["primary_accent"], true),
                Secondary = GetColor(uiNode["secondary"], true),
                SecondaryAccent = GetColor(uiNode["secondary_accent"], true),
                TextColor = GetColor(uiNode["text_color"], true)
            };

			EnvColor = new EnvironmentColors() {
				GridFill = GetColor(envNode["grid_fill"], true),
				Skybox = GetColor(envNode["skybox"], true)
			};
        }

        private Color GetColor(XmlNode node, bool hexCode)
        {   
            if (hexCode)
            {
                uint col = uint.Parse(node.Attributes[0].Value.Substring(1), NumberStyles.HexNumber);
                // Debug.Log(col.ToString("X"));
                byte r = (byte)((col & 0xFF000000) >> 24);
                
                byte g = (byte)((col & 0x00FF0000) >> 16);
                byte b = (byte)((col & 0x0000FF00) >> 8);
                byte a = (byte)(col & 0x000000FF);
                return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
            }
            else
            {
                return new Color(float.Parse(node.Attributes[0].Value) / 255, float.Parse(node.Attributes[1].Value) / 255,
                    float.Parse(node.Attributes[2].Value) / 255, 1);
            }
        }

    }

}