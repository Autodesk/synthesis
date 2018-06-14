#pragma once
#include "LVector3.h"
#include "Surface.h"
#include <list>

using namespace std;

class Submesh {
public:
	Submesh();
	~Submesh();

	//Constructor for the whole mesh
	Submesh(LVector3, LVector3, Surface*);

;private:
	LVector3 * verts;
	LVector3 * norms;

	list <Surface*> surfaces;

};