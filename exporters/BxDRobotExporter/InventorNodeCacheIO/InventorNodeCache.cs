using System;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace InventorNodeCacheIO
{
    public class InventorNodeCache
    {
        public static InventorNodeCache Instance { get; internal set; }

        public Dictionary<Guid, List<string>> NodePartBreakdown;

        public static void WriteInstance()
        {

        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
