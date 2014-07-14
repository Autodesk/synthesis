using System.IO;

public enum WheelType : byte
{
    NORMAL = 0, //As in, not omni or mecanum.
    OMNI = 1,
    MECANUM = 2
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
    }

    //Reads the position of the wheel from the file.
    protected override void ReadDataInternal(BinaryReader reader)
    {        type = (WheelType)reader.ReadByte();
        radius = reader.ReadSingle();
        width = reader.ReadSingle();
        center.x = reader.ReadSingle();
        center.y = reader.ReadSingle();
        center.z = reader.ReadSingle();
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

