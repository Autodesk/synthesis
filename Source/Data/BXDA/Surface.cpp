#include "Surface.h"

using namespace BXDA;

Surface::Surface()
{
	hasColor = 0;
	color = 0xFFFFFFFF;
	specular = 0;
}

Surface::~Surface()
{
	for (Triangle * triangle : triangles)
		delete triangle;
}

Surface::Surface(const Surface * s) : triangles(s->triangles.size())
{
	this->color = s->color;
	this->hasColor = s->hasColor;
	this->translucency = s->translucency;
	this->transparency = s->transparency;
	this->specular = s->specular;

	for (Triangle * triangle : s->triangles)
		triangles.push_back(new Triangle(triangle));
}

Surface::Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular, const vector<int> & indices) : triangles(indices.size() / 3)
{
	this->hasColor = hasColor;
	this->color = color;
	this->transparency = transparency;
	this->translucency = translucency;
	this->specular = specular;

	for (int i = 0; i < indices.size(); i += 3)
		triangles.push_back(new Triangle(indices[i], indices[i + 1], indices[i + 2]));
}
