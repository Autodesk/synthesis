#include "BinaryWriter.h"

using namespace BXDA;

BinaryWriter::BinaryWriter(std::string file)
{
	outputFile.open(file, std::ios::binary);
}

BinaryWriter::~BinaryWriter()
{
	if (outputFile.is_open())
		outputFile.close();
}

void BinaryWriter::write(const std::string & str)
{
	write((char)str.length());

	for (int c = 0; c < str.length(); c++)
		write((char)str[c]);
}
