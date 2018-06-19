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
		//OUT.write((char*)bxda->Version, sizeof(unsigned int));
		//for (Submesh * s : bxda->meshes) {
		//	//int count = s->verts->count();
		//	unsigned char meshflag = (unsigned char)(s->norms != nullptr ? 1 : 0);

		//	OUT.write((char*)meshflag, sizeof(unsigned char));

		//}

		return 1;
	}
	catch (...) {
		return 0;
	}
}