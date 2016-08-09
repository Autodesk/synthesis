using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public enum RobotSensorType : byte
{
    LIMIT = 0,
    ENCODER = 1,
    POTENTIOMETER = 2,
    LIMIT_HALL = 3,
    ACCELEROMETER = 4,
    MAGNETOMETER = 5,
    GYRO = 6
}

public class RobotSensor : BinaryRWObject
{
    public short module, port;
    public RobotSensorType type;
    public Polynomial equation;
    /// <summary>
    /// If this is true source the secondary value from the joint.  (Rotational instead of linear for cylindrical)
    /// </summary>
    public bool useSecondarySource = false;

    public RobotSensor()
    {
    }

    public RobotSensor(RobotSensorType type)
    {
        this.type = type;
    }

    public static RobotSensorType[] GetAllowedSensors(SkeletalJoint_Base joint)
    {
        switch (joint.GetJointType())
        {
            case SkeletalJointType.ROTATIONAL:
                return new RobotSensorType[] {RobotSensorType.ENCODER, RobotSensorType.POTENTIOMETER, RobotSensorType.LIMIT};
            case SkeletalJointType.LINEAR:
                return new RobotSensorType[] {RobotSensorType.LIMIT };
            case SkeletalJointType.CYLINDRICAL:
                return new RobotSensorType[] {RobotSensorType.ENCODER, RobotSensorType.POTENTIOMETER, RobotSensorType.LIMIT};
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
        writer.Write(module);
        writer.Write(port);
        writer.Write(equation);
        writer.Write(useSecondarySource);
    }

    public void ReadBinaryData(BinaryReader reader)
    {
        type = (RobotSensorType) reader.ReadByte();
        module = reader.ReadInt16();
        port = reader.ReadInt16();
        equation = reader.ReadRWObject<Polynomial>();
        useSecondarySource = reader.ReadBoolean();
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
        return module == otherSensor.module && port == otherSensor.port;    // Other fields are not important for equivalancy
    }
}
