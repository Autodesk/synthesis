using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

public enum SkeletalJointType : byte
{
    ROTATIONAL = 1, LINEAR = 2, PLANAR = 3, CYLINDRICAL = 4
}

public interface SkeletalJointFactory
{
     SkeletalJoint_Base create(SkeletalJointType type);
}

public class BaseSkeletalJointFactory : SkeletalJointFactory {
    public SkeletalJoint_Base create(SkeletalJointType type){
        switch(type){
            case SkeletalJointType.ROTATIONAL:
                return new RotationalJoint_Base();
            case SkeletalJointType.LINEAR:
                return new LinearJoint_Base();
            default:
                return null;
        }
    }
}

public abstract class SkeletalJoint_Base
{
    public static SkeletalJointFactory baseFactory = new BaseSkeletalJointFactory();

    public JointDriver cDriver;

    public abstract SkeletalJointType getJointType();

    public abstract void writeJoint(System.IO.BinaryWriter writer);

    protected abstract void readJoint(System.IO.BinaryReader reader);

    public static SkeletalJoint_Base readJointFully(System.IO.BinaryReader reader)
    {
        SkeletalJointType type = (SkeletalJointType) ((int)reader.ReadByte());
        SkeletalJoint_Base joint = baseFactory.create(type);
        joint.readJoint(reader);
        return joint;
    }

    protected virtual string ToString_Internal()
    {
        return Enum.GetName(typeof(SkeletalJointType), getJointType());
    }

    public override string ToString() {
        string info = ToString_Internal();
        if (cDriver != null)
        {
            info += " driven by " + Enum.GetName(typeof(JointDriverType), cDriver.getDriveType()).Replace('_', ' ').ToLowerInvariant() + " (" + cDriver.portA + (JointDriver.hasTwoPorts(cDriver.getDriveType())?","+cDriver.portB:"")+")";
        }
        return info;
    }
}