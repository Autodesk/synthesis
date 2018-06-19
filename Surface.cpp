#include "Surface.h"

using namespace BXDATA;

Surface::Surface() {
	HasColor = 0;
	color = 0xFFFFFFFF;
	specular = 0;
}

Surface::Surface(bool _hasColor, unsigned int _color, float _transparency, float _translucency, float _specular, LVector3* lv) {
	HasColor = _hasColor;
	color = _color;
	transparency = _transparency;
	translucency = _translucency;
	specular = _specular;
	//facet = new LVector3(lv);
}

Surface::Surface(Surface* s) {
	//perform a deep copy
	this->color = s->color;
	this->HasColor = s->HasColor;
	this->translucency = s->translucency;
	this->transparency = s->transparency;
	this->specular = s->specular;
	//this->facet = new LVector3(s->facet);						// May need a deep copy we will see
}

Surface::~Surface() {

}

