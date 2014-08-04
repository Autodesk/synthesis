
public class BallJoint_Base : SkeletalJoint_Base
{
    public BXDVector3 basePoint;

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.BALL;
    }

    protected override void WriteJointInternal(System.IO.BinaryWriter writer)
    {
        writer.Write(basePoint.x);
        writer.Write(basePoint.y);
        writer.Write(basePoint.z);
    }

    protected override void ReadJointInternal(System.IO.BinaryReader reader)
    {
        basePoint = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}