using NUnit.Framework;
using SynthesisAPI.UIManager;
using UnityEngine.UIElements;

namespace TestApi
{
    [TestFixture]
    public class TestUIParsing
    {
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
