using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

public partial class BXDFProperties
{
    /// <summary>
    /// Represents the current version of the BXDF file.
    /// </summary>
    public const string BXDF_CURRENT_VERSION = "2.0.1";

    /// <summary>
    /// Represents the default name of any element.
    /// </summary>
    public const string BXDF_DEFAULT_NAME = "UNDEFINED";

    /// <summary>
    /// Writes out the properties file in XML format for the node with the base provided to
    /// the path provided.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fieldDefinition"></param>
    public static void WriteProperties(string path, FieldDefinition fieldDefinition)
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;

        XmlWriter writer = XmlWriter.Create(path, settings);

        // Begins the document.
        writer.WriteStartDocument();

        /// Writes the root element and its GUID.
        writer.WriteStartElement("BXDF");
        writer.WriteAttributeString("Version", BXDF_CURRENT_VERSION);
        writer.WriteAttributeString("GUID", fieldDefinition.GUID.ToString());

        Dictionary<string, PhysicsGroup> physicsGroups = fieldDefinition.GetPhysicsGroups();

        // Writes the data for each PhysicsGroup.
        foreach (PhysicsGroup physicsGroup in physicsGroups.Values)
        {
            // Starts the element.
            writer.WriteStartElement("PhysicsGroup");

            // Writes the ID attribute for the PhysicsGroup.
            writer.WriteAttributeString("ID", physicsGroup.PhysicsGroupID);

            // Writes the collider property for the PhysicsGroup.
            writer.WriteElementString("Collider", physicsGroup.CollisionType.ToString());

            // Writes the friction property for the PhysicsGroup.
            writer.WriteElementString("Friction", physicsGroup.Friction.ToString());

            // Writes the mass property for the PhysicsGroup.
            writer.WriteElementString("Mass", physicsGroup.Mass.ToString());

            // Ends the element.
            writer.WriteEndElement();
        }

        // Writes the node group.
        WriteFieldNodeGroup(writer, fieldDefinition.NodeGroup);

        // Ends the document.
        writer.WriteEndDocument();

        // Close the writer.
        writer.Close();
    }

    /// <summary>
    /// Reads the given BXDF file from the given path with the latest version possible.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FieldDefinition ReadProperties(string path)
    {
        XmlReader reader = XmlReader.Create(path);

        // Find the BXDF element.
        if (reader.ReadToFollowing("BXDF"))
        {
            string version = reader["Version"];

            // Determine the version of the file.
            switch (version.Substring(0, version.LastIndexOf('.')))
            {
                case "2.0":
                    return ReadProperties_2_0(path);
                default: // If version is unknown.
                    // Attempt to read with the most recent version.
                    return ReadProperties_2_0(path, false);
            }
        }
        else
        {
            // Could not find element, so return null.
            return null;
        }
    }

    /// <summary>
    /// Writes all the data included in a FieldNodeGroup.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="fieldNodeGroup"></param>
    private static void WriteFieldNodeGroup(XmlWriter writer, FieldNodeGroup fieldNodeGroup)
    {
        // Starts the assembly element.
        writer.WriteStartElement("NodeGroup");

        // Writes the NodeGroupID property.
        writer.WriteAttributeString("ID", fieldNodeGroup.NodeGroupID);

        foreach (FieldNode node in fieldNodeGroup.EnumerateFieldNodes())
        {
            // Starts the element.
            writer.WriteStartElement("Node");

            // Writes the NodeID attribute.
            writer.WriteAttributeString("ID", node.NodeID);

            // Writes the MeshID element.
            writer.WriteElementString("MeshID", node.MeshID.ToString());

            // Writes the PhysicsGroupID element.
            writer.WriteElementString("PhysicsGroupID", node.PhysicsGroupID);

            // Ends the element.
            writer.WriteEndElement();
        }

        foreach (FieldNodeGroup nodeGroup in fieldNodeGroup.EnumerateFieldNodeGroups())
        {
            // Reiterates as the current FieldNodeGroup.
            WriteFieldNodeGroup(writer, nodeGroup);
        }

        // Ends the assembly element.
        writer.WriteEndElement();
    }
}
