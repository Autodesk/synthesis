#include "Physics.h"

using namespace BXDATA;

/*
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
*/

Physics::Physics() {

}

Physics::Physics(Vector3 * v) {
	COM = new Vector3(v);
}

Physics::~Physics() {

}