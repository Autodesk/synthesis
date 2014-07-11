using System.IO;

//The position relative to the front of the robot.
public enum WheelPosition : byte
{
    NO_WHEEL = 0,
    FRONT_LEFT = 1,
    FRONT_RIGHT = 2,
    BACK_LEFT = 3,
    BACK_RIGHT = 4
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
    public WheelPosition position
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
    }

    //Writes the position of the wheel to the file.
    protected override void WriteDataInternal(BinaryWriter writer)
    {
        writer.Write((byte)((int)position));
        writer.Write(radius);
        writer.Write(width);
        writer.Write(center.x);
        writer.Write(center.y);
        writer.Write(center.z);
    }

    //Reads the position of the wheel from the file.
    protected override void ReadDataInternal(BinaryReader reader)
    {
        position = (WheelPosition)reader.ReadByte();
        radius = reader.ReadSingle();
        width = reader.ReadSingle();
        center.x = reader.ReadSingle();
        center.y = reader.ReadSingle();
        center.z = reader.ReadSingle();
    }

    public override string ToString()
    {
        return "WheelMeta[pos=" + System.Enum.GetName(typeof(WheelPosition), position) + ",rad=" + radius + "]";
    }
}

