using System.Collections.Generic;

public enum JointDriverType : byte
{
    MOTOR = 1,
    SERVO = 2,
    WORM_SCREW = 3,
    BUMPER_PNEUMATIC = 4,
    RELAY_PNEUMATIC = 5
}

public class JointDriver
{
    private JointDriverType type;

    public int portA, portB;

    public float lowerLimit, upperLimit;

    public Dictionary<JointDriverMetaType, JointDriverMeta> metaInfo = new Dictionary<JointDriverMetaType, JointDriverMeta>();

    public JointDriver(JointDriverType type)
    {
        this.type = type;
    }

    public static JointDriverType[] getAllowedDrivers(SkeletalJoint_Base joint)
    {
        if (joint.getJointType() == SkeletalJointType.ROTATIONAL)
        {
            // Pneumatic and Worm Screw map to angles
            return new JointDriverType[] { JointDriverType.MOTOR, JointDriverType.SERVO, JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW };
        }
        else if (joint.getJointType() == SkeletalJointType.LINEAR)
        {
            return new JointDriverType[] { JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW };
        }
        else if (joint.getJointType() == SkeletalJointType.CYLINDRICAL)
        {
            return new JointDriverType[] { JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW,
                JointDriverType.MOTOR, JointDriverType.SERVO};
        }
        else if (joint.getJointType() == SkeletalJointType.PLANAR)
        {
            //Not sure of an FRC part with planar motion.  Will add later if needed.
            return new JointDriverType[] { };
        }
        else if (joint.getJointType() == SkeletalJointType.BALL)
        {
            return new JointDriverType[] { };
        }
        else if (joint.getJointType() == SkeletalJointType.RIGID)
        {
            return new JointDriverType[] { };
        }
        return new JointDriverType[0];// Not implemented
    }

    public static bool hasTwoPorts(JointDriverType type) { 
        return type == JointDriverType.BUMPER_PNEUMATIC; 
    }
    public static string getPortType(JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
            case JointDriverType.SERVO:
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

    public static bool isMotor (JointDriverType type)
    {
        bool showWheelPos = true;
        switch (type)
        {
            case JointDriverType.MOTOR:
                showWheelPos = true;
                break;
            default:
                showWheelPos = false;
                break;
        }
        return showWheelPos;
    }

    public static int getPortMax(JointDriverType type)
    {
        switch (type)
        {
            case JointDriverType.MOTOR:
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

    public void setPort(int portA, int portB = -1)
    {
        this.portA = portA;
        this.portB = portB;
    }

    public void setLimits(float lower, float upper)
    {
        this.lowerLimit = lower;
        this.upperLimit = upper;
    }

    public override string ToString()
    {
        return System.Enum.GetName(typeof(JointDriverType), getDriveType()).Replace('_', ' ').ToLowerInvariant() + " " + JointDriver.getPortType(type) + "(" + portA + (JointDriver.hasTwoPorts(getDriveType()) ? "," + portB : "") + ")";
    }

    public JointDriverType getDriveType() { return type; }

    public void writeData(System.IO.BinaryWriter writer)
    {
        writer.Write((byte)((int)getDriveType()));
        writer.Write((short)portA);
        writer.Write((short)portB);
        writer.Write(lowerLimit);
        writer.Write(upperLimit);
        writer.Write(metaInfo.Count); // Extension count
        foreach (JointDriverMeta meta in metaInfo.Values)
        {
            meta.writeData(writer);
        }
    }

    public void readData(System.IO.BinaryReader reader)
    {
        type = (JointDriverType)((int)reader.ReadByte());
        portA = reader.ReadInt16();
        portB = reader.ReadInt16();
        lowerLimit = reader.ReadSingle();
        upperLimit = reader.ReadSingle();
        int extensions = reader.ReadInt32();
        metaInfo.Clear();
        for (int i = 0; i < extensions; i++)
        {
            JointDriverMeta meta = JointDriverMeta.readDriverMeta(reader);
            metaInfo.Add(meta.metaType, meta);
        }
    }
}