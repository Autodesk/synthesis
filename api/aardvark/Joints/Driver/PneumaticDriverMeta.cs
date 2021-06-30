using System.IO;

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

    #region WHERE IS YOUR _TODD_ NOW
    public double width;


    public PneumaticPressure pressureEnum
    {
        get
        {
            if (pressurePSI == 20)
                return PneumaticPressure.LOW;
            if (pressurePSI == 40)
                return PneumaticPressure.MEDIUM;
            return PneumaticPressure.HIGH;
        }
        set
        {
            switch (value)
            {
                case PneumaticPressure.MEDIUM:
                    pressurePSI = 40;
                    break;
                case PneumaticPressure.LOW:
                    pressurePSI = 20;
                    break;
                default:
                case PneumaticPressure.HIGH:
                    pressurePSI = 60;
                    break;
            }
        }
    }
    #endregion


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