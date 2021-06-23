using System;
using System.IO;

/// <summary>
/// Base class all joint metadata should inherit from.
/// </summary>
/// <remarks>
/// If you inherit make sure to add your type to <see cref="JointDriverMeta.JOINT_DRIVER_META_TYPES"/>, 
/// and make sure you have a public constructor that takes no arguments.
/// </remarks>
public abstract class JointDriverMeta
{
    // Constant, but can't be declared so.
    public readonly static Type[] JOINT_DRIVER_META_TYPES = new Type[] { typeof(WheelDriverMeta), typeof(PneumaticDriverMeta), typeof(ElevatorDriverMeta) };

    protected abstract void WriteDataInternal(BinaryWriter writer);
    protected abstract void ReadDataInternal(BinaryReader reader);

    public int GetID()
    {
        // Find my ID
        int myID = -1;
        for (int i = 0; i < JOINT_DRIVER_META_TYPES.Length; i++)
        {
            if (JOINT_DRIVER_META_TYPES[i].Equals(GetType()))
            {
                myID = i;
                break;
            }
        }
        System.Diagnostics.Debug.Assert(myID >= 0, "Unknown Driver Meta.  Did you register your type?");
        return myID;
    }

    public void WriteData(BinaryWriter writer)
    {
        writer.Write((byte) GetID());
        WriteDataInternal(writer);
    }

    public static JointDriverMeta ReadDriverMeta(BinaryReader reader)
    {
        int type = reader.ReadByte();
        JointDriverMeta meta = (JointDriverMeta) JOINT_DRIVER_META_TYPES[type].GetConstructor(new Type[0]).Invoke(new Object[0]);
        meta.ReadDataInternal(reader);
        return meta;
    }
}
