
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
        writer.Write(basePoint.x);
        writer.Write(basePoint.y);
        writer.Write(basePoint.z);
        writer.Write(axis.x);
        writer.Write(axis.y);
        writer.Write(axis.z);

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
        basePoint = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        axis = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        hasAngularLimit = (reader.ReadByte() & 1) == 1;
        if (hasAngularLimit)
        {
            angularLimitLow = reader.ReadSingle();
            angularLimitHigh = reader.ReadSingle();
        }

        currentAngularPosition = reader.ReadSingle();
    }
}