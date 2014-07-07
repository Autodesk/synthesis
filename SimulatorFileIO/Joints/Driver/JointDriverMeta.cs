using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public enum JointDriverMetaType : byte
{
    WHEEL_DRIVER = 1
}

public abstract class JointDriverMeta
{
    public JointDriverMetaType metaType { get; private set;}

    protected JointDriverMeta(JointDriverMetaType type)
    {
        this.metaType = type;
    }

    protected abstract void WriteDataInternal(BinaryWriter writer);
    protected abstract void ReadDataInternal(BinaryReader reader);

    public static JointDriverMeta Create(JointDriverMetaType type)
    {
        switch (type)
        {
            case JointDriverMetaType.WHEEL_DRIVER:
                return new WheelDriverMeta();
            default:
                return null;
        }
    }

    public void WriteData(BinaryWriter writer)
    {
        writer.Write((byte)metaType);
        WriteDataInternal(writer);
    }

    public static JointDriverMeta ReadDriverMeta(BinaryReader reader)
    {
        JointDriverMetaType type = (JointDriverMetaType)reader.ReadByte();
        JointDriverMeta meta = JointDriverMeta.Create(type);
        meta.ReadDataInternal(reader);
        return meta;
    }
}
