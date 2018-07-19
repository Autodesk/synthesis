#include "Surface.h"

using namespace BXDA;

Surface::Surface()
{
	hasColor = false;
	color = 0xFFFFFFFF;
	transparency = 0;
	translucency = 0;
	specular = 0.2;
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

std::ostream & BXDA::operator<<(std::ostream & output, const Surface & s)
{
	output << s.hasColor;
	if (s.hasColor)
		output << s.color;

	output << s.transparency;
	output << s.translucency;
	output << s.specular;
}

void Surface::addTriangles(const vector<Triangle>& triangles)
{
	for (Triangle * triangle : triangles)
		this->triangles.push_back(triangle);
}

void BXDA::Surface::addTriangles(const Surface * surface)
{
	for (Triangle * triangle : surface->triangles)
		triangles.push_back(triangle);
}

void Surface::offsetIndices(int offset)
{
	for (Triangle* tri : triangles)
	{
		tri->vertexIndices[0] += offset;
		tri->vertexIndices[1] += offset;
		tri->vertexIndices[2] += offset;
	}
}
