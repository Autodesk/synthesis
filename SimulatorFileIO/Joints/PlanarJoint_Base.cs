/*
 *Purpose: Contains the information for an Inventor planar joint.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

public class PlanarJoint_Base : SkeletalJoint_Base
{
    //Contain the normal to the plane of motion.  Since both define the same plane, maybe we only need one?
    public BXDVector3 parentNormal;
    public BXDVector3 childNormal;

    public override SkeletalJointType getJointType()
    {
        return SkeletalJointType.PLANAR;
    }

    public override void writeJoint(System.IO.BinaryWriter writer)
    {
        writer.Write(parentNormal.x);
        writer.Write(parentNormal.y);
        writer.Write(parentNormal.z);

        writer.Write(childNormal.x);
        writer.Write(childNormal.y);
        writer.Write(childNormal.z);
    }

    protected override void readJoint(System.IO.BinaryReader reader)
    {
        parentNormal = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        childNormal = new BXDVector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
    }
}
