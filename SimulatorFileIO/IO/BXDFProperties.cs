using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

public class BXDFProperties
{
    #region XSD Markup

    /// <summary>
    /// The XSD markup to ensure valid document reading.
    /// </summary>
    private const string xsdMarkup =
      @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>

        <!-- definition of simple elements -->

        <xs:element name='mass' type='xs:nonNegativeInteger'/>
        <xs:element name='meshid' type='xs:nonNegativeInteger'/>
        <xs:element name='pgid' type='xs:string'/>

        <xs:element name='collider'>
            <xs:simpleType>
            <xs:restriction base='xs:string'>
                <xs:enumeration value='mesh'/>
                <xs:enumeration value='box'/>
            </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <xs:element name='friction'>
            <xs:simpleType>
            <xs:restriction base='xs:integer'>
                <xs:minInclusive value='0'/>
                <xs:maxInclusive value='10'/>
            </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <!-- definition of attributes -->

        <xs:attribute name='guid' type='xs:string'/>
        <xs:attribute name='id' type='xs:string'/>

        <!-- definition of complex elements -->

        <xs:element name='assembly'>
            <xs:complexType>
            <xs:sequence>
                <xs:choice minOccurs='0' maxOccurs='unbounded'>
                <xs:element ref='assembly'/>
                <xs:element ref='part'/>
                </xs:choice>
            </xs:sequence>
            <xs:attribute ref='id' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='pg'>
            <xs:complexType>
            <xs:sequence>
                <xs:element ref='collider'/>
                <xs:element ref='friction'/>
                <xs:element ref='mass'/>
            </xs:sequence>
            <xs:attribute ref='id' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='part'>
            <xs:complexType>
            <xs:sequence>
                <xs:element ref='meshid'/>
                <xs:element ref='pgid'/>
            </xs:sequence>
            <xs:attribute ref='id' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='bxdf'>
            <xs:complexType>
            <xs:sequence>
                <xs:element ref='pg' minOccurs='0' maxOccurs='unbounded'/>
                <xs:element ref='assembly'/>
            </xs:sequence>
            <xs:attribute ref='guid' use='required'/>
            </xs:complexType>
        </xs:element>

        </xs:schema>";

    #endregion

    /// <summary>
    /// Writes out the properties file in XML format for the node with the base provided to the path provided.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fieldDefinition"></param>
    public static void WriteXMLProperties(string path, Guid guid, FieldDefinition fieldDefinition)
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.NewLineOnAttributes = true;

        XmlWriter writer = XmlWriter.Create(path, settings);

        // Begins the document.
        writer.WriteStartDocument();

        /// Writes the root element and its GUID.
        writer.WriteStartElement("bxdf");
        writer.WriteAttributeString("guid", guid.ToString());

        Dictionary<string, PhysicsGroup> physicsGroups = fieldDefinition.GetPhysicsGroups();

        // Writes the data for each PhysicsGroup.
        foreach (PhysicsGroup physicsGroup in physicsGroups.Values)
        {
            // Starts the element.
            writer.WriteStartElement("pg");

            // Writes the ID attribute for the PhysicsGroup.
            writer.WriteAttributeString("id", physicsGroup.physicsGroupID);

            // Writes the collider property for the PhysicsGroup.
            writer.WriteElementString("collider", physicsGroup.collisionType.ToString().ToLower());

            // Writes the friction property for the PhysicsGroup.
            writer.WriteElementString("friction", physicsGroup.friction.ToString());

            // Writes the mass property for the PhysicsGroup.
            writer.WriteElementString("mass", physicsGroup.mass.ToString());

            // Ends the element.
            writer.WriteEndElement();
        }

        // Writes the node group.
        WriteFieldNodeGroup(writer, fieldDefinition.NodeGroup);

        // Ends the document.
        writer.WriteEndDocument();

        writer.Close();
    }

    private static void WriteFieldNodeGroup(XmlWriter writer, FieldNodeGroup fieldNodeGroup)
    {
        // Starts the assembly element.
        writer.WriteStartElement("assembly");

        // Writes the NodeGroupID property.
        writer.WriteAttributeString("id", fieldNodeGroup.NodeGroupID);

        foreach (FieldNode node in fieldNodeGroup.EnumerateFieldNodes())
        {
            // Starts the element.
            writer.WriteStartElement("part");

            // Writes the NodeID attribute.
            writer.WriteAttributeString("id", node.NodeID);

            // Writes the MeshID element.
            writer.WriteElementString("meshid", node.MeshID.ToString());

            // Writes the PhysicsGroupID element.
            writer.WriteElementString("pgid", node.PhysicsGroupID);

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

    /// <summary>
    /// Reads the properties contained in the XML BXDF file specified and returns the corresponding definition.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FieldDefinition ReadXMLProperties(string path)
    {
        return null;
    }
}
