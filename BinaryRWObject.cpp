#include "BinaryRWObject.h"

using namespace BXDATA;

BinaryWriter::BinaryWriter(string file) {
	OUT.open(file, ios::out | ios::binary);
}

BinaryWriter::~BinaryWriter() {
	OUT.close();
}

bool BinaryWriter::Write(BXDA * bxda) {
	try {
		OUT.write((char*)&bxda, sizeof(BXDA));
		return 1;
	}
	catch (...) {
		return 0;
	}
}