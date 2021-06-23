using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

public partial class BXDFProperties
{
    #region XSD Markup

    /// <summary>
    /// The XSD markup to ensure valid document reading.
    /// </summary>
    private const string BXDF_XSD_2_2 =
        @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>

        <!-- definition of simple elements -->

        <xs:element name='X' type='xs:decimal'/>
        <xs:element name='Y' type='xs:decimal'/>
        <xs:element name='Z' type='xs:decimal'/>
        <xs:element name='W' type='xs:decimal'/>
        <xs:element name='Mass' type='xs:decimal'/>
        <xs:element name='SubMeshID' type='xs:integer'/>
        <xs:element name='CollisionMeshID' type='xs:integer'/>
        <xs:element name='PropertySetID' type='xs:string'/>
        <xs:element name='Scale' type='xs:decimal'/>
        <xs:element name='Convex' type='xs:boolean'/>

        <xs:element name='Friction'>
            <xs:simpleType>
                <xs:restriction base='xs:decimal'>
                    <xs:minInclusive value='0'/>
                    <xs:maxInclusive value='100'/>
                </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <!-- definition of attributes -->

        <xs:attribute name='GUID' type='xs:string'/>
        <xs:attribute name='ID' type='xs:string'/>

        <xs:attribute name='Version'>
            <xs:simpleType>
                <xs:restriction base='xs:string'>
                    <xs:pattern value='2\.2\.\d+'/>
                </xs:restriction>
            </xs:simpleType>
        </xs:attribute>

        <!-- definition of complex elements -->

        <xs:element name='BXDVector3'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='X'/>
                    <xs:element ref='Y'/>
                    <xs:element ref='Z'/>
                </xs:sequence>
                <xs:attribute ref='ID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='BXDQuaternion'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='X'/>
                    <xs:element ref='Y'/>
                    <xs:element ref='Z'/>
                    <xs:element ref='W'/>
                </xs:sequence>
                <xs:attribute ref='ID' use='required'/>
            </xs:complexType>
        </xs:element>

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

        <xs:element name='BoxCollider'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='BXDVector3'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='SphereCollider'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='Scale'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='MeshCollider'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='Convex'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='PropertySet'>
            <xs:complexType>
                <xs:sequence>
                    <xs:choice minOccurs='0' maxOccurs='1'>
                        <xs:element ref='BoxCollider'/>
                        <xs:element ref='SphereCollider'/>
                        <xs:element ref='MeshCollider'/>
                    </xs:choice>
                    <xs:element ref='Friction'/>
                    <xs:element ref='Mass'/>
                </xs:sequence>
                <xs:attribute ref='ID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='Node'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='BXDVector3'/>
                    <xs:element ref='BXDQuaternion'/>
                    <xs:element ref='SubMeshID'/>
                    <xs:element ref='CollisionMeshID' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='PropertySetID' minOccurs='0' maxOccurs='1'/>
                </xs:sequence>
                <xs:attribute ref='ID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='BXDF'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='PropertySet' minOccurs='0' maxOccurs='unbounded'/>
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
    /// <param name="useValidation"></param>
    /// <returns></returns>
    private static FieldDefinition ReadProperties_2_2(string path, out string result, bool useValidation = true)
    {
        // The FieldDefinition to be returned.
        FieldDefinition fieldDefinition = null;

        XmlReaderSettings settings = new XmlReaderSettings();

        if (useValidation)
        {
            settings.Schemas.Add(XmlSchema.Read(new StringReader(BXDF_XSD_2_2), null));
            settings.ValidationType = ValidationType.Schema;
        }
        else
        {
            settings.ValidationType = ValidationType.None;
        }

        XmlReader reader = XmlReader.Create(path, settings);

        try
        {
            foreach (string name in IOUtilities.AllElements(reader))
            {
                switch (name)
                {
                    case "BXDF":
                        // Assign a value to fieldDefinition with the given GUID attribute.
                        fieldDefinition = FieldDefinition.Factory(new Guid(reader["GUID"]));
                        break;
                    case "PropertySet":
                        // Reads the current element as a PropertySet.
                        ReadPropertySet_2_2(reader.ReadSubtree(), fieldDefinition);
                        break;
                    case "NodeGroup":
                        // Reads the root FieldNodeGroup.
                        ReadFieldNodeGroup_2_2(reader.ReadSubtree(), fieldDefinition.NodeGroup);
                        break;
                }
            }

            result = "Success.";

            return fieldDefinition;
        }
        catch (Exception e)// A variety of exceptions can take place if the file is invalid, but we will always want to return null.
        {
            result = e.Message;

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
    /// Reads a BXDVector3 with the given XmlReader and returns the reading.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static BXDVector3 ReadBXDVector3_2_2(XmlReader reader)
    {
        BXDVector3 vec = new BXDVector3();

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "X":
                    // Assign the x value.
                    vec.x = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "Y":
                    // Assign the y value.
                    vec.y = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "Z":
                    // Assign the z value.
                    vec.z = float.Parse(reader.ReadElementContentAsString());
                    break;
            }
        }

        return vec;
    }

    /// <summary>
    /// Reads a BXDQuaternion with the given XmlReader and returns the reading.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static BXDQuaternion ReadBXDQuaternion_2_2(XmlReader reader)
    {
        BXDQuaternion quat = new BXDQuaternion();

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "X":
                    // Assign the X value.
                    quat.X = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "Y":
                    // Assign the Y value.
                    quat.Y = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "Z":
                    // Assign the Z value.
                    quat.Z = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "W":
                    // Assign the W value.
                    quat.W = float.Parse(reader.ReadElementContentAsString());
                    break;
            }
        }

        return quat;
    }

    /// <summary>
    /// Reads a BoxCollider with the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static PropertySet.BoxCollider ReadBoxCollider_2_2(XmlReader reader)
    {
        // Create the BoxCollider.
        PropertySet.BoxCollider boxCollider = null;

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "BXDVector3":
                    // Reads the scale properties and initializes the BoxCollider.
                    boxCollider = new PropertySet.BoxCollider(ReadBXDVector3_2_2(reader.ReadSubtree()));
                    break;
            }
        }

        return boxCollider;
    }

    /// <summary>
    /// Reads a SphereCollider with the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static PropertySet.SphereCollider ReadSphereCollider_2_2(XmlReader reader)
    {
        // Create the SphereCollider.
        PropertySet.SphereCollider sphereCollider = null;

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "Scale":
                    // Reads the scale property and initializes the SphereCollider.
                    sphereCollider = new PropertySet.SphereCollider(float.Parse(reader.ReadElementContentAsString()));
                    break;
            }
        }

        return sphereCollider;
    }

    /// <summary>
    /// Reads a MeshCollider with the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static PropertySet.MeshCollider ReadMeshCollider_2_2(XmlReader reader)
    {
        // Create the MeshCollider.
        PropertySet.MeshCollider meshCollider = null;

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "Convex":
                    // Reads the convex property and initializes the MeshCollider.
                    meshCollider = new PropertySet.MeshCollider(reader.ReadElementContentAsBoolean());
                    break;
            }
        }

        return meshCollider;
    }

    /// <summary>
    /// Reads the subtree of a PropertySet element.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="fieldDefinition"></param>
    private static void ReadPropertySet_2_2(XmlReader reader, FieldDefinition fieldDefinition)
    {
        // Creates a new PropertySet.
        PropertySet propertySet = new PropertySet();

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "PropertySet":
                    // Assigns the ID attribute value to the PropertySetID property.
                    propertySet.PropertySetID = reader["ID"];
                    break;
                case "BoxCollider":
                    // Assigns the BoxCollider read by the XmlReader to the PropertySet's Collider property.
                    propertySet.Collider = ReadBoxCollider_2_2(reader.ReadSubtree());
                    break;
                case "SphereCollider":
                    // Assigns the SphereCollider read by the XmlReader to the PropertySet's Collider property.
                    propertySet.Collider = ReadSphereCollider_2_2(reader.ReadSubtree());
                    break;
                case "MeshCollider":
                    // Assigns the MeshCollider read by the XmlReader to the PropertySet's Collider property.
                    propertySet.Collider = ReadMeshCollider_2_2(reader.ReadSubtree());
                    break;
                case "Friction":
                    // Assings the Friction attribute value to the Friction property.
                    propertySet.Friction = reader.ReadElementContentAsInt();
                    break;
                case "Mass":
                    // Assings the Mass attribute value to the Mass property.
                    propertySet.Mass = float.Parse(reader.ReadElementContentAsString());
                    break;
            }
        }

        // Adds the PropertySet to the fieldDefinition.
        fieldDefinition.AddPropertySet(propertySet);
    }

    /// <summary>
    /// Reads the subtree of a FieldNode and returns the result.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static FieldNode ReadFieldNode_2_2(XmlReader reader)
    {
        FieldNode node = null;

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "Node":
                    node = new FieldNode(reader["ID"]);
                    break;
                case "BXDVector3":
                    // Read the BXDVector3 as the node's position.
                    node.Position = ReadBXDVector3_2_2(reader.ReadSubtree());
                    break;
                case "BXDQuaternion":
                    // Read the BXDVector3 as the node's rotation.
                    node.Rotation = ReadBXDQuaternion_2_2(reader.ReadSubtree());
                    break;
                case "SubMeshID":
                    // Assign the MeshID attribute value to the SubMeshID property.
                    node.SubMeshID = reader.ReadElementContentAsInt();
                    break;
                case "CollisionMeshID":
                    // Assign the CollisionMeshID attribute value to the CollisionMeshID property.
                    node.CollisionMeshID = reader.ReadElementContentAsInt();
                    break;
                case "PropertySetID":
                    // Assign the PropertySetID attribute value to the PropertySetID property.
                    node.PropertySetID = reader.ReadElementContentAsString();
                    break;
            }
        }

        return node;
    }

    /// <summary>
    /// Iterates through each child FieldNodeGroup and adds all FieldNodes to the given FieldNodeGroup.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="fieldNodeGroup"></param>
    private static void ReadFieldNodeGroup_2_2(XmlReader reader, FieldNodeGroup fieldNodeGroup)
    {
        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "Node":
                    // Add the FieldNode to fieldNodeGroup.
                    fieldNodeGroup.AddNode(ReadFieldNode_2_2(reader.ReadSubtree()));
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
                        ReadFieldNodeGroup_2_2(reader.ReadSubtree(), childNodeGroup);

                        // Add the processed FieldNodeGroup to fieldNodeGroup.
                        fieldNodeGroup.AddNodeGroup(childNodeGroup);
                    }
                    break;
            }
        }
    }
}
