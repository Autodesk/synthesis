using System.IO;

﻿// TODO find proper specs for these
public enum PneumaticDiameter : byte
{
    HIGH = 0,
    MEDIUM = 1,
    LOW = 2
}

public enum PneumaticPressure : byte
{
    HIGH = 0,
    MEDIUM = 1,
    LOW = 2
}


/// <summary>
/// Stores the variables concerning a pneumatic cylinder.
/// </summary>
public class PneumaticDriverMeta : JointDriverMeta
{
    public float widthMM;
    public float pressurePSI;


    protected override void WriteDataInternal(BinaryWriter writer)
    {
        writer.Write(widthMM);
        writer.Write(pressurePSI);
    }
    protected override void ReadDataInternal(BinaryReader reader)
    {
        widthMM = reader.ReadSingle();
        pressurePSI = reader.ReadSingle();
    }
}