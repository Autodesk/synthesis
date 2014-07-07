using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

public enum SkeletalJointType : byte
{
    ROTATIONAL = 1, LINEAR = 2, PLANAR = 3, CYLINDRICAL = 4, BALL = 5, RIGID = 6
}

public interface SkeletalJointFactory
{
     SkeletalJoint_Base Create(SkeletalJointType type);
}

public class BaseSkeletalJointFactory : SkeletalJointFactory {
    public SkeletalJoint_Base Create(SkeletalJointType type){
        switch(type){
            case SkeletalJointType.ROTATIONAL:
                return new RotationalJoint_Base();
            case SkeletalJointType.LINEAR:
                return new LinearJoint_Base();
            case SkeletalJointType.CYLINDRICAL:
                return new CylindricalJoint_Base();
            case SkeletalJointType.PLANAR:
                return new PlanarJoint_Base();
            case SkeletalJointType.BALL:
                return new BallJoint_Base();
            case SkeletalJointType.RIGID:
                return new RigidJoint_Base();
            default:
                return null;
        }
    }
}

public abstract class SkeletalJoint_Base
{
    public static SkeletalJointFactory baseFactory = new BaseSkeletalJointFactory();

    public JointDriver cDriver;

    public abstract SkeletalJointType GetJointType();

    public abstract void WriteJoint(System.IO.BinaryWriter writer);

    protected abstract void ReadJoint(System.IO.BinaryReader reader);

    public static SkeletalJoint_Base ReadJointFully(System.IO.BinaryReader reader)
    {
        SkeletalJointType type = (SkeletalJointType) ((int)reader.ReadByte());
        SkeletalJoint_Base joint = baseFactory.Create(type);
        joint.ReadJoint(reader);
        return joint;
    }

    protected virtual string ToString_Internal()
    {
        return Enum.GetName(typeof(SkeletalJointType), GetJointType());
    }

    public override string ToString() {
        string info = ToString_Internal();
        if (cDriver != null)
        {
            info += " driven by " + Enum.GetName(typeof(JointDriverType), cDriver.GetDriveType()).Replace('_', ' ').ToLowerInvariant() + " (" + cDriver.portA + (JointDriver.HasTwoPorts(cDriver.GetDriveType())?","+cDriver.portB:"")+")";
        }
        return info;
    }
}