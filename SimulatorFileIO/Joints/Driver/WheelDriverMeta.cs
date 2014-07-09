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

    public WheelDriverMeta()
    {
    }

    //Writes the position of the wheel to the file.
    protected override void WriteDataInternal(BinaryWriter writer)
    {
        writer.Write((byte)((int)position));
        writer.Write(radius);
    }

    //Reads the position of the wheel from the file.
    protected override void ReadDataInternal(BinaryReader reader)
    {
        position = (WheelPosition)reader.ReadByte();
        radius = reader.ReadSingle();
    }

    public override string ToString()
    {
        return "WheelMeta[pos=" + System.Enum.GetName(typeof(WheelPosition), position) + ",rad=" + radius + "]";
    }
}

