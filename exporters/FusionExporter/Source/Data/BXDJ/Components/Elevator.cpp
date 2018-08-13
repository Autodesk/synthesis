#include "Elevator.h"

using namespace BXDJ;

Elevator::Elevator(const Elevator & elevatorToCopy)
{
	type = elevatorToCopy.type;
}

Elevator::Elevator(Type type)
{
	this->type = type;
}

rapidjson::Value Elevator::getJSONObject(rapidjson::MemoryPoolAllocator<>& allocator) const
{
	rapidjson::Value elevatorJSON;
	elevatorJSON.SetObject();

	elevatorJSON.AddMember("type", rapidjson::Value((int)type), allocator);

	return elevatorJSON;
}

void Elevator::loadJSONObject(const rapidjson::Value & elevatorJSON)
{
	if (elevatorJSON.IsObject())
	{
		if (elevatorJSON["type"].IsNumber())
			type = (Elevator::Type)elevatorJSON["type"].GetInt();
	}
}

void Elevator::write(XmlWriter & output) const
{
	output.startElement("ElevatorDriverMeta");
	output.writeAttribute("DriverMetaID", "2");

	output.writeElement("ElevatorType", toString(type));

	output.endElement();
}

std::string Elevator::toString(Type type)
{
	switch (type)
	{
	case SINGLE:				return "NOT_MULTI";
	case CASCADING_STAGE_1:		return "CASCADING_STAGE_1";
	case CASCADING_STAGE_2:		return "CASCADING_STAGE_2";
	case CONTINUOUS_STAGE_1:	return "CONTINUOUS_STAGE_1";
	case CONTINUOUS_STAGE_2:	return "CONTINUOUS_STAGE_2";
	}

	return "UNKNOWN";
}
