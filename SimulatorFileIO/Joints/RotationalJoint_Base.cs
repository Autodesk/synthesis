using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

public enum WheelPosition : byte
{
    NOTWHEEL = 0, FRONTLEFT = 1, FRONTRIGHT = 2, BACKLEFT = 3, BACKRIGHT = 4
}

public class RotationalJoint_Base : SkeletalJoint_Base
{

    public BXDVector3 parentNormal;
    public BXDVector3 childNormal;
    public BXDVector3 parentBase;
    public BXDVector3 childBase;
    public float currentAngularPosition;
    public bool hasAngularLimit;
    public float angularLimitLow;
    public float angularLimitHigh;
    public WheelPosition wheelPosition = WheelPosition.NOTWHEEL;

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.ROTATIONAL;
    }

    public override void WriteJoint(System.IO.BinaryWriter writer)
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

        writer.Write((byte)((hasAngularLimit ? 1 : 0)));
        if (hasAngularLimit)
        {
            writer.Write(angularLimitLow);
            writer.Write(angularLimitHigh);
        }

        //TODO: Takes up a lot of space.  Need to learn better way to do this.
        writer.Write((int)this.wheelPosition);
    }

    protected override void ReadJoint(System.IO.BinaryReader reader)
    {
        parentBase = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        parentNormal = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        childBase = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        childNormal = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        hasAngularLimit = (reader.ReadByte() & 1) == 1;
        if (hasAngularLimit)
        {
            angularLimitLow = reader.ReadSingle();
            angularLimitHigh = reader.ReadSingle();
        }

        wheelPosition = (WheelPosition)reader.ReadInt32();
    }
}