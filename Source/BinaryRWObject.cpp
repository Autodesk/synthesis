#include "BinaryRWObject.h"

using namespace BXDATA;
using namespace std;

BinaryWriter::BinaryWriter(string file) {
	ofs.open(file, ios::out | ios::binary);
}

BinaryWriter::~BinaryWriter() {
	
}

bool BinaryWriter::Write(BXDA * bxda) {
	if (ofs.is_open() == true) {
		ofs << bxda->GUID;
		ofs << (unsigned char)bxda->Version;

		int countV = 0, countN = 0, countI = 0;
		size_t countS = bxda->meshes.size();
		for (int i = 0; i < countS; i++) {
			Submesh * temp = bxda->meshes[i];
			countV = temp->verts.size() / 3;
			countN = temp->norms.size() / 3;
			countI = 0;

			unsigned char meshflag = countN > 0 ? 1 : 0;
			ofs << meshflag;

			ofs << (unsigned char)(countV * 3);

			for (double d : temp->verts) {
				ofs << (unsigned char)d;
			}

			ofs << (countN * 3);
			for (double d : temp->norms) {
				ofs << (unsigned char)d;
			}
			delete temp;
		}

		ofs.close();
		return 0;
	}
	else {
		//error when opening the file
		return 1;
	}
}