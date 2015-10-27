using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

public partial class BXDFProperties
{
    /// <summary>
    /// Represents the current version of the BXDF file.
    /// </summary>
    public const string BXDF_CURRENT_VERSION = "2.2.0";

    /// <summary>
    /// Represents the default name of any element.
    /// </summary>
    public const string BXDF_DEFAULT_NAME = "UNDEFINED";

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
                case "2.2":
                    return ReadProperties_2_2(path);
                default: // If version is unknown.
                    // Attempt to read with the most recent version (but without validation).
                    return ReadProperties_2_2(path, false);
            }
        }
        else
        {
            // Could not find element, so return null.
            return null;
        }
    }

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

        Dictionary<string, PropertySet> propertySets = fieldDefinition.GetPropertySet();

        // Writes the data for each PropertySet.
        foreach (PropertySet propertySet in propertySets.Values)
        {
            // Starts the element.
            writer.WriteStartElement("PropertySet");

            // Writes the ID attribute for the PropertySet.
            writer.WriteAttributeString("ID", propertySet.PropertySetID);

            // Writes the collider property for the PropertySet.
            WriteCollider(writer, propertySet.Collider);

            // Writes the friction property for the PropertySet.
            writer.WriteElementString("Friction", propertySet.Friction.ToString());

            // Writes the mass property for the PropertySet.
            writer.WriteElementString("Mass", propertySet.Mass.ToString());

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
    /// Writes the BXDVector3 to an XML file with the given XmlWriter.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="writer"></param>
    private static void WriteBXDVector3(XmlWriter writer, BXDVector3 vec, string id)
    {
        writer.WriteStartElement("BXDVector3");

        writer.WriteAttributeString("VectorID", id);

        writer.WriteElementString("X", vec.x.ToString("F4"));
        writer.WriteElementString("Y", vec.y.ToString("F4"));
        writer.WriteElementString("Z", vec.z.ToString("F4"));

        writer.WriteEndElement();
    }

    /// <summary>
    /// Write the BoxCollider to an XML file with the given XmlWriter.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="boxCollider"></param>
    private static void WriteBoxCollider(XmlWriter writer, PropertySet.BoxCollider boxCollider)
    {
        writer.WriteStartElement("BoxCollider");

        WriteBXDVector3(writer, boxCollider.Scale, "Scale");

        writer.WriteEndElement();
    }

    /// <summary>
    /// Write the SphereCollider to an XML file with the given XmlWriter.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="sphereCollider"></param>
    private static void WriteSphereCollider(XmlWriter writer, PropertySet.SphereCollider sphereCollider)
    {
        writer.WriteStartElement("SphereCollider");

        writer.WriteElementString("Scale", sphereCollider.Scale.ToString("F4"));

        writer.WriteEndElement();
    }

    /// <summary>
    /// Write the MeshCollider to an XML file with the given XmlWriter.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="meshCollider"></param>
    private static void WriteMeshCollider(XmlWriter writer, PropertySet.MeshCollider meshCollider)
    {
        writer.WriteStartElement("MeshCollider");

        writer.WriteElementString("Convex", meshCollider.Convex.ToString());

        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes the given PropertySet's collider to an XML file with the given XmlWriter.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="propertySetCollider"></param>
    private static void WriteCollider(XmlWriter writer, PropertySet.PropertySetCollider propertySetCollider)
    {
        switch (propertySetCollider.CollisionType)
        {
            case PropertySet.PropertySetCollider.PropertySetCollisionType.BOX:
                WriteBoxCollider(writer, (PropertySet.BoxCollider)propertySetCollider);
                break;
            case PropertySet.PropertySetCollider.PropertySetCollisionType.SPHERE:
                WriteSphereCollider(writer, (PropertySet.SphereCollider)propertySetCollider);
                break;
            case PropertySet.PropertySetCollider.PropertySetCollisionType.MESH:
                WriteMeshCollider(writer, (PropertySet.MeshCollider)propertySetCollider);
                break;
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
            writer.WriteElementString("SubMeshID", node.SubMeshID.ToString());

            // Writes the CollisionMeshID element if a collider has been assigned.
            if (node.CollisionMeshID != -1)
                writer.WriteElementString("CollisionMeshID", node.CollisionMeshID.ToString());

            // Writes the PropertySetID element if a PropertySet has been assigned.
            if (node.PropertySetID != BXDF_DEFAULT_NAME)
                writer.WriteElementString("PropertySetID", node.PropertySetID);

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
