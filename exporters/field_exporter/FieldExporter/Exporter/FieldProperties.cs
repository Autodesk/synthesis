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
        Gamepiece[] gamepieces;

        public FieldProperties(BXDVector3[] spawnpoints, Gamepiece[] gamepieces)
        {
            this.spawnpoints = spawnpoints;
            this.gamepieces = gamepieces;
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
            // At some point this should actually be part of the BXDF, not user configurable.
            writer.WriteStartElement("General");
            writer.WriteStartElement("Gamepieces");
            foreach (Gamepiece gamepiece in gamepieces)
            {
                writer.WriteStartElement("gamepiece");
                writer.WriteAttributeString("id", gamepiece.id);
                writer.WriteAttributeString("holdinglimit", gamepiece.holdingLimit.ToString());

                BXDVector3 spawn = new BXDVector3(gamepiece.spawnpoint.x * -0.01, gamepiece.spawnpoint.y * 0.01, gamepiece.spawnpoint.z * 0.01);
                writer.WriteAttributeString("x", spawn.x.ToString());
                writer.WriteAttributeString("y", spawn.y.ToString());
                writer.WriteAttributeString("z", spawn.z.ToString());

                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            // Spawn Points
            foreach (BXDVector3 point in spawnpoints)
            {
                writer.WriteStartElement("RobotSpawnPoint");

                BXDVector3 spawn = new BXDVector3(point.x * -0.01, point.y * 0.01, point.z * 0.01);
                writer.WriteAttributeString("x", spawn.x.ToString());
                writer.WriteAttributeString("y", spawn.y.ToString());
                writer.WriteAttributeString("z", spawn.z.ToString());

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
