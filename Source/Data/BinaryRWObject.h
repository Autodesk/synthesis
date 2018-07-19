#pragma once

#include <fstream>
#include <bitset>
#include "BXDA/Mesh.h"

class BinaryWriter
{
public:
	BinaryWriter(string file);		//Open Operation
	~BinaryWriter();				//For close operation

	bool Write(BXDA::Mesh * mesh);			//Original write operation to be overriden

private:
	ofstream outputFile;
};