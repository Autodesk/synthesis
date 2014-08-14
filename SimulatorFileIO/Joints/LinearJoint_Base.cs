
public class LinearJoint_Base : SkeletalJoint_Base
{

    public BXDVector3 axis;
    public BXDVector3 basePoint;

    public float currentLinearPosition;
    public bool hasUpperLimit, hasLowerLimit;
    public float linearLimitLow, linearLimitHigh;
    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.LINEAR;
    }

    protected override void WriteJointInternal(System.IO.BinaryWriter writer)
    {
        writer.Write(basePoint);
        writer.Write(axis);

        writer.Write((byte)((hasLowerLimit ? 1 : 0) | (hasUpperLimit ? 2 : 0)));
        // Ugh
        if (hasLowerLimit && hasUpperLimit && linearLimitLow > linearLimitHigh)
        {
            float temp = linearLimitHigh;
            linearLimitHigh = linearLimitLow;
            linearLimitLow = temp;
        }
        if (hasLowerLimit)
        {
            writer.Write(linearLimitLow);
        }
        if (hasUpperLimit)
        {
            writer.Write(linearLimitHigh);
        }

        writer.Write(currentLinearPosition);
    }

    protected override void ReadJointInternal(System.IO.BinaryReader reader)
    {
        basePoint = reader.ReadRWObject<BXDVector3>();
        axis = reader.ReadRWObject<BXDVector3>();

        byte limitFlags = reader.ReadByte();
        hasLowerLimit = (limitFlags & 1) == 1;
        hasUpperLimit = (limitFlags & 2) == 2;
        if (hasLowerLimit)
        {
            linearLimitLow = reader.ReadSingle();
        }
        if (hasUpperLimit)
        {
            linearLimitHigh = reader.ReadSingle();
        }
        // Ugh
        if (hasLowerLimit && hasUpperLimit && linearLimitLow > linearLimitHigh)
        {
            float temp = linearLimitHigh;
            linearLimitHigh = linearLimitLow;
            linearLimitLow = temp;
        }

        currentLinearPosition = reader.ReadSingle();
    }
}