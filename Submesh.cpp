#include "Submesh.h"
using namespace BXDATA;

Submesh::Submesh() {

}

Submesh::~Submesh() {
	verts.clear();
	verts.shrink_to_fit();
}

Submesh::Submesh(Submesh* s) {
	verts = s->verts;
	norms = s->norms;
	surfaces = s->surfaces;
}

Submesh::Submesh(LVector3* _verts, LVector3* _norms){
	//verts = new LVector3(_verts);
	//norms = new LVector3(_norms);

	
}

Submesh::Submesh(LVector3* _verts, LVector3* _norms, list<Surface*> _surfaces) {
	//verts = new LVector3(_verts);
	//norms = new LVector3(_norms);
	surfaces = _surfaces;
}

void Submesh::addSurface(Surface* s) {
	surfaces.push_back(s);
}