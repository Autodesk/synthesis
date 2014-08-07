using System.Collections.Generic;

/// <summary>
/// Types of joint drivers.
/// </summary>
public enum JointDriverType : byte
{
    MOTOR = 1,
    SERVO = 2,
    WORM_SCREW = 3,
    BUMPER_PNEUMATIC = 4,
    RELAY_PNEUMATIC = 5,
    DUAL_MOTOR = 6,
    NO_DRIVER = 7
}

/// <summary>
/// Generic class able to represent all types of joint drivers.
/// </summary>
public class JointDriver
{
    /// <summary>
    /// The type of this joint driver.
    /// </summary>
    private JointDriverType type;

    /// <summary>
    /// The port(s) that this joint driver uses.
    /// </summary>
    public int portA, portB;

    /// <summary>
    /// The motion limits for this driver.  For continuous rotation this is likely a force or velocity.
    /// For linear or limited movement it is motion limits.
    /// </summary>
    public float lowerLimit, upperLimit;

    /// <summary>
    /// Metadata information for this joint driver.
    /// </summary>
    private Dictionary<System.Type, JointDriverMeta> metaInfo = new Dictionary<System.Type, JointDriverMeta>();

    /// <summary>
    /// Creates a joint driver with the given type.
    /// </summary>
    /// <param name="type">Driver type</param>
    public JointDriver(JointDriverType type)
    {
        this.type = type;
    }

    /// <summary>
    /// Adds the given joint driver metadata object to this driver, or replaces the existing metadata of the same type.
    /// </summary>
    /// <param name="metaDriver">The metadata</param>
    public void AddInfo(JointDriverMeta metaDriver)
    {
        try
        {
            metaInfo.Add(metaDriver.GetType(), metaDriver);
        }
        catch
        {
            //Always throws SystemNullReferenceException here when you are exporting pneumatic
            //Go to JointDrivderMeta.cs Line 15 to patch
            metaInfo[metaDriver.GetType()] = metaDriver;
        }
    }

    /// <summary>
    /// Gets the metadata of the given type stored in this joint driver, or null if no such metadata exists.
    /// </summary>
    /// <typeparam name="T">The type of the metadata</typeparam>
    /// <returns>Metadata, or null</returns>
    public T GetInfo<T>() where T : JointDriverMeta
    {
        JointDriverMeta val;
        System.Type type = typeof(T);
        if (metaInfo.TryGetValue(type, out val))
        {
            return (T) val;
        }
        return null;
    }

    /// <summary>
    /// Gets the possible types of joint drivers for the given skeletal joint.
    /// </summary>
    /// <param name="joint">Skeletal joint to get allowed types for</param>
    /// <returns>Joint driver options</returns>
    public static JointDriverType[] GetAllowedDrivers(SkeletalJoint_Base joint)
    {
        switch (joint.GetJointType())
        {
            case SkeletalJointType.ROTATIONAL:
                // Pneumatic and Worm Screw map to angles
                return new JointDriverType[] { JointDriverType.MOTOR, JointDriverType.SERVO, JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW, JointDriverType.DUAL_MOTOR, JointDriverType.NO_DRIVER };
            case SkeletalJointType.LINEAR:
                return new JointDriverType[] { JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW, JointDriverType.NO_DRIVER };
            case SkeletalJointType.CYLINDRICAL:
                return new JointDriverType[] { JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW,
                JointDriverType.MOTOR, JointDriverType.SERVO, JointDriverType.DUAL_MOTOR, JointDriverType.NO_DRIVER};
            case SkeletalJointType.PLANAR:
                //Not sure of an FRC part with planar motion.  Will add later if needed.
                return new JointDriverType[] { };
            case SkeletalJointType.BALL:
                return new JointDriverType[] { };
            default:
                return new JointDriverType[0];// Not implemented
        }
    }

    /// <summary>
    /// Checks if the given driver type requires two ports.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>True is the given type requires two ports</returns>
    public static bool HasTwoPorts(JointDriverType type)
    {
        return type == JointDriverType.BUMPER_PNEUMATIC || type == JointDriverType.DUAL_MOTOR;
    }

    /// <summary>
    /// Gets the string representation of the port for the given driver type.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Name of port type</returns>
    public static string GetPortType(JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.SERVO:
            case JointDriverType.DUAL_MOTOR:
            case JointDriverType.WORM_SCREW:
            case JointDriverType.NO_DRIVER:
                return "PWM";
            case JointDriverType.BUMPER_PNEUMATIC:
                return "Solenoid";
            case JointDriverType.RELAY_PNEUMATIC:
                return "Relay";
            default:
                return "Unknown";
        }
    }

    /// <summary>
    /// Checks if the joint is driven (has a motor, or servo, etc.).
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Boolean</returns>
    public static bool IsDrivenJoint(JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.NO_DRIVER:
                return false;
            default:
                return true;
        }
    }

    /// <summary>
    /// Checks if the given driver type is a motor.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Boolean</returns>
    public static bool IsMotor(JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.DUAL_MOTOR:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if the given driver type is a pneumatic.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Boolean</returns>
    public static bool IsPneumatic(JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.BUMPER_PNEUMATIC:
            case JointDriverType.RELAY_PNEUMATIC:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the maximum port number for the given driver type.
    /// </summary>
    /// <param name="type">Driver type</param>
    /// <returns>Max port number</returns>
    public static int GetPortMax(JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.DUAL_MOTOR:
            case JointDriverType.SERVO:
            case JointDriverType.WORM_SCREW:
            case JointDriverType.NO_DRIVER:
                return 8; // PWM
            case JointDriverType.BUMPER_PNEUMATIC:
                return 8; // Bumper
            case JointDriverType.RELAY_PNEUMATIC:
                return 8; // Relay
            default:
                return -1;
        }
    }

    /// <summary>
    /// Sets the port(s) for this driver.
    /// </summary>
    /// <param name="portA">First port</param>
    /// <param name="portB">Option second port</param>
    public void SetPort(int portA, int portB = -1)
    {
        this.portA = portA;
        this.portB = portB;
    }

    /// <summary>
    /// Sets the limits for this driver.
    /// </summary>
    /// <remarks>
    /// For all linear motion these represent the linear limits on that motion.
    /// For all limited angular motion these represent the angular limits on that motion.
    /// For all continuous angular motion these represent the force limits on that motion.
    /// </remarks>
    /// <param name="lower">Lower limit</param>
    /// <param name="upper">Upper limit</param>
    public void SetLimits(float lower, float upper)
    {
        this.lowerLimit = lower;
        this.upperLimit = upper;
    }

    public override string ToString()
    {
        string info = System.Enum.GetName(typeof(JointDriverType), GetDriveType()).Replace('_', ' ').ToLowerInvariant();
        info += "\nPorts: " + JointDriver.GetPortType(type) + "(" + portA + (JointDriver.HasTwoPorts(GetDriveType()) ? "," + portB : "") + ")";
        info += "\nMeta: ";
        foreach (KeyValuePair<System.Type, JointDriverMeta> meta in metaInfo)
        {
            info += "\n\t" + meta.Value.ToString();
        }
        return info;
    }

    /// <summary>
    /// Gets the type of this joint driver.
    /// </summary>
    /// <returns>Driver type</returns>
    public JointDriverType GetDriveType()
    {
        return type;
    }

    /// <summary>
    /// Writes the binary representation of this driver to the stream.
    /// </summary>
    /// <param name="writer">Output stream</param>
    public void WriteData(System.IO.BinaryWriter writer)
    {
        writer.Write((byte) ((int) GetDriveType()));
        writer.Write((short) portA);
        writer.Write((short) portB);
        writer.Write(lowerLimit);
        writer.Write(upperLimit);
        writer.Write(metaInfo.Count); // Extension count
        foreach (JointDriverMeta meta in metaInfo.Values)
        {
            meta.WriteData(writer);
        }
    }

    /// <summary>
    /// Reads the binary representation of this driver from the stream.
    /// </summary>
    /// <param name="reader">Input stream</param>
    public void ReadData(System.IO.BinaryReader reader)
    {
        type = (JointDriverType) ((int) reader.ReadByte());
        portA = reader.ReadInt16();
        portB = reader.ReadInt16();
        lowerLimit = reader.ReadSingle();
        upperLimit = reader.ReadSingle();
        int extensions = reader.ReadInt32();
        metaInfo.Clear();
        for (int i = 0; i < extensions; i++)
        {
            JointDriverMeta meta = JointDriverMeta.ReadDriverMeta(reader);
            AddInfo(meta);
        }
    }

    /// <summary>
    /// Clones joint driver settings for matching skeletal joints from one skeleton to the other.  This does not overwrite existing joint drivers.
    /// </summary>
    /// <param name="from">Source skeleton</param>
    /// <param name="to">Destination skeleton</param>
    public static void CloneDriversFromTo(RigidNode_Base from, RigidNode_Base to)
    {
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        from.ListAllNodes(nodes);
        Dictionary<string, RigidNode_Base> fromNodes = new Dictionary<string, RigidNode_Base>();
        foreach (RigidNode_Base cpy in nodes)
        {
            fromNodes[cpy.GetModelID()] = cpy;
        }
        nodes.Clear();
        to.ListAllNodes(nodes);
        foreach (RigidNode_Base copyTo in nodes)
        {
            RigidNode_Base fromNode;
            if (fromNodes.TryGetValue(copyTo.GetModelID(), out fromNode))
            {
                if (copyTo.GetSkeletalJoint() != null && fromNode.GetSkeletalJoint() != null && copyTo.GetSkeletalJoint().GetJointType() == fromNode.GetSkeletalJoint().GetJointType() && copyTo.GetSkeletalJoint().cDriver == null)
                {
                    // Copy the driver
                    copyTo.GetSkeletalJoint().cDriver = fromNode.GetSkeletalJoint().cDriver;
                }
            }
        }
    }
}