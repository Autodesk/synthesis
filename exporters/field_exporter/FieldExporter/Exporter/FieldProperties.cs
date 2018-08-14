using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FieldExporter.Exporter
{
    class FieldProperties
    {
        BXDVector3[] spawnpoints;

        FieldProperties(BXDVector3[] spawnpoints)
        {
            this.spawnpoints = spawnpoints;
        }

        public void Write(string path)
        {
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter writer = XmlWriter.Create(path, settings);

            // Begins the document.
            writer.WriteStartDocument();

            /// Writes the root element and its GUID.
            writer.WriteStartElement("FieldData");

            // Goals
            writer.WriteStartElement("Goals");
            writer.WriteStartElement("RedGoals");
            writer.WriteEndElement();
            writer.WriteStartElement("BlueGoals");
            writer.WriteEndElement();
            writer.WriteEndElement();

            // Gamepieces
            writer.WriteStartElement("General");
            writer.WriteStartElement("Gamepieces");
            writer.WriteEndElement();

            // Spawn Points
            foreach (BXDVector3 point in spawnpoints)
            {
                writer.WriteStartElement("RobotSpawnPoint");
                writer.WriteAttributeString("x", point.x.ToString());
                writer.WriteAttributeString("y", point.y.ToString());
                writer.WriteAttributeString("z", point.z.ToString());
                writer.WriteEndElement();
            }

            // Ends the document.
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();

            // Close the writer.
            writer.Close();
        }
    }
}
