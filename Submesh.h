#pragma once
#include "LVector3.h"
#include "Surface.h"
#include <list>

using namespace std;
namespace BXDATA {
	class Submesh {
	public:
		Submesh();
		~Submesh();

		//Constructor for the whole mesh
		Submesh(LVector3* verts, LVector3* norms);

		Submesh(LVector3* verts, LVector3* norms, list<Surface*> surfaces);

		void addSurface(Surface*);


		LVector3 * verts;				//Should be private
		LVector3 * norms;				//Should be private

		list <Surface*> surfaces;		//Should be private
	};
}