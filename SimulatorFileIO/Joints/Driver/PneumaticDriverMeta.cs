using System.IO;

﻿//find proper specs for these
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
/// Stores the variables concerning a wheel, such as its position (which may be removed later) and radius.  
/// </summary>
public class PneumaticDriverMeta : JointDriverMeta
{
    public float widthMM;
    public float pressurePSI;

    //Writes the position of the wheel to the file.
    protected override void WriteDataInternal(BinaryWriter writer)
    {
        writer.Write(widthMM);
        writer.Write(pressurePSI);
    }

    //Reads the position of the wheel from the file.
    protected override void ReadDataInternal(BinaryReader reader)
    {
        widthMM = reader.ReadSingle();
        pressurePSI = reader.ReadSingle();
    }
}