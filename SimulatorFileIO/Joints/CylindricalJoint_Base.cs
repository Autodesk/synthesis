/*
 *Purpose: Contains the information for an Inventor cylindrical joint.
 */



public class CylindricalJoint_Base : SkeletalJoint_Base
{

    public BXDVector3 axis; //The axis of both rotation and movement;
    public BXDVector3 basePoint;

    public float currentLinearPosition, currentAngularPosition;

    public bool hasAngularLimit;
    public float angularLimitLow;
    public float angularLimitHigh;
    public bool hasLinearStartLimit;
    public bool hasLinearEndLimit;
    public float linearLimitStart;
    public float linearLimitEnd;

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.CYLINDRICAL;
    }

    protected override void WriteJointInternal(System.IO.BinaryWriter writer)
    {
        writer.Write(basePoint);
        writer.Write(axis);

        //1 indicates a linear limit.
        writer.Write((byte)((hasAngularLimit ? 1 : 0) | (hasLinearStartLimit ? 2 : 0) | (hasLinearEndLimit ? 4 : 0)));
        if (hasAngularLimit)
        {
            writer.Write(angularLimitLow);
            writer.Write(angularLimitHigh);
        }
        if (hasLinearStartLimit)
        {
            writer.Write(linearLimitStart);
        }
        if (hasLinearEndLimit)
        {
            writer.Write(linearLimitEnd);
        }

        writer.Write(currentLinearPosition);
        writer.Write(currentAngularPosition);
    }

    protected override void ReadJointInternal(System.IO.BinaryReader reader)
    {
        basePoint = reader.ReadRWObject<BXDVector3>();
        axis = reader.ReadRWObject<BXDVector3>();

        byte limits = reader.ReadByte();
        hasAngularLimit = (limits & 1) == 1;
        hasLinearStartLimit = (limits & 2) == 2;
        hasLinearEndLimit = (limits & 4) == 4;

        if (hasAngularLimit)
        {
            angularLimitLow = reader.ReadSingle();
            angularLimitHigh = reader.ReadSingle();
        }
        if (hasLinearStartLimit)
        {
            linearLimitStart = reader.ReadSingle();
        }
        if (hasLinearEndLimit)
        {
            linearLimitEnd = reader.ReadSingle();
        }

        currentLinearPosition = reader.ReadSingle();
        currentAngularPosition = reader.ReadSingle();
    }
}
