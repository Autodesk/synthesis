using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

/// <summary>
/// Utility functions for reading/writing BXDJ files
/// </summary>
public static partial class BXDJSkeleton
{
    /// <summary>
    /// Represents the current version of the BXDA file.
    /// </summary>
    public const string BXDJ_CURRENT_VERSION = "2.0.0";

    /// <summary>
    /// Ensures that every node is assigned a model file name by assigning all nodes without a file name a generated name.
    /// </summary>
    /// <param name="baseNode">The base node of the skeleton</param>
    /// <param name="overwrite">Overwrite existing</param>
    public static void SetupFileNames(RigidNode_Base baseNode, bool overwrite = false)
    {
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].ModelFileName == null || overwrite) 
                nodes[i].ModelFileName = ("node_" + i + ".bxda");
        }
    }

    /// <summary>
    /// Writes out the skeleton file for the skeleton with the base provided to the path provided.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="baseNode"></param>
    public static void WriteSkeleton(string path, RigidNode_Base baseNode)
    {
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        // Determine the parent ID for each node in the list.
        int[] parentID = new int[nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].GetParent() != null)
            {
                parentID[i] = nodes.IndexOf(nodes[i].GetParent());

                if (parentID[i] < 0) throw new Exception("Can't resolve parent ID for " + nodes[i].ToString());
            }
            else
            {
                parentID[i] = -1;
            }
        }

        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;

        XmlWriter writer = XmlWriter.Create(path, settings);

        writer.WriteStartDocument();

        writer.WriteStartElement("BXDJ");
        writer.WriteAttributeString("Version", BXDJ_CURRENT_VERSION);

        for (int i = 0; i < nodes.Count; i++)
        {
            writer.WriteStartElement("Node");

            writer.WriteAttributeString("GUID", nodes[i].GUID.ToString());

            writer.WriteElementString("ParentID", parentID[i].ToString());

            nodes[i].ModelFileName = FileUtilities.SanatizeFileName("node_" + i + ".bxda");

            writer.WriteElementString("ModelFileName", nodes[i].ModelFileName);

            writer.WriteElementString("ModelID", nodes[i].GetModelID());

            if (parentID[i] >= 0)
                WriteJoint(nodes[i].GetSkeletalJoint(), writer);

            writer.WriteEndElement();
        }

        writer.WriteEndDocument();

        writer.Close();
    }

    /// <summary>
    /// Writes the backing information and ID for this joint with the given XmlWriter.
    /// </summary>
    /// <param name="joint"></param>
    /// <param name="writer"></param>
    private static void WriteJoint(SkeletalJoint_Base joint, XmlWriter writer)
    {
        switch (joint.GetJointType())
        {
            case SkeletalJointType.BALL:
                WriteBallJoint((BallJoint_Base)joint, writer);
                break;
            case SkeletalJointType.CYLINDRICAL:
                WriteCylindricalJoint((CylindricalJoint_Base)joint, writer);
                break;
            case SkeletalJointType.LINEAR:
                WriteLinearJoint((LinearJoint_Base)joint, writer);
                break;
            case SkeletalJointType.PLANAR:
                WritePlanarJoint((PlanarJoint_Base)joint, writer);
                break;
            case SkeletalJointType.ROTATIONAL:
                WriteRotationalJoint((RotationalJoint_Base)joint, writer);
                break;
            default:
                throw new Exception("Could not determine type of joint");
        }

        if (joint.cDriver != null)
            WriteJointDriver(joint.cDriver, writer);

        for (int i = 0; i < joint.attachedSensors.Count; i++)
        {
            WriteRobotSensor(joint.attachedSensors[i], writer);
        }
    }

    /// <summary>
    /// Used for writing the data of a BallJoint_Base.
    /// </summary>
    /// <param name="joint"></param>
    /// <param name="writer"></param>
    private static void WriteBallJoint(BallJoint_Base joint, XmlWriter writer)
    {
        writer.WriteStartElement("BallJoint");

        WriteBXDVector3(joint.basePoint, writer, "BasePoint");

        writer.WriteEndElement();
    }

    /// <summary>
    /// Used for writing the data of a CylindricalJoint_Base.
    /// </summary>
    /// <param name="joint"></param>
    /// <param name="writer"></param>
    private static void WriteCylindricalJoint(CylindricalJoint_Base joint, XmlWriter writer)
    {
        writer.WriteStartElement("CylindricalJoint");

        joint.EnforceOrder();

        WriteBXDVector3(joint.basePoint, writer, "BasePoint");
        WriteBXDVector3(joint.axis, writer, "Axis");

        if (joint.hasAngularLimit)
        {
            writer.WriteElementString("AngularLowLimit", joint.angularLimitLow.ToString("F4"));
            writer.WriteElementString("AngularHighLimit", joint.angularLimitHigh.ToString("F4"));
        }

        if (joint.hasLinearStartLimit)
            writer.WriteElementString("LinearStartLimit", joint.linearLimitStart.ToString("F4"));

        if (joint.hasLinearEndLimit)
            writer.WriteElementString("LinearEndLimit", joint.linearLimitEnd.ToString("F4"));

        writer.WriteElementString("CurrentLinearPosition", joint.currentLinearPosition.ToString("F4"));
        writer.WriteElementString("CurrentAngularPosition", joint.currentAngularPosition.ToString("F4"));

        writer.WriteEndElement();
    }

    /// <summary>
    /// Used for writing the data of a LinearJoint_Base.
    /// </summary>
    /// <param name="joint"></param>
    /// <param name="writer"></param>
    private static void WriteLinearJoint(LinearJoint_Base joint, XmlWriter writer)
    {
        writer.WriteStartElement("LinearJoint");

        joint.EnforceOrder();

        WriteBXDVector3(joint.basePoint, writer, "BasePoint");
        WriteBXDVector3(joint.axis, writer, "Axis");

        if (joint.hasLowerLimit)
            writer.WriteElementString("LinearLowLimit", joint.linearLimitLow.ToString("F4"));

        if (joint.hasUpperLimit)
            writer.WriteElementString("LinearUpperLimit", joint.linearLimitHigh.ToString("F4"));

        writer.WriteElementString("CurrentLinearPosition", joint.currentLinearPosition.ToString("F4"));

        writer.WriteEndElement();
    }

    /// <summary>
    /// Used for writing the data of a PlanarJoint_Base.
    /// </summary>
    /// <param name="joint"></param>
    /// <param name="writer"></param>
    private static void WritePlanarJoint(PlanarJoint_Base joint, XmlWriter writer)
    {
        writer.WriteStartElement("PlanarJoint");

        WriteBXDVector3(joint.normal, writer, "Normal");
        WriteBXDVector3(joint.basePoint, writer, "BasePoint");

        writer.WriteEndElement();
    }
    
    /// <summary>
    /// Used for writing the data of a RotationalJoint_Base.
    /// </summary>
    /// <param name="joint"></param>
    /// <param name="writer"></param>
    private static void WriteRotationalJoint(RotationalJoint_Base joint, XmlWriter writer)
    {
        writer.WriteStartElement("RotationalJoint");

        joint.EnforceOrder();

        WriteBXDVector3(joint.basePoint, writer, "BasePoint");
        WriteBXDVector3(joint.axis, writer, "Axis");

        if (joint.hasAngularLimit)
        {
            writer.WriteElementString("AngularLowLimit", joint.angularLimitLow.ToString("F4"));
            writer.WriteElementString("AngularHighLimit", joint.angularLimitHigh.ToString("F4"));
        }

        writer.WriteElementString("CurrentAngularPosition", joint.currentAngularPosition.ToString("F4"));

        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes the BXDVector3 to an XML file with the given XmlWriter.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="writer"></param>
    private static void WriteBXDVector3(BXDVector3 vec, XmlWriter writer, string id)
    {
        writer.WriteStartElement("BXDVector3");

        writer.WriteAttributeString("VectorID", id);

        writer.WriteElementString("X", vec.x.ToString("F4"));
        writer.WriteElementString("Y", vec.y.ToString("F4"));
        writer.WriteElementString("Z", vec.z.ToString("F4"));

        writer.WriteEndElement();
    }

    /// <summary>
    /// Used for writing joint driver data.
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="writer"></param>
    private static void WriteJointDriver(JointDriver driver, XmlWriter writer)
    {
        writer.WriteStartElement("JointDriver");

        writer.WriteElementString("DriveType", driver.GetDriveType().ToString());
        writer.WriteElementString("PortA", driver.portA.ToString());
        writer.WriteElementString("PortB", driver.portB.ToString());
        writer.WriteElementString("LowerLimit", driver.lowerLimit.ToString("F4"));
        writer.WriteElementString("UpperLimit", driver.upperLimit.ToString("F4"));
        writer.WriteElementString("SignalType", driver.isCan ? "CAN" : "PWM");
        
        foreach (JointDriverMeta meta in driver.MetaInfo.Values)
        {
            WriteJointDriverMeta(meta, writer);
        }
        
        writer.WriteEndElement();
    }

    /// <summary>
    /// Used for writing joint driver meta data.
    /// </summary>
    /// <param name="meta"></param>
    /// <param name="writer"></param>
    private static void WriteJointDriverMeta(JointDriverMeta meta, XmlWriter writer)
    {
        switch (meta.GetType().ToString())
        {
            case "ElevatorDriverMeta":
                WriteElevatorDriverMeta((ElevatorDriverMeta)meta, writer);
                break;
            case "PneumaticDriverMeta":
                WritePneumaticDriverMeta((PneumaticDriverMeta)meta, writer);
                break;
            case "WheelDriverMeta":
                WriteWheelDriverMeta((WheelDriverMeta)meta, writer);
                break;
        }
    }

    /// <summary>
    /// Used for writing elevator driver meta data.
    /// </summary>
    /// <param name="meta"></param>
    /// <param name="writer"></param>
    private static void WriteElevatorDriverMeta(ElevatorDriverMeta meta, XmlWriter writer)
    {
        writer.WriteStartElement("ElevatorDriverMeta");

        writer.WriteAttributeString("DriverMetaID", meta.GetID().ToString());

        writer.WriteElementString("ElevatorType", meta.type.ToString());

        writer.WriteEndElement();
    }

    /// <summary>
    /// Used for writing pneumatic driver meta data.
    /// </summary>
    /// <param name="meta"></param>
    /// <param name="writer"></param>
    private static void WritePneumaticDriverMeta(PneumaticDriverMeta meta, XmlWriter writer)
    {
        writer.WriteStartElement("PneumaticDriverMeta");

        writer.WriteAttributeString("DriverMetaID", meta.GetID().ToString());

        writer.WriteElementString("WidthMM", meta.widthMM.ToString("F4"));
        writer.WriteElementString("PressurePSI", meta.pressurePSI.ToString("F4"));

        writer.WriteEndElement();
    }

    /// <summary>
    /// Used for writing wheel driver meta data.
    /// </summary>
    /// <param name="meta"></param>
    /// <param name="writer"></param>
    private static void WriteWheelDriverMeta(WheelDriverMeta meta, XmlWriter writer)
    {
        writer.WriteStartElement("WheelDriverMeta");

        writer.WriteAttributeString("DriverMetaID", meta.GetID().ToString());

        writer.WriteElementString("WheelType", meta.type.ToString());
        writer.WriteElementString("WheelRadius", meta.radius.ToString("F4"));
        writer.WriteElementString("WheelWidth", meta.width.ToString("F4"));

        WriteBXDVector3(meta.center, writer, "WheelCenter");

        writer.WriteElementString("ForwardAsympSlip", meta.forwardAsympSlip.ToString("F4"));
        writer.WriteElementString("ForwardAsympValue", meta.forwardAsympValue.ToString("F4"));
        writer.WriteElementString("ForwardExtremeSlip", meta.forwardExtremeSlip.ToString("F4"));
        writer.WriteElementString("ForwardExtremeValue", meta.forwardExtremeValue.ToString("F4"));
        writer.WriteElementString("SideAsympSlip", meta.sideAsympSlip.ToString("F4"));
        writer.WriteElementString("SideAsympValue", meta.sideAsympValue.ToString("F4"));
        writer.WriteElementString("SideExtremeSlip", meta.sideExtremeSlip.ToString("F4"));
        writer.WriteElementString("SideExtremeValue", meta.sideExtremeValue.ToString("F4"));
        writer.WriteElementString("IsDriveWheel", meta.isDriveWheel.ToString().ToLower());

        writer.WriteEndElement();
    }

    /// <summary>
    /// Used for writing the data from a RobotSensor.
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="writer"></param>
    private static void WriteRobotSensor(RobotSensor sensor, XmlWriter writer)
    {
        writer.WriteStartElement("RobotSensor");

        writer.WriteElementString("SensorType", sensor.type.ToString());
        writer.WriteElementString("SensorModule", sensor.module.ToString());
        writer.WriteElementString("SensorPort", sensor.port.ToString());
        WritePolynomial(sensor.equation, writer);
        writer.WriteElementString("UseSecondarySource", sensor.useSecondarySource.ToString().ToLower());

        writer.WriteEndElement();
    }

    /// <summary>
    /// Used for writing a Polynomial's data.
    /// </summary>
    /// <param name="poly"></param>
    /// <param name="writer"></param>
    public static void WritePolynomial(Polynomial poly, XmlWriter writer)
    {
        writer.WriteStartElement("Polynomial");

        for (int i = 0; i < poly.coeff.Length; i++)
        {
            writer.WriteElementString("Coefficient", poly.coeff[i].ToString("F4"));
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Reads the skeleton contained in the BXDJ XML file specified and returns the root node for that skeleton.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static RigidNode_Base ReadSkeleton(string path)
    {
        XmlReader reader = XmlReader.Create(path);

        if (reader.ReadToFollowing("BXDJ"))
        {
            string version = reader["Version"];

            switch (version.Substring(0, version.LastIndexOf('.')))
            {
                case "2.0":
                    return ReadSkeleton_2_0(path);
                default: // If version is unknown.
                    // Attempt to read with the most recent version (but without validation).
                    return ReadSkeleton_2_0(path, false);
            }
        }
        else
        {
            // Could not find element, so return null.
            return null;
        }
    }

    /// <summary>
    /// Reads the skeleton contained in the BXDJ file specified and returns the root node for that skeleton.
    /// </summary>
    /// <param name="path">The input BXDJ file</param>
    /// <returns>The root node of the skeleton</returns>
    public static RigidNode_Base ReadBinarySkeleton(string path)
    {
        BinaryReader reader = null;
        try
        {
            reader = new BinaryReader(new FileStream(path, FileMode.Open)); //Throws IOException
            // Sanity check
            uint version = reader.ReadUInt32();
            BXDIO.CheckReadVersion(version); //Throws FormatException

            int nodeCount = reader.ReadInt32();
            if (nodeCount <= 0) throw new Exception("This appears to be an empty skeleton");

            RigidNode_Base root = null;
            RigidNode_Base[] nodes = new RigidNode_Base[nodeCount];

            for (int i = 0; i < nodeCount; i++)
            {
                //nodes[i] = RigidNode_Base.NODE_FACTORY();

                int parent = reader.ReadInt32();
                nodes[i].ModelFileName = (reader.ReadString());
                nodes[i].ModelFullID = (reader.ReadString());

                if (parent != -1)
                {
                    SkeletalJoint_Base joint = SkeletalJoint_Base.ReadJointFully(reader);
                    nodes[parent].AddChild(joint, nodes[i]);
                }
                else
                {
                    root = nodes[i];
                }
            }

            if (root == null)
            {
                throw new Exception("This skeleton has no known base.  \"" + path + "\" is probably corrupted.");
            }

            return root;
        }
        catch (FormatException fe)
        {
            Console.WriteLine("File version mismatch");
            Console.WriteLine(fe);
            return null;
        }
        catch (IOException ie)
        {
            Console.WriteLine("Could not open skeleton file");
            Console.WriteLine(ie);
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
        finally
        {
            if (reader != null) reader.Close();
        }
    }

    /// <summary>
    /// Clones joint settings for matching skeletal joints from one skeleton to the other.  This does not overwrite existing joint drivers.
    /// </summary>
    /// <param name="from">Source skeleton</param>
    /// <param name="to">Destination skeleton</param>
    public static void CloneDriversFromTo(RigidNode_Base from, RigidNode_Base to, bool overwrite = false)
    {
        List<RigidNode_Base> tempNodes = new List<RigidNode_Base>();
        from.ListAllNodes(tempNodes);

        Dictionary<string, RigidNode_Base> fromNodes = new Dictionary<string, RigidNode_Base>();
        foreach (RigidNode_Base cpy in tempNodes)
        {
            fromNodes[cpy.GetModelID()] = cpy;
        }

        tempNodes.Clear();
        to.ListAllNodes(tempNodes);
        foreach (RigidNode_Base copyTo in tempNodes)
        {
            RigidNode_Base fromNode;
            if (fromNodes.TryGetValue(copyTo.GetModelID(), out fromNode))
            {
                if (copyTo.GetSkeletalJoint() != null && fromNode.GetSkeletalJoint() != null && copyTo.GetSkeletalJoint().GetJointType() == fromNode.GetSkeletalJoint().GetJointType())
                {
                    if(copyTo.GetSkeletalJoint().cDriver == null || overwrite)
                    {
                        // Swap driver.
                        copyTo.GetSkeletalJoint().cDriver = fromNode.GetSkeletalJoint().cDriver;
                    }

                    if (copyTo.GetSkeletalJoint().attachedSensors.Count == 0 || overwrite)
                    {
                        // Swap sensors.
                        copyTo.GetSkeletalJoint().attachedSensors = fromNode.GetSkeletalJoint().attachedSensors;
                    }
                }
            }
        }
    }

}
