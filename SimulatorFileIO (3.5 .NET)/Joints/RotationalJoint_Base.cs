using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

public class RotationalJoint_Base : SkeletalJoint_Base
{

    public BXDVector3 parentNormal;
    public BXDVector3 childNormal;
    public BXDVector3 parentBase;
    public BXDVector3 childBase;
    public double currentAngularPosition;
    public bool hasAngularLimit;
    public double angularLimitLow;
    public double angularLimitHigh;

    public override SkeletalJointType getJointType()
    {
        return SkeletalJointType.ROTATIONAL;
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

        writer.Write((byte)(hasAngularLimit ? 1 : 0));
        if (hasAngularLimit)
        {
            writer.Write(angularLimitLow);
            writer.Write(angularLimitHigh);
        }
    }

    protected override void readJoint(System.IO.BinaryReader reader)
    {
        parentBase = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        parentNormal = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        childBase = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        childNormal = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());

        hasAngularLimit = (reader.ReadByte() & 1) == 1;
        if (hasAngularLimit)
        {
            angularLimitLow = reader.ReadDouble();
            angularLimitHigh = reader.ReadDouble();
        }
    }
}