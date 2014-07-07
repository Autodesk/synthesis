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

    protected abstract void writeDataInternal(BinaryWriter writer);
    protected abstract void readDataInternal(BinaryReader reader);

    public static JointDriverMeta create(JointDriverMetaType type)
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
        writeDataInternal(writer);
    }

    public static JointDriverMeta readDriverMeta(BinaryReader reader)
    {
        JointDriverMetaType type = (JointDriverMetaType)reader.ReadByte();
        JointDriverMeta meta = JointDriverMeta.create(type);
        meta.readDataInternal(reader);
        return meta;
    }
}
