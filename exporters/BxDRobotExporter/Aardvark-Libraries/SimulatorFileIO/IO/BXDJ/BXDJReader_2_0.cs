using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

/// <summary>
/// Utility functions for reading/writing BXDJ files
/// </summary>
public partial class BXDJSkeleton
{
    #region XSD Markup

    /// <summary>
    /// The XSD markup to ensure valid document reading.
    /// </summary>
    private const string BXDJ_XSD_2_0 =
        @"
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
        
        <!-- definition of simple elements -->

        <xs:element name='ParentID' type='xs:integer'/>
        <xs:element name='ModelFileName' type='xs:string'/>
        <xs:element name='ModelID' type='xs:string'/>
        <xs:element name='X' type='xs:decimal'/>
        <xs:element name='Y' type='xs:decimal'/>
        <xs:element name='Z' type='xs:decimal'/>
        <xs:element name='AngularLowLimit' type='xs:decimal'/>
        <xs:element name='AngularHighLimit' type='xs:decimal'/>
        <xs:element name='LinearStartLimit' type='xs:decimal'/>
        <xs:element name='LinearEndLimit' type='xs:decimal'/>
        <xs:element name='LinearLowLimit' type='xs:decimal'/>
        <xs:element name='LinearUpperLimit' type='xs:decimal'/>
        <xs:element name='CurrentLinearPosition' type='xs:decimal'/>
        <xs:element name='CurrentAngularPosition' type='xs:decimal'/>
        <xs:element name='WidthMM' type='xs:decimal'/>
        <xs:element name='PressurePSI' type='xs:decimal'/>
        <xs:element name='WheelRadius' type='xs:decimal'/>
        <xs:element name='WheelWidth' type='xs:decimal'/>
        <xs:element name='ForwardAsympSlip' type='xs:decimal'/>
        <xs:element name='ForwardAsympValue' type='xs:decimal'/>
        <xs:element name='ForwardExtremeSlip' type='xs:decimal'/>
        <xs:element name='ForwardExtremeValue' type='xs:decimal'/>
        <xs:element name='SideAsympSlip' type='xs:decimal'/>
        <xs:element name='SideAsympValue' type='xs:decimal'/>
        <xs:element name='SideExtremeSlip' type='xs:decimal'/>
        <xs:element name='SideExtremeValue' type='xs:decimal'/>
        <xs:element name='IsDriveWheel' type='xs:boolean'/>
        <xs:element name='PortA' type='xs:integer'/>
        <xs:element name='PortB' type='xs:integer'/>
        <xs:element name='LowerLimit' type='xs:float'/>
        <xs:element name='UpperLimit' type='xs:float'/>
        <xs:element name='Coefficient' type='xs:decimal'/>
        <xs:element name='SensorModule' type='xs:integer'/>
        <xs:element name='SensorPort' type='xs:integer'/>
        <xs:element name='UseSecondarySource' type='xs:boolean'/>

        <xs:element name='ElevatorType'>
            <xs:simpleType>
                <xs:restriction base='xs:string'>
                    <xs:enumeration value='NOT_MULTI'/>
                    <xs:enumeration value='CASCADING_STAGE_1'/>
                    <xs:enumeration value='CASCADING_STAGE_2'/>
                    <xs:enumeration value='CONTINUOUS_STAGE_1'/>
                    <xs:enumeration value='CONTINUOUS_STAGE_2'/>
                </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <xs:element name='DriveType'>
            <xs:simpleType>
                <xs:restriction base='xs:string'>
                    <xs:enumeration value='MOTOR'/>
                    <xs:enumeration value='SERVO'/>
                    <xs:enumeration value='WORM_SCREW'/>
                    <xs:enumeration value='BUMPER_PNEUMATIC'/>
                    <xs:enumeration value='RELAY_PNEUMATIC'/>
                    <xs:enumeration value='DUAL_MOTOR'/>
                    <xs:enumeration value='ELEVATOR'/>
                </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <xs:element name='SignalType'>
            <xs:simpleType>
                <xs:restriction base='xs:string'>
                    <xs:enumeration value='CAN'/>
                    <xs:enumeration value='PWM'/>
                </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <xs:element name='WheelType'>
            <xs:simpleType>
                <xs:restriction base='xs:string'>
                    <xs:enumeration value='NOT_A_WHEEL'/>
                    <xs:enumeration value='NORMAL'/>
                    <xs:enumeration value='OMNI'/>
                    <xs:enumeration value='MECANUM'/>
                </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <xs:element name='SensorType'>
            <xs:simpleType>
                <xs:restriction base='xs:string'>
                    <xs:enumeration value='LIMIT'/>
                    <xs:enumeration value='ENCODER'/>
                    <xs:enumeration value='POTENTIOMETER'/>
                    <xs:enumeration value='LIMIT_HALL'/>
                    <xs:enumeration value='ACCELEROMETER'/>
                    <xs:enumeration value='MAGNETOMETER'/>
                    <xs:enumeration value='GYRO'/>
                </xs:restriction>
            </xs:simpleType>
        </xs:element>

        <!-- definition of attributes -->

        <xs:attribute name='GUID' type='xs:string'/>
        <xs:attribute name='VectorID' type='xs:string'/>
        <xs:attribute name='DriverMetaID' type='xs:integer'/>

        <xs:attribute name='Version'>
            <xs:simpleType>
                <xs:restriction base='xs:string'>
                    <xs:pattern value='2\.0\.\d+'/>
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
                <xs:attribute ref='VectorID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='BallJoint'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='BXDVector3'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='CylindricalJoint'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='BXDVector3' minOccurs='2' maxOccurs='2'/>
                    <xs:element ref='AngularLowLimit' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='AngularHighLimit' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='LinearStartLimit' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='LinearEndLimit' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='CurrentLinearPosition'/>
                    <xs:element ref='CurrentAngularPosition'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='LinearJoint'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='BXDVector3' minOccurs='2' maxOccurs='2'/>
                    <xs:element ref='LinearLowLimit' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='LinearUpperLimit' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='CurrentLinearPosition'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='PlanarJoint'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='BXDVector3' minOccurs='2' maxOccurs='2'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='RotationalJoint'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='BXDVector3' minOccurs='2' maxOccurs='2'/>
                    <xs:element ref='AngularLowLimit' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='AngularHighLimit' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='CurrentAngularPosition'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='ElevatorDriverMeta'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='ElevatorType'/>
                </xs:sequence>
                <xs:attribute ref='DriverMetaID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='PneumaticDriverMeta'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='WidthMM'/>
                    <xs:element ref='PressurePSI'/>
                </xs:sequence>
                <xs:attribute ref='DriverMetaID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='WheelDriverMeta'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='WheelType'/>
                    <xs:element ref='WheelRadius'/>
                    <xs:element ref='WheelWidth'/>
                    <xs:element ref='BXDVector3'/>
                    <xs:element ref='ForwardAsympSlip'/>
                    <xs:element ref='ForwardAsympValue'/>
                    <xs:element ref='ForwardExtremeSlip'/>
                    <xs:element ref='ForwardExtremeValue'/>
                    <xs:element ref='SideAsympSlip'/>
                    <xs:element ref='SideAsympValue'/>
                    <xs:element ref='SideExtremeSlip'/>
                    <xs:element ref='SideExtremeValue'/>
                    <xs:element ref='IsDriveWheel'/>
                </xs:sequence>
                <xs:attribute ref='DriverMetaID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='JointDriver'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='DriveType'/>
                    <xs:element ref='PortA'/>
                    <xs:element ref='PortB'/>
                    <xs:element ref='LowerLimit'/>
                    <xs:element ref='UpperLimit'/>
                    <xs:element ref='SignalType'/>
                    <xs:choice maxOccurs='unbounded' minOccurs='0'>
                        <xs:element ref='ElevatorDriverMeta'/>
                        <xs:element ref='PneumaticDriverMeta'/>
                        <xs:element ref='WheelDriverMeta'/>
                    </xs:choice>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='Polynomial'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='Coefficient' maxOccurs='unbounded' minOccurs='0'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='RobotSensor'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='SensorType'/>
                    <xs:element ref='SensorModule'/>
                    <xs:element ref='SensorPort'/>
                    <xs:element ref='Polynomial'/>
                    <xs:element ref='UseSecondarySource'/>
                </xs:sequence>
            </xs:complexType>
        </xs:element>

        <xs:element name='Node'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='ParentID'/>
                    <xs:element ref='ModelFileName'/>
                    <xs:element ref='ModelID'/>
                    <xs:choice maxOccurs='1' minOccurs='0'>
                        <xs:element ref='BallJoint'/>
                        <xs:element ref='CylindricalJoint'/>
                        <xs:element ref='LinearJoint'/>
                        <xs:element ref='PlanarJoint'/>
                        <xs:element ref='RotationalJoint'/>
                    </xs:choice>
                    <xs:element ref='JointDriver' minOccurs='0' maxOccurs='1'/>
                    <xs:element ref='RobotSensor' minOccurs='0' maxOccurs='unbounded'/>
                </xs:sequence>
                <xs:attribute ref='GUID' use='required'/>
            </xs:complexType>
        </xs:element>

        <xs:element name='BXDJ'>
            <xs:complexType>
                <xs:sequence>
                    <xs:element ref='Node' minOccurs='0' maxOccurs='unbounded'/>
                </xs:sequence>
                <xs:attribute ref='Version' use='required'/>
            </xs:complexType>
        </xs:element>
        
        </xs:schema>";

    #endregion

    /// <summary>
    /// Reads the skeleton contained in the XML BXDJ file specified and
    /// returns the corresponding RigidNode_Base.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="useValidation"></param>
    private static RigidNode_Base ReadSkeleton_2_0(string path, bool useValidation = true)
    {
        RigidNode_Base root = null;
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();

        XmlReaderSettings settings = new XmlReaderSettings();

        if (useValidation)
        {
            settings.Schemas.Add(XmlSchema.Read(new StringReader(BXDJ_XSD_2_0), null));
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
                    case "Node":
                        // Reads the current element as a node.
                        ReadNode_2_0(reader.ReadSubtree(), nodes, ref root);
                        break;
                }
            }
        }
        catch (Exception)// A variety of exceptions can take place if the file is invalid, but we will always want to return null.
        {
            // If the file is invalid, return null.
            return null;
        }
        finally
        {
            // Closes the reader.
            reader.Close();
        }

        return nodes[0];
    }

    /// <summary>
    /// Reads a RigidNode_Base with the given reader, list of nodes, and root node reference.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="nodes"></param>
    /// <param name="root"></param>
    private static void ReadNode_2_0(XmlReader reader, List<RigidNode_Base> nodes, ref RigidNode_Base root)
    {
        int parentID = -1;

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "Node":
                    // Adds a new node to the list of RigidNode_Bases.
                    nodes.Add(RigidNode_Base.NODE_FACTORY(new Guid(reader["GUID"])));
                    break;
                case "ParentID":
                    // Stores this ID for later use.
                    parentID = reader.ReadElementContentAsInt();

                    if (parentID == -1) // If this is the root...
                        root = nodes[nodes.Count - 1];
                    break;
                case "ModelFileName":
                    // Assigns the ModelFileName property to the ModelFileName element value.
                    nodes[nodes.Count - 1].ModelFileName = reader.ReadElementContentAsString();
                    break;
                case "ModelID":
                    // Assigns the ModelFullID property to the ModelID element value.
                    nodes[nodes.Count - 1].ModelFullID = reader.ReadElementContentAsString();
                    break;
                case "BallJoint":
                    // Reads the current element as a BallJoint.
                    nodes[parentID].AddChild(ReadBallJoint_2_0(reader.ReadSubtree()), nodes[nodes.Count - 1]);
                    break;
                case "CylindricalJoint":
                    // Reads the current element as a CylindricalJoint.
                    nodes[parentID].AddChild(ReadCylindricalJoint_2_0(reader.ReadSubtree()), nodes[nodes.Count - 1]);
                    break;
                case "LinearJoint":
                    // Reads the current element as a LinearJoint.
                    nodes[parentID].AddChild(ReadLinearJoint_2_0(reader.ReadSubtree()), nodes[nodes.Count - 1]);
                    break;
                case "PlanarJoint":
                    // Reads the current element as a PlanarJoint.
                    nodes[parentID].AddChild(ReadPlanarJoint_2_0(reader.ReadSubtree()), nodes[nodes.Count - 1]);
                    break;
                case "RotationalJoint":
                    // Reads the current elemenet as a RotationalJoint.
                    nodes[parentID].AddChild(ReadRotationalJoint_2_0(reader.ReadSubtree()), nodes[nodes.Count - 1]);
                    break;
                case "JointDriver":
                    // Add a joint driver to the skeletal joint of the current node.
                    nodes[nodes.Count - 1].GetSkeletalJoint().cDriver = ReadJointDriver_2_0(reader.ReadSubtree());
                    break;
                case "RobotSensor":
                    nodes[nodes.Count - 1].GetSkeletalJoint().attachedSensors.Add(ReadRobotSensor_2_0(reader.ReadSubtree()));
                    break;
            }
        }
    }

    /// <summary>
    /// Reads a BallJoint_Base from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static BallJoint_Base ReadBallJoint_2_0(XmlReader reader)
    {
        // Create a new BallJoint_Base.
        BallJoint_Base ballJoint = (BallJoint_Base)SkeletalJoint_Base.JOINT_FACTORY(SkeletalJointType.BALL);

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "BXDVector3":
                    // Read the BXDVector3 as the basePoint.
                    ballJoint.basePoint = ReadBXDVector3_2_0(reader.ReadSubtree());
                    break;
            }
        }

        return ballJoint;
    }

    /// <summary>
    /// Reads a CylindricalJoint_Base from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static CylindricalJoint_Base ReadCylindricalJoint_2_0(XmlReader reader)
    {
        // Create a new CylindricalJoint_Base.
        CylindricalJoint_Base cylindricalJoint = (CylindricalJoint_Base)SkeletalJoint_Base.JOINT_FACTORY(SkeletalJointType.CYLINDRICAL);

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "BXDVector3":
                    switch (reader["VectorID"])
                    {
                        case "BasePoint":
                            // Assign the BXDVector3 to the basePoint.
                            cylindricalJoint.basePoint = ReadBXDVector3_2_0(reader.ReadSubtree());
                            break;
                        case "Axis":
                            // Assign the BXDVector3 to the axis.
                            cylindricalJoint.axis = ReadBXDVector3_2_0(reader.ReadSubtree());
                            break;
                    }
                    break;
                case "AngularLowLimit":
                    // Assign a value to the angularLimitLow.
                    cylindricalJoint.hasAngularLimit = true;
                    cylindricalJoint.angularLimitLow = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "AngularHighLimit":
                    // Assign a value to the angularLimitHigh.
                    cylindricalJoint.angularLimitHigh = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "LinearStartLimit":
                    // Assign a value to the linearLimitStart.
                    cylindricalJoint.hasLinearStartLimit = true;
                    cylindricalJoint.linearLimitStart = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "LinearEndLimit":
                    // Assign a value to the linearLimitEnd.
                    cylindricalJoint.hasLinearEndLimit = true;
                    cylindricalJoint.linearLimitEnd = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "CurrentLinearPosition":
                    // Assign a value to the currentLinearPosition.
                    cylindricalJoint.currentLinearPosition = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "CurrentAngularPosition":
                    // Assign a value to the currentAngularPosition.
                    cylindricalJoint.currentAngularPosition = float.Parse(reader.ReadElementContentAsString());
                    break;
            }
        }

        return cylindricalJoint;
    }

    /// <summary>
    /// Reads a LinearJoint_Base from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static LinearJoint_Base ReadLinearJoint_2_0(XmlReader reader)
    {
        // Create a new LinearJoint_Base.
        LinearJoint_Base linearJoint = (LinearJoint_Base)SkeletalJoint_Base.JOINT_FACTORY(SkeletalJointType.LINEAR);

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "BXDVector3":
                    switch (reader["VectorID"])
                    {
                        case "BasePoint":
                            // Assign the BXDVector3 to the basePoint.
                            linearJoint.basePoint = ReadBXDVector3_2_0(reader.ReadSubtree());
                            break;
                        case "Axis":
                            // Assign the BXDVector3 to the axis.
                            linearJoint.axis = ReadBXDVector3_2_0(reader.ReadSubtree());
                            break;
                    }
                    break;
                case "LinearLowLimit":
                    // Assign a value to the linearLimitLow.
                    linearJoint.hasLowerLimit = true;
                    linearJoint.linearLimitLow = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "LinearUpperLimit":
                    // Assign a value to the linearLimitHigh.
                    linearJoint.hasUpperLimit = true;
                    linearJoint.linearLimitHigh = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "CurrentLinearPosition":
                    // Assign a value to the currentLinearPosition.
                    linearJoint.currentLinearPosition = float.Parse(reader.ReadElementContentAsString());
                    break;
            }
        }

        return linearJoint;
    }

    /// <summary>
    /// Reads a PlanarJoint_Base from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static PlanarJoint_Base ReadPlanarJoint_2_0(XmlReader reader)
    {
        // Create a new PlanarJoint_Base.
        PlanarJoint_Base planarJoint = (PlanarJoint_Base)SkeletalJoint_Base.JOINT_FACTORY(SkeletalJointType.PLANAR);

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "BXDVector3":
                    switch (reader["VectorID"])
                    {
                        case "Normal":
                            // Assign the BXDVector3 to the normal.
                            planarJoint.normal = ReadBXDVector3_2_0(reader.ReadSubtree());
                            break;
                        case "BasePoint":
                            // Assign the BXDVector3 to the basePoint.s
                            planarJoint.basePoint = ReadBXDVector3_2_0(reader.ReadSubtree());
                            break;
                    }
                    break;
            }
        }

        return planarJoint;
    }

    /// <summary>
    /// Reads a RotationalJoint_Base from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static RotationalJoint_Base ReadRotationalJoint_2_0(XmlReader reader)
    {
        // Create a new RotationalJoint_Base.
        RotationalJoint_Base rotationalJoint = (RotationalJoint_Base)SkeletalJoint_Base.JOINT_FACTORY(SkeletalJointType.ROTATIONAL);

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "BXDVector3":
                    switch (reader["VectorID"])
                    {
                        case "BasePoint":
                            // Read the BXDVector3 as the basePoint.
                            rotationalJoint.basePoint = ReadBXDVector3_2_0(reader.ReadSubtree());
                            break;
                        case "Axis":
                            // Read the BXDVector3 as the axis.
                            rotationalJoint.axis = ReadBXDVector3_2_0(reader.ReadSubtree());
                            break;
                    }
                    break;
                case "AngularLowLimit":
                    // Assign the current element value to angularLimitLow.
                    rotationalJoint.hasAngularLimit = true;
                    rotationalJoint.angularLimitLow = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "AngularHighLimit":
                    // Assign the current element value to angularLimitHigh.
                    rotationalJoint.angularLimitHigh = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "CurrentAngularPosition":
                    // Assign the current element value to currentAngularPosition.
                    rotationalJoint.currentAngularPosition = float.Parse(reader.ReadElementContentAsString());
                    break;
            }
        }

        return rotationalJoint;
    }

    /// <summary>
    /// Reads a BXDVector3 with the given XmlReader and returns the reading.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static BXDVector3 ReadBXDVector3_2_0(XmlReader reader)
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
    /// Reads a JointDriver from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static JointDriver ReadJointDriver_2_0(XmlReader reader)
    {
        JointDriver driver = null;

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "DriveType":
                    // Initialize the driver.
                    driver = new JointDriver((JointDriverType)Enum.Parse(typeof(JointDriverType), reader.ReadElementContentAsString()));
                    break;
                case "PortA":
                    // Assign a value to portA.
                    driver.portA = reader.ReadElementContentAsInt();
                    break;
                case "PortB":
                    // Assign a value to portB.
                    driver.portB = reader.ReadElementContentAsInt();
                    break;
                case "LowerLimit":
                    // Assign a value to the lowerLimit.
                    driver.lowerLimit = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "UpperLimit":
                    // Assign a value to the upperLimit.
                    driver.upperLimit = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "SignalType":
                    // Assign a value to isCan.
                    driver.isCan = reader.ReadElementContentAsString().Equals("CAN") ? true : false;
                    break;
                case "ElevatorDriverMeta":
                    // Add an ElevatorDriverMeta.
                    driver.AddInfo(ReadElevatorDriverMeta_2_0(reader.ReadSubtree()));
                    break;
                case "PneumaticDriverMeta":
                    // Add a PneumaticsDriverMeta.
                    driver.AddInfo(ReadPneumaticDriverMeta_2_0(reader.ReadSubtree()));
                    break;
                case "WheelDriverMeta":
                    // Add a WheelDriverMeta.
                    driver.AddInfo(ReadWheelDriverMeta_2_0(reader.ReadSubtree()));
                    break;
            }
        }

        return driver;
    }

    /// <summary>
    /// Reads an ElevatorDriverMeta from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static ElevatorDriverMeta ReadElevatorDriverMeta_2_0(XmlReader reader)
    {
        // Create a new ElevatorDriveMeta.
        ElevatorDriverMeta elevatorDriverMeta = new ElevatorDriverMeta();

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "ElevatorType":
                    // Assign the type to the current element value.
                    elevatorDriverMeta.type = (ElevatorType)Enum.Parse(typeof(ElevatorType), reader.ReadElementContentAsString());
                    break;
            }
        }

        return elevatorDriverMeta;
    }

    /// <summary>
    /// Reads a PneumaticDriverMeta from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static PneumaticDriverMeta ReadPneumaticDriverMeta_2_0(XmlReader reader)
    {
        // Create a new pneumaticDriverMeta.
        PneumaticDriverMeta pneumaticDriverMeta = new PneumaticDriverMeta();

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "WidthMM":
                    // Assign the current element value to widthMM.
                    pneumaticDriverMeta.widthMM = reader.ReadElementContentAsInt();
                    break;
                case "PressurePSI":
                    // Assign the current element value to pressurePSI.
                    pneumaticDriverMeta.pressurePSI = float.Parse(reader.ReadElementContentAsString());
                    break;
            }
        }

        return pneumaticDriverMeta;
    }

    /// <summary>
    /// Reads a WheelDriverMeta from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static WheelDriverMeta ReadWheelDriverMeta_2_0(XmlReader reader)
    {
        // Create new WheelDriveMeta.
        WheelDriverMeta wheelDriverMeta = new WheelDriverMeta();

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "WheelType":
                    // Assign a value to the type.
                    wheelDriverMeta.type = (WheelType)Enum.Parse(typeof(WheelType), reader.ReadElementContentAsString());
                    break;
                case "WheelRadius":
                    // Assign a value to the radius.
                    wheelDriverMeta.radius = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "WheelWidth":
                    // Assign a value to the width.
                    wheelDriverMeta.width = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "BXDVector3":
                    // Assign a value to the center.
                    wheelDriverMeta.center = ReadBXDVector3_2_0(reader.ReadSubtree());
                    break;
                case "ForwardAsympSlip":
                    // Assign a value to the forwardAsympSlip.
                    wheelDriverMeta.forwardAsympSlip = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "ForwardAsympValue":
                    // Assign a value to the forwardAsympValue.
                    wheelDriverMeta.forwardAsympValue = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "ForwardExtremeSlip":
                    // Assign a value to the forwardExtremeSlip.
                    wheelDriverMeta.forwardExtremeSlip = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "ForwardExtremeValue":
                    // Assign a value to the forwardExtremeValue.
                    wheelDriverMeta.forwardExtremeValue = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "SideAsympSlip":
                    // Assign a value to the sideAsympSlip.
                    wheelDriverMeta.sideAsympSlip = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "SideAsympValue":
                    // Assign a value to the sideAsympValue.
                    wheelDriverMeta.sideAsympValue = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "SideExtremeSlip":
                    // Assign a value to the sideExtremeSlip.
                    wheelDriverMeta.sideExtremeSlip = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "SideExtremeValue":
                    // Assign a value to the sideExtremeValue.
                    wheelDriverMeta.sideExtremeValue = float.Parse(reader.ReadElementContentAsString());
                    break;
                case "IsDriveWheel":
                    // Assign a value to isDriveWheel.
                    wheelDriverMeta.isDriveWheel = reader.ReadElementContentAsBoolean();
                    break;
            }
        }

        return wheelDriverMeta;
    }

    /// <summary>
    /// Read a RobotSensor from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static RobotSensor ReadRobotSensor_2_0(XmlReader reader)
    {
        RobotSensor robotSensor = null;

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "SensorType":
                    // Initialize the RobotSensor.
                    robotSensor = new RobotSensor((RobotSensorType)Enum.Parse(typeof(RobotSensorType), reader.ReadElementContentAsString()));
                    break;
                case "SensorModule":
                    // Assign a value to the module.
                    robotSensor.module = (short)reader.ReadElementContentAsInt();
                    break;
                case "SensorPort":
                    // Assign a value to the port.
                    robotSensor.port = (short)reader.ReadElementContentAsInt();
                    break;
                case "Polynomial":
                    // Create a polynomial and assign it to the equation.
                    robotSensor.equation = ReadPolynomial_2_0(reader.ReadSubtree());
                    break;
                case "UseSecondarySource":
                    // Assign a value to useSecondarySource.
                    robotSensor.useSecondarySource = reader.ReadElementContentAsBoolean();
                    break;
            }
        }

        return robotSensor;
    }

    /// <summary>
    /// Reads a Polynomial from the given XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static Polynomial ReadPolynomial_2_0(XmlReader reader)
    {
        // Initialize a list of floats.
        List<float> coeff = new List<float>();

        foreach (string name in IOUtilities.AllElements(reader))
        {
            switch (name)
            {
                case "Coefficient":
                    // Add a new coefficient to the list of floats.
                    coeff.Add(float.Parse(reader.ReadElementContentAsString()));
                    break;
            }
        }

        // Convert the list of floats to a Polynomial.
        return new Polynomial(coeff.ToArray());
    }
}
