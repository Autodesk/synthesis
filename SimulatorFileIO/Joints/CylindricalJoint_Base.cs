/*
 *Purpose: Contains the information for an Inventor cylindrical joint.
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

public class CylindricalJoint_Base : SkeletalJoint_Base
{

    public BXDVector3 parentNormal; //The axis of both rotation and movement;
    public BXDVector3 childNormal;
    public BXDVector3 parentBase; //The starting point of the vector.
    public BXDVector3 childBase;

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

        //1 indicates a linear limit.
        writer.Write((byte)((hasAngularLimit ? 1 : 0)|(hasLinearStartLimit?2:0) | (hasLinearEndLimit?4:0)));
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
    }

    protected override void ReadJoint(System.IO.BinaryReader reader)
    {
        parentBase = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        parentNormal = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        childBase = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        childNormal = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

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
    }
}
