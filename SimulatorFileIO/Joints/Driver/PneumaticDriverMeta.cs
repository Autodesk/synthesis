using System.IO;

//find proper specs for these
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

    public BXDVector3 center
    {
        get;
        set;
    }


    public PneumaticDriverMeta()
    {
        center = new BXDVector3();
    }

    //Writes the position of the wheel to the file.
    protected override void WriteDataInternal(BinaryWriter writer)
    {
        writer.Write((byte)((int)type));
        
        writer.Write(widthMM);
    }

    //Reads the position of the wheel from the file.
    protected override void ReadDataInternal(BinaryReader reader)
    {
        type = (JointDriverType)reader.ReadByte();
        
        widthMM = reader.ReadSingle();
    }

    public string GetTypeString()
    {
        switch (type)
        {
            case JointDriverType.BUMPER_PNEUMATIC:
                return "Bumper Pneumatic";
            case JointDriverType.RELAY_PNEUMATIC:
                return "Relay Pneumatic";
            default:
                return "Unknown";
        }
    }
/*
    public override string ToString()
    {
        return "WheelMeta[rad=" + radius + "]";
    } */
}