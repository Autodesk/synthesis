using System.IO;

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




public class PneumaticDriverMeta : JointDriverMeta
{
    /// <summary>
    /// Stores the variables concerning a wheel, such as its position (which may be removed later) and radius.  
    /// </summary>

    public JointDriverType type
    {
        get;
        set;
    }

    public float widthMM
    {
        get;
        set;
    }

    public float pressurePSI
    {
        get;
        set;
    }


    //Writes the position of the wheel to the file.
    protected override void WriteDataInternal(BinaryWriter writer)
    {
        writer.Write((byte)((int)type));

        writer.Write(widthMM);
        writer.Write(pressurePSI);
    }

    //Reads the position of the wheel from the file.
    protected override void ReadDataInternal(BinaryReader reader)
    {
        widthMM = reader.ReadSingle();
        pressurePSI = reader.ReadSingle();
    }

/*
    public override string ToString()
    {
        return "WheelMeta[rad=" + radius + "]";
    } */
}