using System.Xml.Serialization;

namespace TestApi
{
    [XmlRoot("TestXmlObject")]
    public class TestXmlObject
    {
        [XmlElement("Text")]
        public string Text { get; set; }
    }
}
