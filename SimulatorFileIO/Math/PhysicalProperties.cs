using System.IO;

public class PhysicalProperties : RWObject
{
    public BXDVector3 centerOfMass = new BXDVector3();
    public float mass;

    public void Add(float addMass, BXDVector3 addCOM)
    {
        this.centerOfMass.Multiply(mass);
        this.centerOfMass.Add(addCOM.Copy().Multiply(addMass));
        this.mass += addMass;
        this.centerOfMass.Multiply(1.0f / this.mass);
    }

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
