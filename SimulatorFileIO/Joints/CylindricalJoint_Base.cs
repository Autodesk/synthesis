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

    public override void WriteJoint(System.IO.BinaryWriter writer)
    {
        writer.Write(basePoint.x);
        writer.Write(basePoint.y);
        writer.Write(basePoint.z);
        writer.Write(axis.x);
        writer.Write(axis.y);
        writer.Write(axis.z);

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
    }

    protected override void ReadJoint(System.IO.BinaryReader reader)
    {
        basePoint = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        axis = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

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
    }
}
