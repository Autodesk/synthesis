using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public enum RobotSensorType : byte
{
    LIMIT,
    ENCODER,
    POTENTIOMETER
}

public class RobotSensor : RWObject
{
    public short module, port;
    public RobotSensorType type;
    public Polynomial equation;
    /// <summary>
    /// If this is true source the secondary angle from the joint.  (Rotational instead of linear for cylindrical)
    /// </summary>
    public bool useSecondarySource = false;

    public RobotSensor(RobotSensorType type)
    {
        this.type = type;
    }

    public void WriteData(BinaryWriter writer)
    {
        writer.Write((byte) type);
        writer.Write(module);
        writer.Write(port);
        writer.Write(equation.coeff.Length);
        for (int i = 0; i < equation.coeff.Length; i++)
        {
            writer.Write(equation.coeff[i]);
        }
        writer.Write(useSecondarySource);
    }

    public void ReadData(BinaryReader reader)
    {
        type = (RobotSensorType) reader.ReadByte();
        module = reader.ReadInt16();
        port = reader.ReadInt16();
        equation = new Polynomial(new float[reader.ReadInt32()]);
        for (int i = 0; i < equation.coeff.Length; i++)
        {
            equation.coeff[i] = reader.ReadSingle();
        }
        useSecondarySource = reader.ReadBoolean();
    }

    public static RobotSensor ReadSensorFully(BinaryReader reader)
    {
        RobotSensor sensor = new RobotSensor(RobotSensorType.LIMIT);
        sensor.ReadData(reader);
        return sensor;
    }
}
