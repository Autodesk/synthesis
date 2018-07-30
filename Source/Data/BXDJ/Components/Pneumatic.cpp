#include "Pneumatic.h"

using namespace BXDJ;

Pneumatic::Pneumatic(const Pneumatic & pneumaticToCopy)
{
	widthMillimeter = pneumaticToCopy.widthMillimeter;
	pressurePSI = pneumaticToCopy.pressurePSI;
}

Pneumatic::Pneumatic(float widthMillimeter, float pressurePSI)
{
	this->widthMillimeter = widthMillimeter;
	this->pressurePSI = pressurePSI;
}

float Pneumatic::getWidth() const
{
	return widthMillimeter;
}

float Pneumatic::getPressure() const
{
	return pressurePSI;
}

void Pneumatic::write(XmlWriter & output) const
{
	output.startElement("PneumaticDriverMeta");
	output.writeAttribute("DriverMetaID", "1");

	output.writeElement("WidthMM", std::to_string(widthMillimeter));
	output.writeElement("PressurePSI", std::to_string(pressurePSI));

	output.endElement();
}
