#include "Pneumatic.h"

using namespace BXDJ;

const float Pneumatic::COMMON_WIDTHS[] = { 2.5f, 5.0f, 10.0f };
const float Pneumatic::COMMON_PRESSURES[] = { 10.0f, 20.0f, 60.0f };

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

int Pneumatic::getCommonWidth() const
{
	if (widthMillimeter <= 2.5f)
		return 0;
	else if (widthMillimeter <= 5.0f)
		return 1;
	else
		return 2;
}

int Pneumatic::getCommonPressure() const
{
	if (pressurePSI <= 10.0f)
		return 0;
	else if (pressurePSI <= 20.0f)
		return 1;
	else
		return 2;
}

void Pneumatic::write(XmlWriter & output) const
{
	output.startElement("PneumaticDriverMeta");
	output.writeAttribute("DriverMetaID", "1");

	output.writeElement("WidthMM", std::to_string(widthMillimeter));
	output.writeElement("PressurePSI", std::to_string(pressurePSI));

	output.endElement();
}
