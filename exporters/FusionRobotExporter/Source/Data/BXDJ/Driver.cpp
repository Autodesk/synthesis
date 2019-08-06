#include "Driver.h"
#include "Joint.h"

using namespace BXDJ;

Driver::Driver(const Driver & driverToCopy)
{
	type = driverToCopy.type;
	motor = driverToCopy.motor;
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
	motor = GENERIC;
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
	output.writeElement("MotorType", toString(motor));

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
	output.writeElement("HasBrake", "false");

	// Component Information
	if (wheel != nullptr)
		output.write(*wheel);
	else if (pneumatic != nullptr)
		output.write(*pneumatic);
	else if (elevator != nullptr)
		output.write(*elevator);

	output.endElement();
}

nlohmann::json Driver::getJSONObject() const
{

	nlohmann::json driverJSON;
	
	driverJSON["type"] = (int)type;
	driverJSON["signal"] = (int)portSignal;
	driverJSON["portOne"] = portOne;
	driverJSON["portTwo"] = portTwo;

	// Components
	// Wheel Information
	nlohmann::json wheelJSON;
	if (wheel != nullptr)
		wheelJSON = wheel->getJSONObject();

	// Pneumatic Information
	nlohmann::json pneumaticJSON;
	if (pneumatic != nullptr)
		pneumaticJSON = pneumatic->getJSONObject();

	// Elevator Information
	nlohmann::json elevatorJSON;
	if (elevator != nullptr)
		elevatorJSON = elevator->getJSONObject();

	driverJSON["wheel"] = wheelJSON;
	driverJSON["pneumatic"] = pneumaticJSON;
	driverJSON["elevator"] = elevatorJSON;

	return driverJSON;
}

nlohmann::json BXDJ::Driver::GetExportJson()
{
	nlohmann::json exportJson;
	if (this == nullptr) {
		return exportJson;
	}
	

	exportJson["port1"] = portOne + 1;
	exportJson["port2"] = portTwo + 1;
	exportJson["isCan"] = portSignal == Driver::Signal::CAN;
	exportJson["lowerLimit"] = 0;
	exportJson["upperLimit"] = 0;
	exportJson["motor"] = motor + 1;
	exportJson["InputGear"] = inputGear;
	exportJson["OutputGear"] = outputGear;
	exportJson["hasBrake"] = false;
	
	nlohmann::json metaJson;
	if (getWheel() != nullptr) {
		metaJson["WheelDriverMeta, SimulatorAPI, Version=0.3.3.1, Culture=neutral, PublicKeyToken=null"] = getWheel()->GetExportJson();
	}

	exportJson["MetaInfo"] = metaJson;
	return exportJson;
}

void Driver::loadJSONObject(const nlohmann::json driverJSON)
{
	if (driverJSON.is_object())
	{
		// Driver Properties

		if (driverJSON["type"].is_number()) {
			type = (Driver::Type)driverJSON["type"].get<int>();
		}

		if (driverJSON["signal"].is_number()) {
			portSignal = (Driver::Signal)driverJSON["signal"].get<int>();
		}

		if (driverJSON["portOne"].is_number()) {
			portOne = driverJSON["portOne"].get<int>();
		}

		if (driverJSON["portTwo"].is_number()) {
			portTwo = driverJSON["portTwo"].get<int>();
		}


		// Components
		if (driverJSON["wheel"].is_object()) {
			nlohmann::json wheelJSON = driverJSON["wheel"];
			Wheel wheel;
			wheel.loadJSONObject(wheelJSON);
			setComponent(wheel);
		}
		else if(driverJSON["pneumatic"].is_object()){
			nlohmann::json pneumaticJSON = driverJSON["pneumatic"];
			Pneumatic pneumatic;
			pneumatic.loadJSONObject(pneumaticJSON);
			setComponent(pneumatic);

		}
		else if (driverJSON["elevator"].is_object())
		{
			nlohmann::json elevatorJSON = driverJSON["elevator"];
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

std::string Driver::toString(Motor type)
{
	switch (type)
	{
	case GENERIC:						return "GENERIC";
	case CIM:							return "CIM";
	case MINI_CIM:						return "MINI_CIM";
	case BAG_MOTOR:						return "BAG_MOTOR";
	case REDLINE_775_PRO:				return "REDLINE_775_PRO";
	case _9015:							return "_9015";
	case BANEBOTS_775_18V:				return "BANEBOTS_775_18v";
	case BANEBOTS_775_12V:				return "BANEBOTS_775_12v";
	case BANEBOTS_550_12V:				return "BANEBOTS_550_12v";
	case ANDYMARK_775_125:				return "ANDYMARK_775_125";
	case SNOW_BLOWER:					return "SNOW_BLOWER";
	case NIDEN_BLDC:					return "NIDEN_BLDC";
	case THROTTLE_MOTOR:				return "THROTTLE_MOTOR";
	case WINDOW_MOTOR:					return "WINDOW_MOTOR";
	case NEVEREST:						return "NEVEREST";
	case TETRIX_MOTOR:					return "TETRIX_MOTOR";
	case MODERN_ROBOTICS_MATRIX_12V:	return "MODERN_ROBOTICS_MATRIX_12V";
	case REV_ROBOTICS_HD_HEX_12V:		return "REV_ROBOTICS_HD_HEX_12V";
	case REV_ROBOTICS_CORE_HEX_12V:		return "REV_ROBOTICS_CORE_HEX_12V";
	case VEX_V5_SMART_MOTOR:			return "VEX_V5_Smart_Motor";
	case VEX_269:						return "VEX_269";
	case VEX_393:						return "VEX_393";
	}

	return "GENERIC";
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
