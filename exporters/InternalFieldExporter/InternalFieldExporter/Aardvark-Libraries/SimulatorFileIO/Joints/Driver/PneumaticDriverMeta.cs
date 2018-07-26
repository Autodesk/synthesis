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

    #region WHERE IS YOUR _TODD_ NOW
    public PneumaticDiameter widthEnum
    {
        set
        {
            switch (value)
            {
                case PneumaticDiameter.HIGH:
                    widthMM = 10;
                    break;
                case PneumaticDiameter.LOW:
                    widthMM = 1;
                    break;
                case PneumaticDiameter.MEDIUM:
                default:
                    widthMM = 5;
                    break;
            }
        }
        get
        {
            if (widthMM == 10)
                return PneumaticDiameter.HIGH;
            if (widthMM == 1)
                return PneumaticDiameter.LOW;
            return PneumaticDiameter.MEDIUM;
        }
    }

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