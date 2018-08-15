#include "Driver.h"
#include "Joint.h"

using namespace BXDJ;

Driver::Driver(const Driver & driverToCopy)
{
	type = driverToCopy.type;
	portSignal = driverToCopy.portSignal;
	portOne = driverToCopy.portOne;
	portTwo = driverToCopy.portTwo;
	inputGear = driverToCopy.inputGear;
	outputGear = driverToCopy.outputGear;

	// Components
	if (driverToCopy.wheel != nullptr)
		setComponent(*driverToCopy.wheel);

	if (driverToCopy.pneumatic != nullptr)
		setComponent(*driverToCopy.pneumatic);

	if (driverToCopy.elevator != nullptr)
		setComponent(*driverToCopy.elevator);
}

Driver::Driver(Type type)
{
	this->type = type;
	portSignal = PWM;
	portOne = 0;
	portTwo = 0;
	inputGear = 1;
	outputGear = 1;
}

void Driver::write(XmlWriter & output) const
{
	output.startElement("JointDriver");

	output.writeElement("DriveType", toString(type));

	output.writeElement("Port1", std::to_string(portOne + 1)); // Synthesis engine downshifts port numbers due to old code using 1 and 2 for drive.
	output.writeElement("Port2", std::to_string(portTwo + 1)); // For backwards compatibility, ports will be stored one larger than their actual value.

	if (inputGear > 0 && outputGear > 0)
	{
		output.writeElement("InputGear", std::to_string(inputGear));
		output.writeElement("OutputGear", std::to_string(outputGear));
	}

	output.writeElement("LowerLimit", "0.0000"); // These values appear to be deprecated, but they exist in
	output.writeElement("UpperLimit", "0.0000"); // the Inventor exporter, so they will be included for safety.
	
	output.writeElement("SignalType", toString(portSignal));

	// Component Information
	if (wheel != nullptr)
		output.write(*wheel);
	else if (pneumatic != nullptr)
		output.write(*pneumatic);
	else if (elevator != nullptr)
		output.write(*elevator);

	output.endElement();
}

rapidjson::Value Driver::getJSONObject(rapidjson::MemoryPoolAllocator<>& allocator) const
{
	rapidjson::Value driverJSON;
	driverJSON.SetObject();

	driverJSON.AddMember("type", rapidjson::Value((int)type), allocator);
	driverJSON.AddMember("signal", rapidjson::Value((int)portSignal), allocator);
	driverJSON.AddMember("portOne", rapidjson::Value(portOne), allocator);
	driverJSON.AddMember("portTwo", rapidjson::Value(portTwo), allocator);

	// Components
	// Wheel Information
	rapidjson::Value wheelJSON;
	if (wheel != nullptr)
		wheelJSON = wheel->getJSONObject(allocator);

	// Pneumatic Information
	rapidjson::Value pneumaticJSON;
	if (pneumatic != nullptr)
		pneumaticJSON = pneumatic->getJSONObject(allocator);

	// Elevator Information
	rapidjson::Value elevatorJSON;
	if (elevator != nullptr)
		elevatorJSON = elevator->getJSONObject(allocator);

	driverJSON.AddMember("wheel", wheelJSON, allocator);
	driverJSON.AddMember("pneumatic", pneumaticJSON, allocator);
	driverJSON.AddMember("elevator", elevatorJSON, allocator);

	return driverJSON;
}

void Driver::loadJSONObject(const rapidjson::Value & driverJSON)
{
	if (driverJSON.IsObject())
	{
		// Driver Properties
		if (driverJSON.HasMember("type") && driverJSON["type"].IsNumber())
			type = (Driver::Type)driverJSON["type"].GetInt();
		if (driverJSON.HasMember("signal") && driverJSON["signal"].IsNumber())
			portSignal = (Driver::Signal)driverJSON["signal"].GetInt();
		if (driverJSON.HasMember("portOne") && driverJSON["portOne"].IsNumber())
			portOne = driverJSON["portOne"].GetInt();
		if (driverJSON.HasMember("portTwo") && driverJSON["portTwo"].IsNumber())
			portTwo = driverJSON["portTwo"].GetInt();

		// Components

		if (driverJSON.HasMember("wheel") && driverJSON["wheel"].IsObject())
		{
			const rapidjson::Value& wheelJSON = driverJSON["wheel"];
			Wheel wheel;
			wheel.loadJSONObject(wheelJSON);
			setComponent(wheel);
		}
		else if (driverJSON.HasMember("pneumatic") && driverJSON["pneumatic"].IsObject())
		{
			const rapidjson::Value& pneumaticJSON = driverJSON["pneumatic"];
			Pneumatic pneumatic;
			pneumatic.loadJSONObject(pneumaticJSON);
			setComponent(pneumatic);
		}
		else if (driverJSON.HasMember("elevator") && driverJSON["elevator"].IsObject())
		{
			const rapidjson::Value& elevatorJSON = driverJSON["elevator"];
			Elevator elevator;
			elevator.loadJSONObject(elevatorJSON);
			setComponent(elevator);
		}
	}
}

// Component Functions
void Driver::removeComponents()
{
	this->wheel = nullptr;
	this->pneumatic = nullptr;
	this->elevator = nullptr;
}

void Driver::setComponent(Wheel wheel)
{
	removeComponents();
	this->wheel = std::make_unique<Wheel>(wheel);
}

void Driver::setComponent(Pneumatic pneumatic)
{
	removeComponents();
	this->pneumatic = std::make_unique<Pneumatic>(pneumatic);
}

void Driver::setComponent(Elevator elevator)
{
	removeComponents();
	this->elevator = std::make_unique<Elevator>(elevator);
}

std::unique_ptr<Wheel> Driver::getWheel()
{
	if (wheel != nullptr)
		return std::make_unique<Wheel>(*wheel);
	else
		return nullptr;
}

std::unique_ptr<Pneumatic> Driver::getPneumatic()
{
	if (pneumatic != nullptr)
		return std::make_unique<Pneumatic>(*pneumatic);
	else
		return nullptr;
}

std::unique_ptr<Elevator> Driver::getElevator()
{
	if (elevator != nullptr)
		return std::make_unique<Elevator>(*elevator);
	else
		return nullptr;
}

// Static Functions
std::string Driver::toString(Type type)
{
	switch (type)
	{
	case MOTOR:			   return "MOTOR";
	case SERVO:			   return "SERVO";
	case WORM_SCREW:	   return "WORM_SCREW";
	case BUMPER_PNEUMATIC: return "BUMPER_PNEUMATIC";
	case RELAY_PNEUMATIC:  return "RELAY_PNEUMATIC";
	case DUAL_MOTOR:	   return "DUAL_MOTOR";
	case ELEVATOR:		   return "ELEVATOR";
	}

	return "UNKNOWN";
}

std::string Driver::toString(Signal type)
{
	switch (type)
	{
	case PWM: return "PWM";
	case CAN: return "CAN";
	}

	return "UNKNOWN";
}
