using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

//The position relative to the front of the robot.
public enum WheelPosition
{
    NOWHEEL = 0, FRONTLEFT = 1, FRONTRIGHT = 2, BACKLEFT = 3, BACKRIGHT = 4
}


class WheelDriverMeta : JointDriverMeta
{
    public WheelPosition position {get; set;}

    public WheelDriverMeta()
        : base(JointDriverMetaType.WHEELDRIVER)
    {
    }
    
    //Writes the position of the wheel to the file.
    protected override void writeDataInternal(BinaryWriter writer)
    {
        writer.Write((byte)((int)position));
    }

    //Reads the position of the wheel from the file.
    protected override void readDataInternal(BinaryReader reader)
    {
        position = (WheelPosition)reader.ReadByte();
    }

    WheelPosition getPosition()
    {
        return position;
    }
}

