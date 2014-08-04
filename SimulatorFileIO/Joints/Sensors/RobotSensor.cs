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

public class RobotSensor
{
    public short module, port;
    public readonly RobotSensorType type;
    public float[] polyCoeff;
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
        writer.Write(polyCoeff.Length);
        for (int i = 0; i < polyCoeff.Length; i++)
        {
            writer.Write(polyCoeff[i]);
        }
        writer.Write(useSecondarySource);
    }

    public static RobotSensor ReadData(BinaryReader reader)
    {
        RobotSensor sensor = new RobotSensor((RobotSensorType) reader.ReadByte());
        sensor.module = reader.ReadInt16();
        sensor.port = reader.ReadInt16();
        sensor.polyCoeff = new float[reader.ReadInt32()];
        for (int i = 0; i < sensor.polyCoeff.Length; i++)
        {
            sensor.polyCoeff[i] = reader.ReadSingle();
        }
        sensor.useSecondarySource = reader.ReadBoolean();
        return sensor;
    }
}
