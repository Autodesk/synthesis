#pragma once
#include "BXDA.h"

using namespace std;

namespace BXDATA {
	class BinaryWriter {
	public:
		BinaryWriter(string file);		//Open Operation
		~BinaryWriter();				//For close operation

		bool Write(BXDA obj);			//Original write operation to be overriden

	private:

	};

	class BinaryReader {
	public:

	private:

	};
}