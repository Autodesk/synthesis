#include "Submesh.h"

Submesh::Submesh() {

}

Submesh::~Submesh() {

}

Submesh::Submesh(LVector3* _verts, LVector3* _norms){
	verts = new LVector3(_verts);
	norms = new LVector3(_norms);
}

void Submesh::addSurface(Surface* s) {
	surfaces.push_back(s);
}