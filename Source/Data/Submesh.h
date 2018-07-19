#pragma once

#include <vector>
#include "LVector3.h"
#include "Surface.h"

using namespace std;

namespace BXDATA
{
	class Submesh
	{
	public:
		Submesh();
		~Submesh();

		Submesh(Submesh*);	//copy constructor

		//Constructor for the whole mesh
		Submesh(LVector3* verts, LVector3* norms);

		Submesh(LVector3* verts, LVector3* norms, list<Surface*> surfaces);

		void addSurface(Surface*);

		vector<double> verts;
		vector<double> norms;

		list <Surface*> surfaces;		//Should be private
	};
}