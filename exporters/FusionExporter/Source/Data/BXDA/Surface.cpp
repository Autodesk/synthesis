#include "Surface.h"
#include <Core/Materials/Appearance.h>
#include <Core/Materials/Properties.h>
#include <Core/Materials/ColorProperty.h>
#include <Core/Application/Color.h>

using namespace BXDA;

Surface::Surface()
{
	hasColor = false;
	color = 0xFFFFFFFF;
	transparency = 0;
	translucency = 0;
	specular = 0.1f;
}

Surface::Surface(const Surface & s) : triangles(s.triangles.size())
{
	color = s.color;
	hasColor = s.hasColor;
	translucency = s.translucency;
	transparency = s.transparency;
	specular = s.specular;

	for (int t = 0; t < s.triangles.size(); t++)
		triangles[t] = s.triangles[t];
}

Surface::Surface(const std::vector<int> & indices, int offset) : Surface()
{
	triangles = std::vector<Triangle>(indices.size() / 3);

	for (int i = 0; i < indices.size(); i += 3)
		triangles[i / 3] = Triangle(indices[i] + offset, indices[i + 1] + offset, indices[i + 2] + offset);
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
		this->triangles.push_back(triangle);
}

void BXDA::Surface::addTriangles(const Surface & surface)
{
	for (Triangle triangle : surface.triangles)
		triangles.push_back(triangle);
}

void Surface::offsetIndices(int offset)
{
	for (Triangle tri : triangles)
	{
		tri.vertexIndices[0] += offset;
		tri.vertexIndices[1] += offset;
		tri.vertexIndices[2] += offset;
	}
}

void BXDA::Surface::setColor(unsigned char r, unsigned char g, unsigned char b, unsigned char a)
{
	hasColor = true;
	color = r + g * 0x100 + b * 0x10000 + a * 0x1000000;
}

void Surface::setColor(core::Ptr<core::Material> material, core::Ptr<core::Appearance> appearance)
{
	if (material == nullptr || appearance == nullptr)
		return;

	core::Ptr<core::ColorProperty> colorPropM = material->appearance()->appearanceProperties()->itemById("metal_f0");
	if (colorPropM == nullptr)
		colorPropM = material->appearance()->appearanceProperties()->itemByName("Color");

	core::Ptr<core::ColorProperty> colorPropA = appearance->appearanceProperties()->itemById("metal_f0");
	if (colorPropA == nullptr)
		colorPropA = appearance->appearanceProperties()->itemByName("Color");
		
	if (colorPropM != nullptr && colorPropM->value() != nullptr &&
		colorPropA != nullptr && colorPropA->value() != nullptr)

		setColor(colorPropM->value()->red() * (colorPropA->value()->red() / 255.0),
				 colorPropM->value()->green() * (colorPropA->value()->green() / 255.0),
				 colorPropM->value()->blue() * (colorPropA->value()->blue() / 255.0),
				 colorPropM->value()->opacity() * (colorPropA->value()->opacity() / 255.0));
}

void BXDA::Surface::removeColor()
{
	hasColor = false;
}

void Surface::write(BinaryWriter & output) const
{
	// Output color
	output.write(hasColor);
	if (hasColor)
		output.write(color);

	// Output other material information
	output.write(transparency);
	output.write(translucency);
	output.write(specular);
	
	// Output triangles
	output.write((int)triangles.size() * 3);
	for (Triangle triangle : triangles)
		output.write(triangle);
}