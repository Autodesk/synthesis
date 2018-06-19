#pragma once
#include "LVector3.h"
#include "Surface.h"
#include <vector>

using namespace std;

namespace BXDATA {
	class Submesh {
	public:
		Submesh();
		~Submesh();

		Submesh(Submesh*);	//copy constructor

		//Constructor for the whole mesh
		Submesh(LVector3* verts, LVector3* norms);

		Submesh(LVector3* verts, LVector3* norms, list<Surface*> surfaces);

		void addSurface(Surface*);


		//LVector3 * verts;				//Should be private
		//LVector3 * norms;				//Should be private

		vector<double> verts;
		vector<double> norms;

		list <Surface*> surfaces;		//Should be private
	};
}