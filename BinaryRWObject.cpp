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

		//char* buffer = new char[]; //write all to buffer and then to file


		//ofs.write((char*)&bxda->Version, sizeof(bxda->Version));
		//bitset<8> bit_ver(bxda->Version);
		ofs << bxda->GUID;
		ofs << (unsigned char)bxda->Version;

		int countV = 0, countN = 0, countI = 0, d2 = 0;
		size_t countS = bxda->meshes.size();
		for (int i = 0; i < countS; i++) {
			Submesh * temp = bxda->meshes[i];
			//int count = s->verts->count();
			countV = temp->verts.size() / 3;
			countN = temp->norms.size() / 3;
			countI = 0;

			unsigned char meshflag = countN > 0 ? 1 : 0;
			ofs << meshflag;

			//ofs.write((char*)meshflag, sizeof(unsigned char));

			//byte * buf = new byte[(countV*3) * sizeof(int)]; //this needs to be 8bit or less



			//char buffer1[8];
			//int asdasdasd = 5; //testing
			//_itoa_s((countV * 3), buffer1, 8, 10);
			//ofs.write(buffer1, 8);

			ofs << (unsigned char)(countV * 3);

			//bitset
			/*if (countV == countN) {
				for (int i = 0; i < temp->verts.size; i++) {
					bitset<8> bit_V(&temp->verts[i]);
					bitset<8> bit_N(&temp->norms[i]);

					ofs << bit_V;
					ofs << bit_N;
				}
			}*/

			//ofs << (countV);
			for (double d : temp->verts) {
				//buf[countI] = d;
				//ofs.write(((char*)((int)d)), sizeof(double));		//loss of precision
				d2 = d;
				bitset<8> bit_V(d2);
				ofs << (unsigned char)d;
			}

			//char buffer2[8];
			//_itoa_s((countV * 3), buffer2, 8, 10);
			//ofs.write(buffer2, 8);

			ofs << (countN * 3);
			for (double d : temp->norms) {
				//buf[countI] = d;
				//ofs.write(((char*)((int)n)), sizeof(double));		//loss of precision
				d2 = d;
				bitset<8> bit_N(d2);
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
	

}