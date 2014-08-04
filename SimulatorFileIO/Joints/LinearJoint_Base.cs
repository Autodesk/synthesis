
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
        writer.Write(basePoint.x);
        writer.Write(basePoint.y);
        writer.Write(basePoint.z);
        writer.Write(axis.x);
        writer.Write(axis.y);
        writer.Write(axis.z);

        writer.Write((byte)((hasLowerLimit ? 1 : 0) | (hasUpperLimit ? 2 : 0)));
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
        basePoint = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        axis = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

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

        currentLinearPosition = reader.ReadSingle();
    }
}