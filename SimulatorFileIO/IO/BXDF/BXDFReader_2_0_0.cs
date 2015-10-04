using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

public partial class BXDFProperties
{
    #region XSD Markup

    /// <summary>
    /// The XSD markup to ensure valid document reading.
    /// </summary>
    private const string BXDF_XSD_2_0_0 =
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

        <xs:attribute name='Version' type='xs:string' fixed='2_0_0'/>
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
    /// Reads the properties contained in the XML BXDF file specified and returns
    /// the corresponding FieldDefinition.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static FieldDefinition ReadProperties_2_0_x(string path)
    {
        // The FieldDefinition to be returned.
        FieldDefinition fieldDefinition = null;

        XmlReaderSettings settings = new XmlReaderSettings();
        settings.Schemas.Add(XmlSchema.Read(new StringReader(BXDF_XSD_2_0_0), null));
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
                            fieldDefinition = FieldDefinition.Factory(new Guid(reader["GUID"]));
                            break;
                        case "PhysicsGroup":
                            // Reads the current element as a PhysicsGroup.
                            ReadPhysicsGroup_2_0_x(reader.ReadSubtree(), fieldDefinition);
                            break;
                        case "NodeGroup":
                            // Reads the root FieldNodeGroup.
                            ReadFieldNodeGroup_2_0_x(reader.ReadSubtree(), fieldDefinition.NodeGroup);
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
    /// Reads the subtree of a PhysicsGroup element.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="fieldDefinition"></param>
    private static void ReadPhysicsGroup_2_0_x(XmlReader reader, FieldDefinition fieldDefinition)
    {
        // Creates a new PhysicsGroup.
        PhysicsGroup physicsGroup = new PhysicsGroup();

        while (reader.Read())
        {
            if (reader.IsStartElement())
            {
                switch (reader.Name)
                {
                    case "PhysicsGroup":
                        // Assigns the ID attribute value to the PhysicsGroupID property.
                        physicsGroup.PhysicsGroupID = reader["ID"];
                        break;
                    case "Collider":
                        // Assings the Collider attribute value to the ColliderType property.
                        physicsGroup.CollisionType =
                            (PhysicsGroupCollisionType)Enum.Parse(typeof(PhysicsGroupCollisionType),
                            reader.ReadElementContentAsString());
                        break;
                    case "Friction":
                        // Assings the Friction attribute value to the Friction property.
                        physicsGroup.Friction = reader.ReadElementContentAsInt();
                        break;
                    case "Mass":
                        // Assings the Mass attribute value to the Mass property.
                        physicsGroup.Mass = reader.ReadElementContentAsInt();
                        break;
                }
            }
        }

        // Adds the PhysicsGroup to the fieldDefinition.
        fieldDefinition.AddPhysicsGroup(physicsGroup);
    }

    /// <summary>
    /// Iterates through each child FieldNodeGroup and adds all FieldNodes to the given FieldNodeGroup.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="fieldNodeGroup"></param>
    private static void ReadFieldNodeGroup_2_0_x(XmlReader reader, FieldNodeGroup fieldNodeGroup)
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

                        // If an ID has not been assigned to the current FieldNodeGroup.
                        if (fieldNodeGroup.NodeGroupID.Equals(BXDFProperties.BXDF_DEFAULT_NAME))
                        {
                            // Assign the ID attribute value to the NodeGroupID property.
                            fieldNodeGroup.NodeGroupID = reader["ID"];
                        }
                        else
                        {
                            // Creates a new FieldNodeGroup.
                            FieldNodeGroup childNodeGroup = new FieldNodeGroup(BXDFProperties.BXDF_DEFAULT_NAME);

                            // Re-iterate as the childNodeGroup.
                            ReadFieldNodeGroup_2_0_x(reader.ReadSubtree(), childNodeGroup);

                            // Add the processed FieldNodeGroup to fieldNodeGroup.
                            fieldNodeGroup.AddNodeGroup(childNodeGroup);
                        }

                        break;
                }
            }
        }
    }
}
