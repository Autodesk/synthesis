public enum JointDriverType
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

    public double lowerLimit, upperLimit;

    public JointDriver(JointDriverType type)
    {
        this.type = type;
    }

    public static JointDriverType[] getAllowedDrivers(SkeletalJoint joint)
    {
        if (joint is RotationalJoint)
        {
            // Pneumatic and Worm Screw map to angles
            return new JointDriverType[] { JointDriverType.MOTOR, JointDriverType.SERVO, JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW };
        }
        else if (joint is LinearJoint)
        {
            return new JointDriverType[] { JointDriverType.BUMPER_PNEUMATIC, JointDriverType.RELAY_PNEUMATIC, JointDriverType.WORM_SCREW };
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

    public void setLimits(double lower, double upper)
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
        writer.Write((byte)getDriveType());
        writer.Write((short)portA);
        writer.Write((short)portB);
        writer.Write(lowerLimit);
        writer.Write(upperLimit);
        writer.Write(0); // Extension count
        // No extensions
    }

    public void readData(System.IO.BinaryReader reader)
    {
        type = (JointDriverType)reader.ReadByte();
        portA = reader.ReadInt16();
        portB = reader.ReadInt16();
        lowerLimit = reader.ReadDouble();
        upperLimit = reader.ReadDouble();
        int extensions = reader.ReadInt32();
    }
}