using System.IO;

public enum RobotSensorType : byte
{
    ENCODER = 0,
    LIMIT = 1,
    POTENTIOMETER = 2,
    LIMIT_HALL = 3,
    ACCELEROMETER = 4,
    MAGNETOMETER = 5,
    GYRO = 6
}
public enum SensorConnectionType : byte
{
    DIO = 0,
    ANALOG = 1,
    SPI = 2,
    I2C = 3,
}

public class RobotSensor : BinaryRWObject
{
    public float portA;
    public float portB;
    public RobotSensorType type;
    public SensorConnectionType conTypePortA;
    public SensorConnectionType conTypePortB;
    public double conversionFactor;
    public RobotSensor()
    {
    }

    public RobotSensor(RobotSensorType type)
    {
        this.type = type;

        switch (type)
        {
            case RobotSensorType.ENCODER:
                conTypePortA = SensorConnectionType.DIO;
                conTypePortB = SensorConnectionType.DIO;
                break;
            case RobotSensorType.LIMIT:
                conTypePortA = SensorConnectionType.DIO;
                conTypePortB = SensorConnectionType.DIO;
                break;
            case RobotSensorType.POTENTIOMETER:
                conTypePortA = SensorConnectionType.ANALOG;
                conTypePortB = SensorConnectionType.ANALOG;
                break;
        }
    }

    public static RobotSensorType[] GetAllowedSensors(SkeletalJoint_Base joint)
    {
        switch (joint.GetJointType())
        {
            case SkeletalJointType.ROTATIONAL:
                return new RobotSensorType[] {RobotSensorType.ENCODER/*, RobotSensorType.POTENTIOMETER, RobotSensorType.LIMIT*/};
            case SkeletalJointType.LINEAR:
                return new RobotSensorType[] {RobotSensorType.ENCODER };
            case SkeletalJointType.CYLINDRICAL:
                return new RobotSensorType[] {RobotSensorType.ENCODER/*, RobotSensorType.POTENTIOMETER, RobotSensorType.LIMIT*/};
            case SkeletalJointType.PLANAR:
                return new RobotSensorType[] { };
            case SkeletalJointType.BALL:
                return new RobotSensorType[] { };
            default:
                return new RobotSensorType[0];// Not implemented
        }
    }

    public void WriteBinaryData(BinaryWriter writer)
    {
        writer.Write((byte) type);
        writer.Write(portA);
        writer.Write((byte)conTypePortA);
        writer.Write(portB);
        writer.Write((byte)conTypePortB);
        writer.Write(conversionFactor);
    }

    public void ReadBinaryData(BinaryReader reader)
    {
        type = (RobotSensorType) reader.ReadByte();
        portA = reader.ReadInt16();
        portB = reader.ReadInt16();
        conversionFactor = reader.ReadDouble();
    }

    public static RobotSensor ReadSensorFully(BinaryReader reader)
    {
        RobotSensor sensor = new RobotSensor(RobotSensorType.LIMIT);
        sensor.ReadBinaryData(reader);
        return sensor;
    }
    
    /// <summary>
    /// Compares two sensors, returns true if all fields are identical.
    /// </summary>
    /// <param name="otherSensor"></param>
    public bool Equals(RobotSensor otherSensor)
    {
        return portA == otherSensor.portA && portB == otherSensor.portB && conversionFactor == otherSensor.conversionFactor;    // Other fields are not important for equivalancy
    }
}
