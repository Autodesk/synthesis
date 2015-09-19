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
            writer.WriteElementString("collider", physicsGroup.collisionType.ToString());

            // Writes the friction property for the PhysicsGroup.
            writer.WriteElementString("friction", physicsGroup.friction.ToString());

            // Writes the mass property for the PhysicsGroup.
            writer.WriteElementString("mass", physicsGroup.mass.ToString());

            // Ends the element.
            writer.WriteEndElement();
        }

        
    }

    ///// <summary>
    ///// Writes out the properties file for the node with the base provided to the path provided.
    ///// </summary>
    ///// <param name="path">The output file path</param>
    ///// <param name="fieldDefinition">The field definition to reference</param>
    //public static void WriteProperties(String path, FieldDefinition fieldDefinition)
    //{
    //    // Begin IO.
    //    BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create));

    //    // Writes the format version.
    //    writer.Write(BXDIO.FORMAT_VERSION);

    //    // Writes the name of the field definition.
    //    writer.Write(fieldDefinition.DefinitionID);

    //    Dictionary<string, PhysicsGroup> physicsGroups = fieldDefinition.GetPhysicsGroups();

    //    // Writes PhysicsGroup count.
    //    writer.Write(physicsGroups.Count);
        
    //    for (int i = 0; i < physicsGroups.Count; i++)
    //    {
    //        // The current PhysicsGroup.
    //        PhysicsGroup currentGroup = physicsGroups.ElementAt(i).Value;

    //        // Writes the name/ID of the physics group.
    //        writer.Write(currentGroup.physicsGroupID);

    //        // Writes the collision type of the current PhysicsGroup.
    //        writer.Write((byte)currentGroup.collisionType);

    //        // Writes the friction of the PhysicsGroup.
    //        writer.Write(currentGroup.friction);

    //        // Writes a boolean determining if the PhysicsGroup is dynamic.
    //        writer.Write(currentGroup.dynamic);

    //        // Writes a double representing the PhysicsGroup's mass.
    //        writer.Write(currentGroup.mass);
    //    }

    //    List<FieldNode> nodes = fieldDefinition.GetChildren();

    //    // Writes node count.
    //    writer.Write(nodes.Count);

    //    for (int i = 0; i < nodes.Count; i++)
    //    {
    //        // Writes the name of the current node.
    //        writer.Write(nodes[i].NodeID);

    //        // Writes the name of the parent PhysicsGroup.
    //        writer.Write(nodes[i].PhysicsGroupID);
    //    }
    //    writer.Close();
    //}

    /// <summary>
    /// Reads the properties contained in the XML BXDF file specified and returns the corresponding definition.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FieldDefinition ReadXMLProperties(string path)
    {
        return null;
    }

    ///// <summary>
    ///// Reads the properties contained in the BXDF file specified and returns the corresponding definition.
    ///// </summary>
    ///// <param name="path">The input BXDF file</param>
    ///// <returns>The root node of the skeleton</returns>
    //public static FieldDefinition ReadProperties(string path)
    //{
    //    BinaryReader reader = null;
    //    try
    //    {
    //        reader = new BinaryReader(new FileStream(path, FileMode.Open)); //Throws IOException
    //        // Checks version.
    //        uint version = reader.ReadUInt32();
    //        BXDIO.CheckReadVersion(version);

    //        // Reads definition's ID.
    //        FieldDefinition fieldDefinition = new FieldDefinition(reader.ReadString());

    //        // Reads number of PhysicsGroups.
    //        int groupCount = reader.ReadInt32();

    //        PhysicsGroup[] groups = new PhysicsGroup[groupCount];

    //        for (int i = 0; i < groupCount; i++)
    //        {
    //            // Reads the ID of the current PhysicsGroup.
    //            groups[i].physicsGroupID = reader.ReadString();

    //            // Reads the collision type of the current PhysicsGroup.
    //            groups[i].collisionType = (PhysicsGroupCollisionType)reader.ReadByte();

    //            // Reads the friction value of the current PhysicsGroup.
    //            groups[i].friction = reader.ReadInt32();

    //            // Reads a boolean to determin if the PhysicsGroup is dynamic.
    //            groups[i].dynamic = reader.ReadBoolean();

    //            // Reads the mass value of the current PhysicsGroup.
    //            groups[i].mass = reader.ReadDouble();

    //            fieldDefinition.AddPhysicsGroup(groups[i]);
    //        }

    //        // Reads number of nodes.
    //        int nodeCount = reader.ReadInt32();

    //        FieldNode[] nodes = new FieldNode[nodeCount];

    //        for (int i = 0; i < nodeCount; i++)
    //        {
    //            nodes[i] = new FieldNode(reader.ReadString());//FieldNode.FIELDNODE_FACTORY();

    //            // Reads ID of node.
    //            //nodes[i].NodeID = reader.ReadString();

    //            // Reads ID of node's PhysicsGroup.
    //            nodes[i].PhysicsGroupID = reader.ReadString();

    //            fieldDefinition.AddChild(nodes[i]);
    //        }

    //        // Returns result.
    //        return fieldDefinition;
    //    }
    //    catch
    //    {
    //        return null;
    //    }
    //    finally
    //    {
    //        if (reader != null) reader.Close();
    //    }
    //}
}
