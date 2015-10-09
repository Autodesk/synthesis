using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

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
}