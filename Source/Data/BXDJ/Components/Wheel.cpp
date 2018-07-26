#include "Wheel.h"
#include "../Joints/RotationalJoint.h"

using namespace BXDJ;

Wheel::Wheel(const Wheel & wheelToCopy)
{
	radius = wheelToCopy.radius;
	width = wheelToCopy.width;
	type = wheelToCopy.type;
}

Wheel::Wheel(const RotationalJoint & joint, Type type)
{
	this->type = type;

	// Calculate radius and width
	BXDA::Mesh mesh(joint.getChild()->getGUID());
	joint.getChild()->getMesh(mesh);
	mesh.calculateWheelShape(joint.getAxisOfRotation(), joint.getChildBasePoint(), radius, width);
}

double Wheel::getRadius() const
{
	return radius;
}

double Wheel::getWidth() const
{
	return width;
}

Vector3<float> Wheel::getCenter() const
{
	return Vector3<float>();
}

void Wheel::write(XmlWriter & output) const
{
	output.startElement("WheelDriverMeta");
	output.writeAttribute("DriverMetaID", "0");

	output.writeElement("WheelType", toString(type));
	output.writeElement("WheelRadius", std::to_string(getRadius()));
	output.writeElement("WheelWidth", std::to_string(getWidth()));

	output.startElement("BXDVector3");
	output.writeAttribute("VectorID", "WheelCenter");
	output.write(getCenter());
	output.endElement();

	output.writeElement("ForwardAsympSlip", std::to_string(0.0f));
	output.writeElement("ForwardAsympValue", std::to_string(0.0f));
	output.writeElement("ForwardExtremeSlip", std::to_string(0.0f));
	output.writeElement("ForwardExtremeValue", std::to_string(0.0f));
	output.writeElement("SideAsympSlip", std::to_string(0.0f));
	output.writeElement("SideAsympValue", std::to_string(0.0f));
	output.writeElement("SideExtremeSlip", std::to_string(0.0f));
	output.writeElement("SideExtremeValue", std::to_string(0.0f));
	output.writeElement("IsDriveWheel", std::to_string(false));

	output.endElement();
}

std::string Wheel::toString(Type type)
{
	switch (type)
	{
	case NORMAL:  return "NORMAL";
	case OMNI:    return "OMNI";
	case MECANUM: return "MECANUM";
	}

	return "UNKNOWN";
}
