
public class BallJoint_Base : SkeletalJoint_Base
{
    public BXDVector3 parentBase;
    public BXDVector3 childBase;

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.BALL;
    }

    public override void WriteJoint(System.IO.BinaryWriter writer)
    {
        writer.Write(parentBase.x);
        writer.Write(parentBase.y);
        writer.Write(parentBase.z);

        writer.Write(childBase.x);
        writer.Write(childBase.y);
        writer.Write(childBase.z);
    }

    protected override void ReadJoint(System.IO.BinaryReader reader)
    {
        parentBase = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        childBase = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}