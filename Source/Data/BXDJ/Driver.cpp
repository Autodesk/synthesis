#include "Driver.h"
#include "Joint.h"

using namespace BXDJ;

Driver::Driver(const Driver & driverToCopy)
{
	type = driverToCopy.type;
	portSignal = driverToCopy.portSignal;
	portA = driverToCopy.portA;
	portB = driverToCopy.portB;
	inputGear = driverToCopy.inputGear;
	outputGear = driverToCopy.outputGear;

	// Components
	if (driverToCopy.wheel != nullptr)
		setComponent(*driverToCopy.wheel);
	else
		wheel = nullptr;

	if (driverToCopy.pneumatic != nullptr)
		setComponent(*driverToCopy.pneumatic);
	else
		pneumatic = nullptr;
}

Driver::Driver(Type type)
{
	this->type = type;
	portSignal = PWM;
	portA = -1;
	portB = -1;
	inputGear = 0;
	outputGear = 0;

	wheel = nullptr;
	pneumatic = nullptr;
}

void Driver::write(XmlWriter & output) const
{
	output.startElement("JointDriver");

	output.writeElement("DriveType", toString(type));

	output.writeElement("PortA", std::to_string(portA + 1)); // Synthesis engine downshifts port numbers due to old code using 1 and 2 for drive.
	output.writeElement("PortB", std::to_string(portB + 1)); // For backwards compatibility, ports will be stored one larger than their actual value.

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

	output.endElement();
}

// Component Functions
void Driver::removeComponents()
{
	this->wheel = nullptr;
	this->pneumatic = nullptr;
}

void Driver::setComponent(Wheel wheel)
{
	removeComponents();
	this->wheel = std::make_unique<Wheel>(wheel);
}

void BXDJ::Driver::setComponent(Pneumatic pneumatic)
{
	removeComponents();
	this->pneumatic = std::make_unique<Pneumatic>(pneumatic);
}

std::unique_ptr<Wheel> Driver::getWheel()
{
	if (wheel != nullptr)
		return std::make_unique<Wheel>(*wheel);
	else
		return nullptr;
}

std::unique_ptr<Pneumatic> BXDJ::Driver::getPneumatic()
{
	if (pneumatic != nullptr)
		return std::make_unique<Pneumatic>(*pneumatic);
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
