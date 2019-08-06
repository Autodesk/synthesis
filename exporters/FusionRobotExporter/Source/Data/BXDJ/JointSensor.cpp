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

rapidjson::Value JointSensor::getJSONObject(rapidjson::MemoryPoolAllocator<>& allocator) const
{
	rapidjson::Value sensorJSON;
	sensorJSON.SetObject();

	sensorJSON.AddMember("type", rapidjson::Value((int)type), allocator);
	sensorJSON.AddMember("signal", rapidjson::Value((int)portSignal), allocator);
	sensorJSON.AddMember("portA", rapidjson::Value(portA), allocator);
	sensorJSON.AddMember("portB", rapidjson::Value(portB), allocator);
	sensorJSON.AddMember("conversionFactor", rapidjson::Value(conversionFactor), allocator);

	return sensorJSON;
}

void JointSensor::loadJSONObject(const rapidjson::Value & sensorJSON)
{
	if (sensorJSON.IsObject())
	{
		if (sensorJSON["type"].IsNumber())
			type = (Type)sensorJSON["type"].GetInt();
		if (sensorJSON["signal"].IsNumber())
			portSignal = (Signal)sensorJSON["signal"].GetInt();
		if (sensorJSON["portA"].IsNumber())
			portA = sensorJSON["portA"].GetInt();
		if (sensorJSON["portB"].IsNumber())
			portB = sensorJSON["portB"].GetInt();
		if (sensorJSON["conversionFactor"].IsNumber())
			conversionFactor = sensorJSON["conversionFactor"].GetDouble();
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
