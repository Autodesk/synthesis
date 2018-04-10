using System;
using System.Collections.Generic;

/// <summary>
/// Possible types of skeletal joints.
/// </summary>
public enum SkeletalJointType : byte
{
    DEFAULT = 0,
    ROTATIONAL = 1,
    LINEAR = 2,
    PLANAR = 3,
    CYLINDRICAL = 4,
    BALL = 5
}

/// <summary>
/// Represents a moving joint between two nodes.
/// </summary>
public abstract class SkeletalJoint_Base
{
    /// <summary>
    /// Generic delegate for creating skeletal joints from a joint type.
    /// </summary>
    public delegate SkeletalJoint_Base SkeletalJointFactory(SkeletalJointType type);

    /// <summary>
    /// Factory object used to create skeletal joint objects when reading skeletons from a file.
    /// </summary>
    public static SkeletalJointFactory JOINT_FACTORY = delegate(SkeletalJointType type)
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
    };

    /// <summary>
    /// The joint driver for this joint.  This can be null.
    /// </summary>
    public JointDriver cDriver;

    /// <summary>
    /// The sensors that read information from this joint.
    /// </summary>
    public List<RobotSensor> attachedSensors = new List<RobotSensor>();

    /// <summary>
    /// The type of this joint.
    /// </summary>
    /// <returns>The joint type</returns>
    public abstract SkeletalJointType GetJointType();

    /// <summary>
    /// Gets all the angular degrees of freedom for this joint.
    /// </summary>
    /// <returns>The angular degrees of freedom</returns>
    public abstract IEnumerable<AngularDOF> GetAngularDOF();
    /// <summary>
    /// Gets all the linear degrees of freedom for this joint.
    /// </summary>
    /// <returns>The linear degrees of freedom</returns>
    public abstract IEnumerable<LinearDOF> GetLinearDOF();

    /// <summary>
    /// Writes the backing information and ID for this joint to the output stream.
    /// </summary>
    /// <param name="writer">Output stream</param>
    public void WriteBinaryJoint(System.IO.BinaryWriter writer)
    {
        writer.Write((byte) ((int) GetJointType()));
        WriteBinaryJointInternal(writer);

        writer.Write(cDriver != null);
        if (cDriver!=null){
            cDriver.WriteBinaryData(writer);
        }
        writer.Write(attachedSensors.Count);
        for (int i = 0; i < attachedSensors.Count; i++)
        {
            attachedSensors[i].WriteBinaryData(writer);
        }
    }
    protected abstract void WriteBinaryJointInternal(System.IO.BinaryWriter writer);

    /// <summary>
    /// Reads the backing information for this joint from the input stream.
    /// </summary>
    /// <param name="reader">Input stream</param>
    public void ReadBinaryJoint(System.IO.BinaryReader reader)
    {
        // ID is already read
        ReadBinaryJointInternal(reader);

        if (reader.ReadBoolean())
        {
            cDriver = new JointDriver(JointDriverType.MOTOR);
            cDriver.ReadBinaryData(reader);
        }
        else
        {
            cDriver = null;
        }
        int sensorCount = reader.ReadInt32();
        attachedSensors = new List<RobotSensor>(sensorCount);
        for (int i = 0; i < sensorCount; i++)
        {
            attachedSensors.Add(RobotSensor.ReadSensorFully(reader));
        }
    }
    protected abstract void ReadBinaryJointInternal(System.IO.BinaryReader reader);

    /// <summary>
    /// Identifies the type of a joint, creates an instance, and reads that joint from the given input stream.
    /// </summary>
    /// <param name="reader">Input stream</param>
    /// <returns>The created joint</returns>
    public static SkeletalJoint_Base ReadJointFully(System.IO.BinaryReader reader)
    {
        SkeletalJointType type = (SkeletalJointType)((int)reader.ReadByte());
        SkeletalJoint_Base joint = JOINT_FACTORY(type);
        joint.ReadBinaryJoint(reader);
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

    public void WriteBinaryData(System.IO.BinaryWriter writer)
    {
        WriteBinaryJoint(writer);
    }

    public void ReadBinaryData(System.IO.BinaryReader reader)
    {
        throw new NotImplementedException("Don't read a joint directly!");
    }
}
