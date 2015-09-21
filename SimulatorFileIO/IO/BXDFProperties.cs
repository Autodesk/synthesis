using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
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

        <xs:element name='Mass' type='xs:nonNegativeInteger'/>
        <xs:element name='MeshID' type='xs:nonNegativeInteger'/>
        <xs:element name='PhysicsGroupID' type='xs:string'/>

        <xs:element name='Collider'>
            <xs:simpleType>
            <xs:restriction base='xs:string'>
                <xs:enumeration value='MESH'/>
                <xs:enumeration value='BOX'/>
            </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <xs:element name='Friction'>
            <xs:simpleType>
            <xs:restriction base='xs:integer'>
                <xs:minInclusive value='0'/>
                <xs:maxInclusive value='10'/>
            </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <!-- definition of attributes -->

        <xs:attribute name='Version' type='xs:nonNegativeInteger' fixed='0'/>
        <xs:attribute name='GUID' type='xs:string'/>
        <xs:attribute name='ID' type='xs:string'/>

        <!-- definition of complex elements -->

        <xs:element name='NodeGroup'>
            <xs:complexType>
            <xs:sequence>
                <xs:choice minOccurs='0' maxOccurs='unbounded'>
                <xs:element ref='NodeGroup'/>
                <xs:element ref='Node'/>
                </xs:choice>
            </xs:sequence>
            <xs:attribute ref='ID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='PhysicsGroup'>
            <xs:complexType>
            <xs:sequence>
                <xs:element ref='Collider'/>
                <xs:element ref='Friction'/>
                <xs:element ref='Mass'/>
            </xs:sequence>
            <xs:attribute ref='ID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='Node'>
            <xs:complexType>
            <xs:sequence>
                <xs:element ref='MeshID'/>
                <xs:element ref='PhysicsGroupID'/>
            </xs:sequence>
            <xs:attribute ref='ID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='BXDF'>
            <xs:complexType>
            <xs:sequence>
                <xs:element ref='PhysicsGroup' minOccurs='0' maxOccurs='unbounded'/>
                <xs:element ref='NodeGroup'/>
            </xs:sequence>
            <xs:attribute ref='Version' use='required'/>
            <xs:attribute ref='GUID' use='required'/>
            </xs:complexType>
        </xs:element>

        </xs:schema>";

    #endregion

    /// <summary>
    /// Represents the current version of the BXDF file.
    /// </summary>
    private const int VERSION = 0;

    /// <summary>
    /// Writes out the properties file in XML format for the node with the base provided to
    /// the path provided.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fieldDefinition"></param>
    public static void WriteXMLProperties(string path, FieldDefinition fieldDefinition)
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;

        XmlWriter writer = XmlWriter.Create(path, settings);

        // Begins the document.
        writer.WriteStartDocument();

        /// Writes the root element and its GUID.
        writer.WriteStartElement("BXDF");
        writer.WriteAttributeString("Version", VERSION.ToString());
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
    /// Reads the properties contained in the XML BXDF file specified and returns
    /// the corresponding FieldDefinition.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FieldDefinition ReadXMLProperties(string path)
    {
        // The FieldDefinition to be returned.
        FieldDefinition fieldDefinition = null;

        XmlReaderSettings settings = new XmlReaderSettings();
        settings.Schemas.Add(XmlSchema.Read(new StringReader(xsdMarkup), null));
        settings.ValidationType = ValidationType.Schema;

        XmlReader reader = XmlReader.Create(path, settings);

        try
        {
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "BXDF":

                            // Assign a value to fieldDefinition with the given GUID attribute.
                            fieldDefinition = new FieldDefinition(new Guid(reader["GUID"]));

                            break;
                        case "PhysicsGroup":

                            // Creates a new PhysicsGroup.
                            PhysicsGroup physicsGroup = new PhysicsGroup();

                            // Assigns the ID attribute value to the PhysicsGroupID property.
                            physicsGroup.PhysicsGroupID = reader["ID"];
                            
                            // Assings the Collider attribute value to the ColliderType property.
                            reader.ReadToFollowing("Collider");
                            physicsGroup.CollisionType =
                                (PhysicsGroupCollisionType)Enum.Parse(typeof(PhysicsGroupCollisionType),
                                reader.ReadElementContentAsString());

                            // Assings the Friction attribute value to the Friction property.
                            reader.ReadToFollowing("Friction");
                            physicsGroup.Friction = reader.ReadElementContentAsInt();

                            // Assings the Mass attribute value to the Mass property.
                            reader.ReadToFollowing("Mass");
                            physicsGroup.Mass = reader.ReadElementContentAsInt();

                            // Adds the PhysicsGroup to fieldDefinition.
                            fieldDefinition.AddPhysicsGroup(physicsGroup);

                            break;
                        case "NodeGroup":

                            // Assigns the ID attribute value to the NodeGroup.NodeGroupID property.
                            fieldDefinition.NodeGroup.NodeGroupID = reader["ID"];

                            // Reads the root FieldNodeGroup.
                            ReadFieldNodeGroup(reader.ReadSubtree(), fieldDefinition.NodeGroup);

                            break;
                    }
                }
            }

            return fieldDefinition;
        }
        catch (XmlSchemaValidationException)
        {
            // If the file is invalid, return null.
            return null;
        }
        finally
        {
            // Closes the reader.
            reader.Close();
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

    /// <summary>
    /// Iterates through each child FieldNodeGroup and adds all FieldNodes to the given FieldNodeGroup.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="fieldNodeGroup"></param>
    private static void ReadFieldNodeGroup(XmlReader reader, FieldNodeGroup fieldNodeGroup)
    {
        while (reader.Read())
        {
            if (reader.IsStartElement())
            {
                switch (reader.Name)
                {
                    case "Node":

                        // Create a new FieldNode.
                        FieldNode node = new FieldNode(reader["ID"]);

                        // Assign the MeshID attribute value to the MeshID property.
                        reader.ReadToFollowing("MeshID");
                        node.MeshID = reader.ReadElementContentAsInt();

                        // Assign the PhysicsGroupID attribute value to the PhysicsGroupID property.
                        reader.ReadToFollowing("PhysicsGroupID");
                        node.PhysicsGroupID = reader.ReadElementContentAsString();

                        // Add the FieldNode to fieldNodeGroup.
                        fieldNodeGroup.AddNode(node);

                        break;
                    case "NodeGroup":

                        // If we aren't re-reading the parent FieldNodeGroup...
                        if (reader["ID"] != fieldNodeGroup.NodeGroupID)
                        {
                            // Create a new FieldNodeGroup.
                            FieldNodeGroup childNodeGroup = new FieldNodeGroup(reader["ID"]);

                            // Re-iterate as the childNodeGroup.
                            ReadFieldNodeGroup(reader.ReadSubtree(), childNodeGroup);

                            // Add the processed FieldNodeGroup to fieldNodeGroup.
                            fieldNodeGroup.AddNodeGroup(childNodeGroup);
                        }

                        break;
                }
            }
        }
    }
}
