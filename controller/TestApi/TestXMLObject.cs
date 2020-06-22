using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TestApi
{
    [XmlRoot("TestXMLObject")]
    public class TestXMLObject
    {
        [XmlElement("Text")]
        public string Text { get; set; }
    }
}
