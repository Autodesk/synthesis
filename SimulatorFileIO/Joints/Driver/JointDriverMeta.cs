using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public enum JointDriverMetaType : byte
{

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
            default:
                return null;
        }
    }

    public void writeData(BinaryWriter writer)
    {
        writer.Write((byte)metaType);
        WriteDataInternal(writer);
    }

    public static JointDriverMeta readDriverMeta(BinaryReader reader)
    {
        JointDriverMetaType type = (JointDriverMetaType)reader.ReadByte();
        JointDriverMeta meta = JointDriverMeta.Create(type);
        meta.ReadDataInternal(reader);
        return meta;
    }
}
