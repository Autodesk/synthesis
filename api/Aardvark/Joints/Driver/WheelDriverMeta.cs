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

/// <summary>
/// Stores the variables concerning a wheel, such as its position (which may be removed later) and radius.  
/// </summary>
public class WheelDriverMeta : JointDriverMeta
{
    public float radius;
    public WheelType type;
    public float width;
    public BXDVector3 center;

    public float forwardExtremeSlip;
    public float forwardExtremeValue;
    public float forwardAsympSlip;
    public float forwardAsympValue;
    public float sideExtremeSlip;
    public float sideExtremeValue;
    public float sideAsympSlip;
    public float sideAsympValue;
    public bool isDriveWheel;
    public WheelDriverMeta()
    {
        center = new BXDVector3();
       
    }

    //Writes the position of the wheel to the file.
    protected override void WriteDataInternal(BinaryWriter writer)
    {
        writer.Write((byte) ((int) type));
        writer.Write(radius);
        writer.Write(width);

        writer.Write(center);

        writer.Write(forwardAsympSlip);
        writer.Write(forwardAsympValue);
        writer.Write(forwardExtremeSlip);
        writer.Write(forwardExtremeValue);
        writer.Write(sideAsympSlip);
        writer.Write(sideAsympValue);
        writer.Write(sideExtremeSlip);
        writer.Write(sideExtremeValue);
        writer.Write(isDriveWheel);
    }

    //Reads the position of the wheel from the file.
    protected override void ReadDataInternal(BinaryReader reader)
    {
        type = (WheelType) reader.ReadByte();
        radius = reader.ReadSingle();
        width = reader.ReadSingle();

        center = reader.ReadRWObject<BXDVector3>();

        forwardAsympSlip = reader.ReadSingle();
        forwardAsympValue = reader.ReadSingle();
        forwardExtremeSlip = reader.ReadSingle();
        forwardExtremeValue = reader.ReadSingle();
        sideAsympSlip = reader.ReadSingle();
        sideAsympValue = reader.ReadSingle();
        sideExtremeSlip = reader.ReadSingle();
        sideExtremeValue = reader.ReadSingle();
        isDriveWheel = reader.ReadBoolean();
    }

    public void SetFrictionLevel(FrictionLevel level)
    {
        switch (level)
        {
            case FrictionLevel.HIGH:
                forwardExtremeSlip = 1; //Speed of max static friction force.
                forwardExtremeValue = 10; //Force of max static friction force.
                forwardAsympSlip = 1.5f; //Speed of leveled off kinetic friction force.
                forwardAsympValue = 8; //Force of leveld off kinetic friction force.

                if (type == WheelType.OMNI) //Set to relatively low friction, as omni wheels can move sidways.
                {
                    sideExtremeSlip = 1; //Same as above, but orthogonal to the movement of the wheel.
                    sideExtremeValue = .01f;
                    sideAsympSlip = 1.5f;
                    sideAsympValue = .005f;
                }
                else
                {
                    sideExtremeSlip = 1;
                    sideExtremeValue = 10;
                    sideAsympSlip = 1.5f;
                    sideAsympValue = 8;
                }
                break;
            case FrictionLevel.MEDIUM:
                forwardExtremeSlip = 1f;
                forwardExtremeValue = 7;
                forwardAsympSlip = 1.5f;
                forwardAsympValue = 5;

                if (type == WheelType.OMNI)
                {
                    sideExtremeSlip = 1;
                    sideExtremeValue = .01f;
                    sideAsympSlip = 1.5f;
                    sideAsympValue = .005f;
                }
                else
                {
                    sideExtremeSlip = 1;
                    sideExtremeValue = 7;
                    sideAsympSlip = 1.5f;
                    sideAsympValue = 5;
                }
                break;
            case FrictionLevel.LOW:
                forwardExtremeSlip = 1;
                forwardExtremeValue = 5;
                forwardAsympSlip = 1.5f;
                forwardAsympValue = 3;

                if (type == WheelType.OMNI)
                {
                    sideExtremeSlip = 1;
                    sideExtremeValue = .01f;
                    sideAsympSlip = 1.5f;
                    sideAsympValue = .005f;
                }
                else
                {
                    sideExtremeSlip = 1;
                    sideExtremeValue = 5;
                    sideAsympSlip = 1.5f;
                    sideAsympValue = 3;
                }
                break;
        }
    }

    public FrictionLevel GetFrictionLevel()
    {
        if (forwardExtremeValue >= 10)
            return FrictionLevel.HIGH;
        else if (forwardExtremeValue >= 7)
            return FrictionLevel.MEDIUM;
        else
            return FrictionLevel.LOW;
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