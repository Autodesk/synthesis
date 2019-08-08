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

nlohmann::json Elevator::getJSONObject() const
{
	nlohmann::json elevatorJSON;

	elevatorJSON["type"] = (int)type;

	return elevatorJSON;
}

void Elevator::loadJSONObject(nlohmann::json elevatorJSON)
{
	if (elevatorJSON.is_object())
	{
		if (elevatorJSON["type"].is_number())
			type = (Elevator::Type)elevatorJSON["type"].get<int>();
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
