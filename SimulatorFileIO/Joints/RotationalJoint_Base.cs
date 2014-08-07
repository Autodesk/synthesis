
public class RotationalJoint_Base : SkeletalJoint_Base
{

    public BXDVector3 axis;
    public BXDVector3 basePoint;

    public float currentAngularPosition;
    public bool hasAngularLimit;
    public float angularLimitLow;
    public float angularLimitHigh;

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.ROTATIONAL;
    }

    protected override void WriteJointInternal(System.IO.BinaryWriter writer)
    {
        writer.Write(basePoint);
        writer.Write(axis);

        writer.Write((byte)((hasAngularLimit ? 1 : 0)));
        if (hasAngularLimit)
        {
            writer.Write(angularLimitLow);
            writer.Write(angularLimitHigh);
        }

        writer.Write(currentAngularPosition);
    }

    protected override void ReadJointInternal(System.IO.BinaryReader reader)
    {
        basePoint = reader.ReadRWObject<BXDVector3>();
        axis = reader.ReadRWObject<BXDVector3>();

        hasAngularLimit = (reader.ReadByte() & 1) == 1;
        if (hasAngularLimit)
        {
            angularLimitLow = reader.ReadSingle();
            angularLimitHigh = reader.ReadSingle();
        }

        currentAngularPosition = reader.ReadSingle();
    }
}