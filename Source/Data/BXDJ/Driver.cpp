#include "Driver.h"
#include "Joint.h"

using namespace BXDJ;

Driver::Driver(const Driver & driverToCopy)
{
	joint = driverToCopy.joint;
	type = driverToCopy.type;
	portSignal = driverToCopy.portSignal;
	portA = driverToCopy.portA;
	portB = driverToCopy.portB;
	inputGear = driverToCopy.inputGear;
	outputGear = driverToCopy.outputGear;
}

Driver::Driver(Joint * joint, Type type)
{
	this->joint = joint;
	this->type = type;
	portSignal = PWM;
	portA = -1;
	portB = -1;
	inputGear = 1;
	outputGear = 1;
}

void BXDJ::Driver::addComponent(std::shared_ptr<Component> component)
{
	components.push_back(component);
}

void BXDJ::Driver::write(XmlWriter & output) const
{
	output.startElement("JointDriver");

	output.writeElement("DriveType", toString(type));

	output.writeElement("PortA", std::to_string(portA + 1)); // Synthesis engine downshifts port numbers due to old code using 1 and 2 for drive.
	output.writeElement("PortB", std::to_string(portB + 1)); // For backwards compatibility, ports will be stored one larger than their actual value.

	if (inputGear > 0)
		output.writeElement("InputGear", std::to_string(inputGear));
	if (outputGear > 0)
		output.writeElement("OutputGear", std::to_string(outputGear));

	output.writeElement("LowerLimit", "0.0000"); // These values appear to be deprecated, but they exist in
	output.writeElement("UpperLimit", "0.0000"); // the Inventor exporter, so they will be included for safety.
	
	output.writeElement("SignalType", toString(portSignal));

	// Component Information
	for (std::shared_ptr<Component> component : components)
		output.write(*component);

	output.endElement();
}

std::string BXDJ::Driver::toString(Type type)
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
