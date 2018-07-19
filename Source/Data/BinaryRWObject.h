#pragma once

#include <fstream>
#include <bitset>
#include "BXDA/Mesh.h"

class BinaryWriter
{
public:
	BinaryWriter(std::string file);	//Open Operation
	~BinaryWriter();				//For close operation

	bool Write(const BXDA::Mesh & mesh);	//Original write operation to be overriden
	template<typename T>
	void OutputBytes(T data, std::ostream& output);

private:
	std::ofstream outputFile;

};