using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class PhysicalProperties
{
    public BXDVector3 centerOfMass = new BXDVector3();
    public float mass;

    public void WriteData(BinaryWriter writer)
    {
        writer.Write(centerOfMass.x);
        writer.Write(centerOfMass.y);
        writer.Write(centerOfMass.z);
        writer.Write(mass);
    }

    public void ReadData(BinaryReader reader)
    {
        centerOfMass = new BXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        mass = reader.ReadSingle();
    }
}
