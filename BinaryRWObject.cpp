#include "BinaryRWObject.h"

using namespace BXDATA;

BinaryWriter::BinaryWriter(string file) {
	ofs.open(file, ios::out | ios::binary);
}

BinaryWriter::~BinaryWriter() {
	ofs.close();
}

bool BinaryWriter::Write(BXDA * & bxda) {
	/*
			int vertCount = verts.Length / 3;
            byte meshFlags = (byte)((norms != null ? 1 : 0));

            writer.Write(meshFlags);
            writer.WriteArray(verts, 0, vertCount * 3);
            if (norms != null)
            {
                writer.WriteArray(norms, 0, vertCount * 3);
            }

            writer.Write(surfaces.Count);
            foreach (BXDASurface surface in surfaces)
            {
                surface.WriteData(writer);
            }
	*/

	try {
		//OUT.write((char*)&bxda, sizeof(BXDA));
		ofs.write((char*)bxda->Version, sizeof(unsigned int));
		int countV, countN, countI;
		//int countS = bxda->meshes.size();
		//for (int i = 0; i < countS; i++) {
		//	Submesh * temp = bxda->meshes[i];
		//	//int count = s->verts->count();
		//	countV = temp->verts.size() / 3;
		//	countN = temp->norms.size() / 3;
		//	countI = 0;

		//	unsigned char meshflag = countN > 0 ? 1 : 0;

		//	ofs.write((char*)meshflag, sizeof(unsigned char));

		//	//byte * buf = new byte[(countV*3) * sizeof(double)];

		//	ofs.write((char*)(countV * 3), sizeof(int));
		//	for (double d : temp->verts) {
		//		//buf[countI] = d;
		//		ofs.write(((char*)((int)d)), sizeof(double));		//loss of precision
		//	}

		//	ofs.write((char*)(countN * 3), sizeof(int));
		//	for (double n : temp->norms) {
		//		//buf[countI] = d;
		//		ofs.write(((char*)((int)n)), sizeof(double));		//loss of precision
		//	}
		//	delete temp;

		}

		return 1;
	}
	catch (...) {
		return 0;
	}
}