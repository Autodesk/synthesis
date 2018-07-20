#include "Surface.h"

using namespace BXDA;

Surface::Surface()
{
	hasColor = false;
	color = 0xFFFFFFFF;
	transparency = 0;
	translucency = 0;
	specular = 0.2f;
}

Surface::~Surface()
{
	for (Triangle * triangle : triangles)
		delete triangle;
}

Surface::Surface(const Surface & s) : triangles(s.triangles.size())
{
	color = s.color;
	hasColor = s.hasColor;
	translucency = s.translucency;
	transparency = s.transparency;
	specular = s.specular;

	for (int t = 0; t < s.triangles.size(); t++)
		triangles[t] = new Triangle(*s.triangles[t]);
}

Surface::Surface(const std::vector<int> & indices) : Surface()
{
	triangles = std::vector<Triangle *>(indices.size() / 3);

	for (int i = 0; i < indices.size(); i += 3)
		triangles[i / 3] = new Triangle(indices[i], indices[i + 1], indices[i + 2]);
}

Surface::Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular)
{
	this->hasColor = hasColor;
	this->color = color;
	this->transparency = transparency;
	this->translucency = translucency;
	this->specular = specular;
}

Surface::Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular, const std::vector<int> & indices) : Surface(indices)
{
	this->hasColor = hasColor;
	this->color = color;
	this->transparency = transparency;
	this->translucency = translucency;
	this->specular = specular;
}

void Surface::addTriangles(const std::vector<Triangle> & triangles)
{
	for (Triangle triangle : triangles)
		this->triangles.push_back(new Triangle(triangle));
}

void BXDA::Surface::addTriangles(const Surface & surface)
{
	for (Triangle * triangle : surface.triangles)
		triangles.push_back(new Triangle(*triangle));
}

void Surface::offsetIndices(int offset)
{
	for (Triangle * tri : triangles)
	{
		tri->vertexIndices[0] += offset;
		tri->vertexIndices[1] += offset;
		tri->vertexIndices[2] += offset;
	}
}

void Surface::write(BinaryWriter & output) const
{
	// Output color
	output.write(hasColor);
	if (hasColor)
		output.write((int)color);

	// Output other material information
	output.write(transparency);
	output.write(translucency);
	output.write(specular);

	// Output triangles
	output.write((int)triangles.size() * 3);
	for (Triangle * triangle : triangles)
		output.write(*triangle);
}