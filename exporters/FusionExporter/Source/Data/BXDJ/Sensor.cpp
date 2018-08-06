#include "Sensor.h"

using namespace BXDJ;

Sensor::Sensor(const Sensor & sensorToCopy)
{
	type = sensorToCopy.type;
	portSignal = sensorToCopy.portSignal;
	portA = sensorToCopy.portA;
	portB = sensorToCopy.portB;
	conversionFactor = sensorToCopy.conversionFactor;
}

Sensor::Sensor(Type type)
{
	this->type = type;
	portSignal = PWM;
	portA = 0;
	portB = 1;
	conversionFactor = 1;
}

void Sensor::write(XmlWriter & output) const
{
	output.startElement("RobotSensor");

	output.writeElement("SensorType", toString(type));
	output.writeElement("SensorPortNumber1", std::to_string(portA));
	output.writeElement("SensorSignalType1", std::to_string(portSignal));
	output.writeElement("SensorPortNumber2", std::to_string(portB));
	output.writeElement("SensorSignalType2", std::to_string(portSignal));
	output.writeElement("SensorConversionFactor", std::to_string(conversionFactor));

	output.endElement();
}

std::string Sensor::toString(Type type)
{
	switch (type)
	{
	case ENCODER: return "ENCODER";
	}

	return "UNKNOWN";
}

std::string Sensor::toString(Signal type)
{
	switch (type)
	{
	case PWM: return "PWM";
	case CAN: return "CAN";
	}

	return "UNKNOWN";
}
