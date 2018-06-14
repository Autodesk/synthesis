#pragma once
#include "Vector3.h"
#include "Surface.h"

class Submesh {
public:
	Submesh();
	~Submesh();

	Submesh(Vector3*, Vector3*);

;private:
	Vector3 * verts;
	Vector3 * norms;

	Surface * surfaces;

};