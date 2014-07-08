using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Base class all joint metadata should inherit from.
/// </summary>
/// <remarks>
/// If you inherit make sure to add your type to <see cref="JointDriverMeta.JOINT_DRIVER_TYPES"/>, 
/// and make sure you have a public constructor that takes no arguments.
/// </remarks>
public abstract class JointDriverMeta
{
    // Constant, but can't be declared so.
    private static Type[] JOINT_DRIVER_TYPES = new Type[] { typeof(WheelDriverMeta) };

    protected abstract void WriteDataInternal(BinaryWriter writer);
    protected abstract void ReadDataInternal(BinaryReader reader);

    public void WriteData(BinaryWriter writer)
    {
        // Find my ID
        int myID = -1;
        for (int i = 0; i < JOINT_DRIVER_TYPES.Length; i++)
        {
            if (JOINT_DRIVER_TYPES[i].Equals(GetType()))
            {
                myID = i;
                break;
            }
        }
        if (myID < 0)
        {
            throw new Exception("Unknown Driver Meta.  Did you register your type?");
        }
        writer.Write((byte) myID);
        WriteDataInternal(writer);
    }

    public static JointDriverMeta ReadDriverMeta(BinaryReader reader)
    {
        int type = reader.ReadByte();
        JointDriverMeta meta = (JointDriverMeta) JOINT_DRIVER_TYPES[type].GetConstructor(new Type[0]).Invoke(new Object[0]);
        meta.ReadDataInternal(reader);
        return meta;
    }
}
