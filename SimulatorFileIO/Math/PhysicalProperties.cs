using System.IO;

public class PhysicalProperties : RWObject
{
    public BXDVector3 centerOfMass = new BXDVector3();
    public float mass;

    public void WriteData(BinaryWriter writer)
    {
        writer.Write(centerOfMass);
        writer.Write(mass);
    }

    public void ReadData(BinaryReader reader)
    {
        centerOfMass = reader.ReadRWObject<BXDVector3>();
        mass = reader.ReadSingle();
    }
}
