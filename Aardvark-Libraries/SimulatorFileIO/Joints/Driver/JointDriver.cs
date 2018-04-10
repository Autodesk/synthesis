using System;
using System.Collections.Generic;
/// <summary>
/// Generic class able to represent all types of joint drivers.
/// </summary>
public class JointDriver : BinaryRWObject, IComparable<JointDriver>
{
    /// <summary>
    /// The type of this joint driver.
    /// </summary>
    private JointDriverType type;

    /// <summary>
    /// The port(s) that this joint driver uses.
    /// </summary>
    public int portA, portB;

    public bool isCan = false;

    /// <summary>
    /// The motion limits for this driver.  For continuous rotation this is likely a force or velocity.
    /// For linear or limited movement it is motion limits.
    /// </summary>
    public float lowerLimit, upperLimit;

    /// <summary>
    /// Metadata information for this joint driver.
    /// </summary>
    public Dictionary<System.Type, JointDriverMeta> MetaInfo
    {
        get;
        private set;
    }

    /// <summary>
    /// Creates as joint driver with no type.  This is mainly for IO
    /// </summary>
    public JointDriver()
    {
        MetaInfo = new Dictionary<System.Type, JointDriverMeta>();
    }

    /// <summary>
    /// Creates a joint driver with the given type.
    /// </summary>
    /// <param name="type">Driver type</param>
    public JointDriver(JointDriverType type)
        : this()
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
            MetaInfo.Add(metaDriver.GetType(), metaDriver);
        }
        catch
        {
            //Always throws SystemNullReferenceException here when you are exporting pneumatic
            //Go to JointDrivderMeta.cs Line 15 to patch
            MetaInfo[metaDriver.GetType()] = metaDriver;
        }
    }


    /// <summary>
    /// Removes the metadata of the given type stored in this joint driver and returns it, or null if no such metadata exists.
    /// </summary>
    /// <typeparam name="T">The type of the metadata</typeparam>
    /// <returns>Metadata, or null</returns>
    public T RemoveInfo<T>() where T : JointDriverMeta
    {
        JointDriverMeta val;
        System.Type type = typeof(T);
        if (MetaInfo.TryGetValue(type, out val))
        {
            MetaInfo.Remove(type);
            return (T) val;
        }
        return null;
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
        if (MetaInfo.TryGetValue(type, out val))
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
        if (MetaInfo.TryGetValue(type, out val))
        {
            return val;
        }
        return null;
    }

    public void CopyMetaInfo(JointDriver toCopy)
    {
        foreach (KeyValuePair<System.Type, JointDriverMeta> pair in MetaInfo)
        {
            toCopy.MetaInfo.Add(pair.Key, pair.Value);
        }
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
                return new JointDriverType[] { JointDriverType.MOTOR, JointDriverType.SERVO, JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW, JointDriverType.DUAL_MOTOR};
            case SkeletalJointType.LINEAR:
                return new JointDriverType[] { JointDriverType.ELEVATOR, JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW};
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
        lowerLimit = lower;
        upperLimit = upper;
    }

    public override string ToString()
    {
        string info = System.Enum.GetName(typeof(JointDriverType), GetDriveType()).Replace('_', ' ').ToLowerInvariant();
        info += "\nPorts: " + type.GetPortType() + "(" + portA + (type.HasTwoPorts() ? "," + portB : "") + ")";
        info += "\nMeta: ";
        foreach (KeyValuePair<System.Type, JointDriverMeta> meta in MetaInfo)
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
    public void WriteBinaryData(System.IO.BinaryWriter writer)
    {
        writer.Write((byte) ((int) GetDriveType()));
        writer.Write((short) portA);
        writer.Write((short) portB);
        writer.Write(lowerLimit);
        writer.Write(upperLimit);
        writer.Write(isCan);
        writer.Write(MetaInfo.Count); // Extension count
        foreach (JointDriverMeta meta in MetaInfo.Values)
        {
            meta.WriteData(writer);
        }
    }

    /// <summary>
    /// Reads the binary representation of this driver from the stream.
    /// </summary>
    /// <param name="reader">Input stream</param>
    public void ReadBinaryData(System.IO.BinaryReader reader)
    {
        type = (JointDriverType) ((int) reader.ReadByte());
        portA = reader.ReadInt16();
        portB = reader.ReadInt16();
        lowerLimit = reader.ReadSingle();
        upperLimit = reader.ReadSingle();
        isCan = reader.ReadBoolean();
        int extensions = reader.ReadInt32();
        MetaInfo.Clear();
        for (int i = 0; i < extensions; i++)
        {
            JointDriverMeta meta = JointDriverMeta.ReadDriverMeta(reader);
            AddInfo(meta);
        }
    }

    public int CompareTo(JointDriver driver)
    {
        if (driver == null) return 1;

        if (ToString() == driver.ToString()) return 0;
        else return 1;
    }

}