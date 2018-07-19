#include "BinaryRWObject.h"

BinaryWriter::BinaryWriter(string file)
{
	outputFile.open(file, ios::out | ios::binary);
}

BinaryWriter::~BinaryWriter()
{

}

bool BinaryWriter::Write(BXDA::Mesh * mesh)
{
	if (!outputFile.is_open())
		return false;

	outputFile << mesh->getGUID();
	outputFile << mesh->getVersion();

	/*
	for (int i = 0; i < countS; i++)
	{
		Submesh * temp = bxda->submeshes[i];
		countV = temp->verts.size() / 3;
		countN = temp->norms.size() / 3;
		countI = 0;

		unsigned char meshflag = countN > 0 ? 1 : 0;
		ofs << meshflag;

		ofs << (unsigned char)(countV * 3);

		for (double d : temp->verts)
		{
			ofs << (unsigned char)d;
		}

		ofs << (countN * 3);
		for (double d : temp->norms)
		{
			ofs << (unsigned char)d;
		}
		delete temp;
	}
	*/

	outputFile.close();

	return true;
}