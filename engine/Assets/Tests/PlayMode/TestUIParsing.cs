namespace Tests
{
    public class TestUIParsing
    {
        /*
        // TODO
        [TestCase]
        public void TestParsingBasic()
        {
            var style = "style=\"flex-direction: row; height: 50px; width: auto; \"";
            
            XmlDocument testDoc = new XmlDocument();
            testDoc.LoadXml($"<VisualElement name=\"tab\" {style} />");
            Assert.NotNull(testDoc.FirstChild);

            var element = UIParser.CreateVisualElement(testDoc.FirstChild);
            Assert.NotNull(element);
            
            VisualElement unityElement = element.UnityVisualElement;
            Assert.NotNull(unityElement);
            
            Assert.AreEqual(FlexDirection.Row, unityElement.style.flexDirection.value);
            Assert.AreEqual(StyleKeyword.Undefined, unityElement.style.height.keyword); // Fun fact: If you assign the keyword to be None, it defaults to Undefined
            Assert.AreEqual(LengthUnit.Pixel, unityElement.style.height.value.unit);
            Assert.AreEqual(50, unityElement.style.height.value.value);
            Assert.AreEqual(StyleKeyword.Auto, unityElement.style.width.keyword);
        }

        [TestCase]
        public void TestParsingBasic2()
        {
            XmlDocument testDoc = new XmlDocument();
            testDoc.LoadXml("<Label text=\"Label\" name=\"title\" style=\"height: 100%; margin-left: 10px; margin-right: 10px; -unity-text-align: middle-center; -unity-font-style: bold; font-size: 22px;\" />");
            
            VisualElement element = ApiProvider.CreateUnityType<VisualElement>();
            element = UIParser.ParseStyle(testDoc.FirstChild.Attributes[2].Value, element);
            
            Assert.True(true);
        }
        */
    }

}
