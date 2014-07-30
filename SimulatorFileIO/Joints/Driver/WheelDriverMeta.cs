using System.IO;

public enum WheelType : byte
{
    NOT_A_WHEEL = 0,
    NORMAL = 1, //As in, not omni or mecanum.
    OMNI = 2,
    MECANUM = 3
}

public enum FrictionLevel : byte
{
    HIGH = 0,
    MEDIUM = 1,
    LOW = 2
}



public class WheelDriverMeta : JointDriverMeta
{
    /// <summary>
    /// Stores the variables concerning a wheel, such as its position (which may be removed later) and radius.  
    /// </summary>

    public float radius
    {
        get;
        set;
    }

    public WheelType type
    {
        get;
        set;
    }

    public float width
    {
        get;
        set;
    }

    public BXDVector3 center
    {
        get;
        set;
    }

    public float forwardExtremeSlip
    {
        get;
        set;
    }

    public float forwardExtremeValue
    {
        get;
        set;
    }

    public float forwardAsympSlip
    {
        get;
        set;
    }

    public float forwardAsympValue
    {
        get;
        set;
    }

    public float sideExtremeSlip
    {
        get;
        set;
    }

    public float sideExtremeValue
    {
        get;
        set;
    }

    public float sideAsympSlip
    {
        get;
        set;
    }

    public float sideAsympValue
    {
        get;
        set;
    }

    public WheelDriverMeta()
    {
        center = new BXDVector3();
    }

    //Writes the position of the wheel to the file.
    protected override void WriteDataInternal(BinaryWriter writer)
    {
        writer.Write((byte)((int)type));
        writer.Write(radius);
        writer.Write(width);

        writer.Write(center.x);
        writer.Write(center.y);
        writer.Write(center.z);

        writer.Write(forwardAsympSlip);
        writer.Write(forwardAsympValue);
        writer.Write(forwardExtremeSlip);
        writer.Write(forwardExtremeValue);
        writer.Write(sideAsympSlip);
        writer.Write(sideAsympValue);
        writer.Write(sideExtremeSlip);
        writer.Write(sideExtremeValue);
    }

    //Reads the position of the wheel from the file.
    protected override void ReadDataInternal(BinaryReader reader)
    {        
        type = (WheelType)reader.ReadByte();
        radius = reader.ReadSingle();
        width = reader.ReadSingle();

        center.x = reader.ReadSingle();
        center.y = reader.ReadSingle();
        center.z = reader.ReadSingle();

        forwardAsympSlip = reader.ReadSingle();
        forwardAsympValue = reader.ReadSingle();
        forwardExtremeSlip = reader.ReadSingle();
        forwardExtremeValue = reader.ReadSingle();
        sideAsympSlip = reader.ReadSingle();
        sideAsympValue = reader.ReadSingle();
        sideExtremeSlip = reader.ReadSingle();
        sideExtremeValue = reader.ReadSingle();
    }

    public string GetTypeString()
    {
        switch (type)
        {
            case WheelType.OMNI:
                return "Omni Wheel";
            case WheelType.MECANUM:
                return "Mecanum";
            default:
                return "Normal";
        }
    }

    public override string ToString()
    {
        return "WheelMeta[rad=" + radius + "]";
    }
}