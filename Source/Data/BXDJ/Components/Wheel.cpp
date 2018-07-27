#include "Wheel.h"
#include "../Joints/RotationalJoint.h"

using namespace BXDJ;

Wheel::Wheel(const Wheel & wheelToCopy)
{
	type = wheelToCopy.type;
	frictionLevel = wheelToCopy.frictionLevel;
	isDriveWheel = wheelToCopy.isDriveWheel;
	radius = wheelToCopy.radius;
	width = wheelToCopy.width;
	center = wheelToCopy.center;
}

Wheel::Wheel(Type type, FrictionLevel frictionLevel, bool isDriveWheel)
{
	this->type = type;
	this->frictionLevel = frictionLevel;
	this->isDriveWheel = isDriveWheel;
	radius = 0;
	width = 0;
	center = Vector3<>();
}

Wheel::Wheel(const Wheel & wheel, const RotationalJoint & joint) : Wheel(wheel)
{
	center = joint.getChildBasePoint();

	// Calculate radius and width
	BXDA::Mesh mesh(joint.getChild()->getGUID());
	joint.getChild()->getMesh(mesh);

	Vector3<> axis = joint.getAxisOfRotation();

	double minWidth, maxWidth;
	mesh.calculateWheelShape(axis, center, minWidth, maxWidth, radius);
	width = maxWidth - minWidth;

	center = center + axis * (width / 2); // Offset center to actual center of wheel
}

double Wheel::getRadius() const
{
	return radius;
}

double Wheel::getWidth() const
{
	return width;
}

Vector3<> Wheel::getCenter() const
{
	return center;
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

	// Friction Info
	output.writeElement("ForwardAsympSlip", std::to_string(1.0f));
	output.writeElement("ForwardExtremeSlip", std::to_string(1.5f));
	output.writeElement("SideAsympSlip", std::to_string(1.0f));
	output.writeElement("SideExtremeSlip", std::to_string(1.5f));

	if (frictionLevel == LOW)
	{
		output.writeElement("ForwardAsympValue", std::to_string(5.0f));
		output.writeElement("ForwardExtremeValue", std::to_string(3.0f));
		if (type != OMNI)
		{
			output.writeElement("SideAsympValue", std::to_string(5.0f));
			output.writeElement("SideExtremeValue", std::to_string(3.0f));
		}
	}
	else if (frictionLevel == MEDIUM)
	{
		output.writeElement("ForwardAsympValue", std::to_string(7.0f));
		output.writeElement("ForwardExtremeValue", std::to_string(5.0f));
		if (type != OMNI)
		{
			output.writeElement("SideAsympValue", std::to_string(7.0f));
			output.writeElement("SideExtremeValue", std::to_string(5.0f));
		}
	}
	else if (frictionLevel == HIGH)
	{
		output.writeElement("ForwardAsympValue", std::to_string(10.0f));
		output.writeElement("ForwardExtremeValue", std::to_string(8.0f));
		if (type != OMNI)
		{
			output.writeElement("SideAsympValue", std::to_string(10.0f));
			output.writeElement("SideExtremeValue", std::to_string(8.0f));
		}
	}

	if (type == OMNI)
	{
		output.writeElement("SideAsympValue", std::to_string(0.01f));
		output.writeElement("SideExtremeValue", std::to_string(0.005f));
	}

	output.writeElement("IsDriveWheel", isDriveWheel ? "true" : "false");

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
