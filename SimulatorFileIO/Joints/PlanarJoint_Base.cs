/*
 *Purpose: Contains the information for an Inventor planar joint.
 */


public class PlanarJoint_Base : SkeletalJoint_Base
{
    public BXDVector3 normal;
    public BXDVector3 basePoint;

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.PLANAR;
    }

    protected override void WriteJointInternal(System.IO.BinaryWriter writer)
    {
        writer.Write(normal);
        writer.Write(basePoint);
    }

    protected override void ReadJointInternal(System.IO.BinaryReader reader)
    {
        normal = reader.ReadRWObject<BXDVector3>();
        basePoint = reader.ReadRWObject<BXDVector3>();
    }
}
