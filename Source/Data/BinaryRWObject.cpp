#include "BinaryRWObject.h"

BinaryWriter::BinaryWriter(std::string file)
{
	outputFile.open(file, std::ios::binary);
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

template<typename T>
void BinaryWriter::OutputBytes(T data, std::ostream& output)
{
	char* bytes = (char*)(&data);
	for (int i = 0; i < sizeof(data); i++)
		output << bytes[i];
}