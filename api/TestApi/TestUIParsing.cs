using NUnit.Framework;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine.UIElements;

namespace TestApi
{
    [TestFixture]
    public class TestUIParsing
    {
        [TestCase]
        public void TestParsingBasic()
        {
            XmlDocument testDoc = new XmlDocument();
            testDoc.LoadXml("<VisualElement name=\"tab\" style=\"flex-direction: row; height: 50px; width: auto; \" />");
            dynamic[] result = new dynamic[3];
            ParseStyle(testDoc.FirstChild.Attributes[1].Value, ref result);

            Assert.AreEqual(result[0].Item2, typeof(StyleEnum<FlexDirection>));
            Assert.AreEqual(result[0].Item1.value, FlexDirection.Row);
            Assert.AreEqual(result[1].Item2, typeof(StyleLength));
            Assert.AreEqual(result[1].Item1.keyword, StyleKeyword.Undefined); // Fun fact: If you assign the keyword to be None, it defaults to Undefined
            Assert.AreEqual(result[1].Item1.value.unit, LengthUnit.Pixel);
            Assert.AreEqual(result[1].Item1.value.value, 50);
            Assert.AreEqual(result[2].Item2, typeof(StyleLength));
            Assert.AreEqual(result[2].Item1.keyword, StyleKeyword.Auto);
        }

        [TestCase]
        public void TestParsingBasic2()
        {
            XmlDocument testDoc = new XmlDocument();
            testDoc.LoadXml("<Label text=\"Label\" name=\"title\" style=\"height: 100%; margin-left: 10px; margin-right: 10px; -unity-text-align: middle-center; -unity-font-style: bold; font-size: 22px;\" />");
            dynamic[] result = new dynamic[6];
            ParseStyle(testDoc.FirstChild.Attributes[2].Value, ref result);
            Assert.True(true);
        }

        #region CopyCode

        public static void ParseStyle(string data, ref dynamic[] returnObjects)
        {
            var list = data.Replace("&apos;", "\"").Split(';').ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != " ")
                {
                    (dynamic, Type) result = ParseEntry(list[i]);
                    if (result.Item2 != null)
                        returnObjects[i] = result;
                }
            }
        }

        public static (dynamic, Type) ParseEntry(string entry)
        {
            var entrySplit = entry.Split(':');
            entrySplit[0] = entrySplit[0].Replace(" ", "");
            if (entrySplit[0] == "") return (null, null);
            string propName = UIParser.MapCssName(entrySplit[0]);
            if (propName == "unityFontStyle") propName = "unityFontStyleAndWeight";
            var type = typeof(IStyle).GetProperty(propName).PropertyType;
            // typeof(TEntryType);
            // var property = entry.GetType().GetProperty(UIParser.MapCssName(entrySplit[0]));

            if (type.GenericTypeArguments.Length > 0) // ERROR: the type wont actually be an enum
            {
                return (typeof(UIParser).GetMethod("ToStyleEnum").MakeGenericMethod(type.GenericTypeArguments[0])
                        .Invoke(null, new object[] { entrySplit[1] }), type);
            }
            switch (type.Name) // ERROR: use type.Name instead of type.ToString()
            {
                case "StyleFloat":
                    return (UIParser.ToStyleFloat(entrySplit[1]), type);
                case "StyleInt":
                    return (UIParser.ToStyleInt(entrySplit[1]), type);
                case "StyleLength":
                    var len = (UIParser.ToStyleLength(entrySplit[1]), type);
                    return len;
                case "StyleFont":
                    return (UIParser.ToStyleFont(entrySplit[1]), type);
                case "StyleCursor":
                    return (UIParser.ToStyleCursor(entrySplit[1]), type);
                case "StyleColor":
                    return (UIParser.ToStyleColor(entrySplit[1]), type);
                case "StyleBackground":
                    return (UIParser.ToStyleBackground(entrySplit[1]), type);
                default:
                    throw new Exception($"Unhandled type in USS parser: \"{type.ToString()}\"");
            }
        }

        #endregion

        [TestCase]
        public void TestLengthPercentParse()
        {
            StyleLength length = UIParser.ToStyleLength("100%");
            Assert.AreEqual(length.keyword, StyleKeyword.Undefined);
            Assert.AreEqual(length.value.unit, LengthUnit.Percent);
            Assert.AreEqual(length.value.value, 100.0f);
        }

        [TestCase]
        public void TestMapCssName()
        {
            Assert.AreEqual(UIParser.MapCssName("flex-direction"), "flexDirection");
            Assert.AreEqual(UIParser.MapCssName("height"), "height");
            Assert.AreEqual(UIParser.MapCssName("-unity-background-image-tint-color"), "unityBackgroundImageTintColor");
        }
    }
}
