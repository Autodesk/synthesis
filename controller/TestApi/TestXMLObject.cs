using System.Xml.Serialization;

namespace TestApi
{
    [XmlRoot("TestXMLObject")]
    public class TestXmlObject
    {
        [XmlElement("Text")]
        public string Text { get; set; }
    }
}
