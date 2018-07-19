#include "BinaryRWObject.h"

BinaryWriter::BinaryWriter(std::string file)
{
	outputFile.open(file, std::ios::out | std::ios::binary);
}

BinaryWriter::~BinaryWriter()
{

}

bool BinaryWriter::Write(const BXDA::Mesh & mesh)
{
	if (!outputFile.is_open())
		return false;

	outputFile << mesh;

	outputFile.close();

	return true;
}