using System.IO;

public class PhysicalProperties : BinaryRWObject
{
    public BXDVector3 centerOfMass = new BXDVector3();
    public float mass;

    public void Add(float addMass, BXDVector3 addCOM)
    {
        centerOfMass.Multiply(mass);
        centerOfMass.Add(addCOM.Copy().Multiply(addMass));
        mass += addMass;
        centerOfMass.Multiply(1.0f / mass);
    }

    public void WriteBinaryData(BinaryWriter writer)
    {
        writer.Write(centerOfMass);
        writer.Write(mass);
    }

    public void ReadBinaryData(BinaryReader reader)
    {
        centerOfMass = reader.ReadRWObject<BXDVector3>();
        mass = reader.ReadSingle();
    }
}
