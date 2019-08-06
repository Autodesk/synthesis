#include "JointSensor.h"

using namespace BXDJ;

JointSensor::JointSensor(const JointSensor & sensorToCopy)
{
	type = sensorToCopy.type;
	portSignal = sensorToCopy.portSignal;
	portA = sensorToCopy.portA;
	portB = sensorToCopy.portB;
	conversionFactor = sensorToCopy.conversionFactor;
}

JointSensor::JointSensor(Type type)
{
	this->type = type;
	portSignal = DIO;
	portA = 0;
	portB = 1;
	conversionFactor = 1;
}

nlohmann::json JointSensor::getJSONObject() const
{
	nlohmann::json sensorJSON;
	
	sensorJSON["type"] = (int)type;
	sensorJSON["signal"] = (int)portSignal;
	sensorJSON["portA"] = portA;
	sensorJSON["portB"] = portB;
	sensorJSON["conversionFactor"] = conversionFactor;

	return sensorJSON;
}

void JointSensor::loadJSONObject(nlohmann::json sensorJSON)
{
	if (sensorJSON.is_object())
	{
		if (sensorJSON["type"].is_number())
			type = (Type)sensorJSON["type"].get<int>();
		if (sensorJSON["conTypePortA"].is_number())
			portSignal = (Signal)sensorJSON["signal"].get<int>();
		if (sensorJSON["conTypePortB"].is_number())
			portSignal = (Signal)sensorJSON["signal"].get<int>();
		if (sensorJSON["portA"].is_number())
			portA = sensorJSON["portA"].get<int>();
		if (sensorJSON["portB"].is_number())
			portB = sensorJSON["portB"].get<int>();
		if (sensorJSON["conversionFactor"].is_number())
			conversionFactor = sensorJSON["conversionFactor"].get<double>();
	}
}

nlohmann::json BXDJ::JointSensor::GetExportJSON()
{
	nlohmann::json json;
	json["type"] = type;
	json["signal"] = portSignal;
	json["portA"] = portA;
	json["portB"] = portB;
	json["conversionFactor"] = conversionFactor;
	return json;
}

void JointSensor::write(XmlWriter & output) const
{
	output.startElement("RobotSensor");

	output.writeElement("SensorType", toString(type));
	output.writeElement("SensorPortNumberA", std::to_string(portA));
	output.writeElement("SensorSignalTypeA", toString(portSignal));
	output.writeElement("SensorPortNumberB", std::to_string(portB));
	output.writeElement("SensorSignalTypeB", toString(portSignal));
	output.writeElement("SensorConversionFactor", std::to_string(conversionFactor));

	output.endElement();
}

std::string JointSensor::toString(Type type)
{
	switch (type)
	{
	case ENCODER: return "ENCODER";
	}

	return "UNKNOWN";
}

std::string JointSensor::toString(Signal type)
{
	switch (type)
	{
	case DIO: return "DIO";
	case ANALOG: return "ANALOG";
	}

	return "UNKNOWN";
}
