using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

public class LinearJoint_Base : SkeletalJoint_Base
{

    public BXDVector3 parentNormal;
    public BXDVector3 childNormal;
    public BXDVector3 parentBase;
    public BXDVector3 childBase;
    public double currentLinearPosition;
    public bool hasUpperLimit, hasLowerLimit;
    public double linearLimitLow, linearLimitHigh;
    public override SkeletalJointType getJointType()
    {
        return SkeletalJointType.LINEAR;
    }

    public override void writeJoint(System.IO.BinaryWriter writer)
    {
        writer.Write(parentBase.x);
        writer.Write(parentBase.y);
        writer.Write(parentBase.z);
        writer.Write(parentNormal.x);
        writer.Write(parentNormal.y);
        writer.Write(parentNormal.z);

        writer.Write(childBase.x);
        writer.Write(childBase.y);
        writer.Write(childBase.z);
        writer.Write(childNormal.x);
        writer.Write(childNormal.y);
        writer.Write(childNormal.z);

        writer.Write((byte)((hasLowerLimit ? 1 : 0) | (hasUpperLimit ? 2 : 0)));
        if (hasLowerLimit)
        {
            writer.Write(linearLimitLow);
        }
        if (hasUpperLimit)
        {
            writer.Write(linearLimitHigh);
        }
    }

    protected override void readJoint(System.IO.BinaryReader reader)
    {
        parentBase = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        parentNormal = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        childBase = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        childNormal = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());

        byte limitFlags = reader.ReadByte();
        hasLowerLimit = (limitFlags & 1) == 1;
        hasUpperLimit = (limitFlags & 2) == 2;
        if (hasLowerLimit)
        {
            linearLimitLow = reader.ReadDouble();
        }
        if (hasUpperLimit)
        {
            linearLimitHigh = reader.ReadDouble();
        }
    }
}