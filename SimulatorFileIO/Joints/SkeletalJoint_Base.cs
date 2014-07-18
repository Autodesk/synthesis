using System;

/// <summary>
/// Possible types of skeletal joints.
/// </summary>
public enum SkeletalJointType : byte
{
    ROTATIONAL = 1,
    LINEAR = 2,
    PLANAR = 3,
    CYLINDRICAL = 4,
    BALL = 5
}

/// <summary>
/// Generic structure for creating skeletal joints from a joint type.
/// </summary>
public interface SkeletalJointFactory
{
    SkeletalJoint_Base Create(SkeletalJointType type);
}

/// <summary>
/// Basic factory for creating the API joint objects based on type.
/// </summary>
public class BaseSkeletalJointFactory : SkeletalJointFactory
{
    public SkeletalJoint_Base Create(SkeletalJointType type)
    {
        switch (type)
        {
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
            default:
                return null;
        }
    }
}

/// <summary>
/// Represents a moving joint between two nodes.
/// </summary>
public abstract class SkeletalJoint_Base
{
    /// <summary>
    /// Factory object used to create skeletal joint objects when reading skeletons from a file.
    /// </summary>
    public static SkeletalJointFactory baseFactory = new BaseSkeletalJointFactory();

    /// <summary>
    /// The joint driver for this joint.  This can be null.
    /// </summary>
    public JointDriver cDriver;

    /// <summary>
    /// The type of this joint.
    /// </summary>
    /// <returns>The joint type</returns>
    public abstract SkeletalJointType GetJointType();

    /// <summary>
    /// Writes the backing information for this joint to the output stream.
    /// </summary>
    /// <param name="writer">Output stream</param>
    public abstract void WriteJoint(System.IO.BinaryWriter writer);

    /// <summary>
    /// Reads the backing information for this joint from the input stream.
    /// </summary>
    /// <param name="reader">Input stream</param>
    protected abstract void ReadJoint(System.IO.BinaryReader reader);

    /// <summary>
    /// Identifies the type of a joint, creates an instance, and reads that joint from the given input stream.
    /// </summary>
    /// <param name="reader">Input stream</param>
    /// <returns>The created joint</returns>
    public static SkeletalJoint_Base ReadJointFully(System.IO.BinaryReader reader)
    {
        SkeletalJointType type = (SkeletalJointType)((int)reader.ReadByte());
        SkeletalJoint_Base joint = baseFactory.Create(type);
        joint.ReadJoint(reader);
        return joint;
    }

    protected virtual string ToString_Internal()
    {
        return Enum.GetName(typeof(SkeletalJointType), GetJointType());
    }

    public override string ToString()
    {
        string info = ToString_Internal();
        if (cDriver != null)
        {
            info += "\n Driver: " + cDriver.ToString().Replace("\n", "\n ");
        }
        return info;
    }
}