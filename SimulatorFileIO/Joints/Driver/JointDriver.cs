using System.Collections.Generic;
/// <summary>
/// Generic class able to represent all types of joint drivers.
/// </summary>
public class JointDriver : RWObject
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
    /// Creates as joint driver with no type.  This is mainly for IO
    /// </summary>
    public JointDriver()
    {
    }

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
    /// Gets the metadata of the given type stored in this joint driver, or null if no such metadata exists.
    /// </summary>
    /// <param name="type">The type to get info for</param>
    /// <returns>Metadata, or null</returns>
    public JointDriverMeta GetInfo(System.Type type)
    {
        JointDriverMeta val;
        if (metaInfo.TryGetValue(type, out val))
        {
            return val;
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
                return new JointDriverType[] { JointDriverType.MOTOR, JointDriverType.SERVO, JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW, JointDriverType.DUAL_MOTOR };
            case SkeletalJointType.LINEAR:
                return new JointDriverType[] { JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW };
            case SkeletalJointType.CYLINDRICAL:
                return new JointDriverType[] { JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW,
                JointDriverType.MOTOR, JointDriverType.SERVO, JointDriverType.DUAL_MOTOR};
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
        info += "\nPorts: " + type.GetPortType() + "(" + portA + (type.HasTwoPorts() ? "," + portB : "") + ")";
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