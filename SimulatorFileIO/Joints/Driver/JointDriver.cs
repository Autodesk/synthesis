using System.Collections.Generic;

public enum JointDriverType : byte
{
    MOTOR = 1,
    SERVO = 2,
    WORM_SCREW = 3,
    BUMPER_PNEUMATIC = 4,
    RELAY_PNEUMATIC = 5,
    DUAL_MOTOR = 6
}

public class JointDriver
{
    private JointDriverType type;

    public int portA, portB;

    public float lowerLimit, upperLimit;

    private Dictionary<System.Type, JointDriverMeta> metaInfo = new Dictionary<System.Type, JointDriverMeta>();

    public JointDriver(JointDriverType type)
    {
        this.type = type;
    }

    //Adds details to the driver type.
    public void AddInfo(JointDriverMeta metaDriver)
    {
        metaInfo.Add(metaDriver.GetType(), metaDriver);
    }

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

    public static JointDriverType[] GetAllowedDrivers(SkeletalJoint_Base joint)
    {
        switch (joint.GetJointType())
        {
            case SkeletalJointType.ROTATIONAL:
                // Pneumatic and Worm Screw map to angles
                return new JointDriverType[] { JointDriverType.MOTOR, JointDriverType.SERVO, JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW, JointDriverType.DUAL_MOTOR};
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
            case SkeletalJointType.RIGID:
                return new JointDriverType[] { };
            default:
                return new JointDriverType[0];// Not implemented
        }
    }

    public static bool HasTwoPorts(JointDriverType type)
    {
        return type == JointDriverType.BUMPER_PNEUMATIC;
        return type == JointDriverType.DUAL_MOTOR;
    }
    public static string GetPortType(JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.SERVO:
            case JointDriverType.DUAL_MOTOR:
            case JointDriverType.WORM_SCREW:
                return "PWM";
            case JointDriverType.BUMPER_PNEUMATIC:
                return "Solenoid";
            case JointDriverType.RELAY_PNEUMATIC:
                return "Relay";
            default:
                return "Unknown";
        }
    }

    public static bool IsMotor(JointDriverType type)
    {
        bool showWheelPos = true;
        switch (type)
        {
            case JointDriverType.MOTOR:
                showWheelPos = true;
                break;
            case JointDriverType.DUAL_MOTOR:
                showWheelPos = true;
                break;
            default:
                showWheelPos = false;
                break;
        }
        return showWheelPos;
    }

    public static int GetPortMax(JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.DUAL_MOTOR:
            case JointDriverType.SERVO:
            case JointDriverType.WORM_SCREW:
                return 8; // PWM
            case JointDriverType.BUMPER_PNEUMATIC:
                return 8; // Bumper
            case JointDriverType.RELAY_PNEUMATIC:
                return 8; // Relay
            default:
                return -1;
        }
    }

    public void SetPort(int portA, int portB = -1)
    {
        this.portA = portA;
        this.portB = portB;
    }

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

    public JointDriverType GetDriveType()
    {
        return type;
    }

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
}