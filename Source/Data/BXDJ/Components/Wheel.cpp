#include "Wheel.h"

using namespace BXDJ;

Wheel::Wheel(Driver * driver, Type type) : Component(driver)
{
	this->type = type;
}

float Wheel::getRadius() const
{
	return 0.0f;
}

float Wheel::getWidth() const
{
	return 0.0f;
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
