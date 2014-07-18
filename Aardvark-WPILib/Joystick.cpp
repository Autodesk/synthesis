/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Joystick.h"
#include "DriverStation.h"
#include "NetworkCommunication/UsageReporting.h"
#include "WPIErrors.h"
#include <math.h>

const uint32_t Joystick::kDefaultXAxis;
const uint32_t Joystick::kDefaultYAxis;
const uint32_t Joystick::kDefaultZAxis;
const uint32_t Joystick::kDefaultTwistAxis;
const uint32_t Joystick::kDefaultThrottleAxis;
const uint32_t Joystick::kDefaultTriggerButton;
const uint32_t Joystick::kDefaultTopButton;
static Joystick *joysticks[DriverStation::kJoystickPorts];
static bool joySticksInitialized = false;

/**
 * Construct an instance of a joystick.
 * The joystick index is the usb port on the drivers station.
 * 
 * @param port The port on the driver station that the joystick is plugged into.
 */
Joystick::Joystick(uint32_t port)
	: m_ds (NULL)
	, m_port (port)
	, m_axes (NULL)
	, m_buttons (NULL)
{
	InitJoystick(kNumAxisTypes, kNumButtonTypes);

	m_axes[kXAxis] = kDefaultXAxis;
	m_axes[kYAxis] = kDefaultYAxis;
	m_axes[kZAxis] = kDefaultZAxis;
	m_axes[kTwistAxis] = kDefaultTwistAxis;
	m_axes[kThrottleAxis] = kDefaultThrottleAxis;
	
	m_buttons[kTriggerButton] = kDefaultTriggerButton;
	m_buttons[kTopButton] = kDefaultTopButton;

	nUsageReporting::report(nUsageReporting::kResourceType_Joystick, port);
}

/**
 * Version of the constructor to be called by sub-classes.
 * 
 * This constructor allows the subclass to configure the number of constants
 * for axes and buttons.
 * 
 * @param port The port on the driver station that the joystick is plugged into.
 * @param numAxisTypes The number of axis types in the enum.
 * @param numButtonTypes The number of button types in the enum.
 */
Joystick::Joystick(uint32_t port, uint32_t numAxisTypes, uint32_t numButtonTypes)
	: m_ds (NULL)
	, m_port (port)
	, m_axes (NULL)
	, m_buttons (NULL)
{
	InitJoystick(numAxisTypes, numButtonTypes);
}

void Joystick::InitJoystick(uint32_t numAxisTypes, uint32_t numButtonTypes)
{
	if ( !joySticksInitialized )
	{
		for (unsigned i = 0; i < DriverStation::kJoystickPorts; i++)
			joysticks[i] = NULL;
		joySticksInitialized = true;
	}
	joysticks[m_port - 1] = this;
	
	m_ds = DriverStation::GetInstance();
	m_axes = new uint32_t[numAxisTypes];
	m_buttons = new uint32_t[numButtonTypes];
}

Joystick * Joystick::GetStickForPort(uint32_t port)
{
	Joystick *stick = joysticks[port - 1];
	if (stick == NULL)
	{
		stick = new Joystick(port);
		joysticks[port - 1] = stick;
	}
	return stick;
}

Joystick::~Joystick()
{
	delete [] m_buttons;
	delete [] m_axes;
}

/**
 * Get the X value of the joystick.
 * This depends on the mapping of the joystick connected to the current port.
 */
float Joystick::GetX(JoystickHand hand)
{
	return GetRawAxis(m_axes[kXAxis]);
}

/**
 * Get the Y value of the joystick.
 * This depends on the mapping of the joystick connected to the current port.
 */
float Joystick::GetY(JoystickHand hand)
{
	return GetRawAxis(m_axes[kYAxis]);
}

/**
 * Get the Z value of the current joystick.
 * This depends on the mapping of the joystick connected to the current port.
 */
float Joystick::GetZ()
{
	return GetRawAxis(m_axes[kZAxis]);
}

/**
 * Get the twist value of the current joystick.
 * This depends on the mapping of the joystick connected to the current port.
 */
float Joystick::GetTwist()
{
	return GetRawAxis(m_axes[kTwistAxis]);
}

/**
 * Get the throttle value of the current joystick.
 * This depends on the mapping of the joystick connected to the current port.
 */
float Joystick::GetThrottle()
{
	return GetRawAxis(m_axes[kThrottleAxis]);
}

/**
 * Get the value of the axis.
 * 
 * @param axis The axis to read [1-6].
 * @return The value of the axis.
 */
float Joystick::GetRawAxis(uint32_t axis)
{
	return m_ds->GetStickAxis(m_port, axis);
}

/**
 * For the current joystick, return the axis determined by the argument.
 * 
 * This is for cases where the joystick axis is returned programatically, otherwise one of the
 * previous functions would be preferable (for example GetX()).
 * 
 * @param axis The axis to read.
 * @return The value of the axis.
 */
float Joystick::GetAxis(AxisType axis)
{
	switch(axis)
	{
		case kXAxis: return this->GetX();
		case kYAxis: return this->GetY();
		case kZAxis: return this->GetZ();
		case kTwistAxis: return this->GetTwist();
		case kThrottleAxis: return this->GetThrottle();
		default:
			wpi_setWPIError(BadJoystickAxis);
			return 0.0;
	}
}

/**
 * Read the state of the trigger on the joystick.
 * 
 * Look up which button has been assigned to the trigger and read its state.
 * 
 * @param hand This parameter is ignored for the Joystick class and is only here to complete the GenericHID interface.
 * @return The state of the trigger.
 */
bool Joystick::GetTrigger(JoystickHand hand)
{
	return GetRawButton(m_buttons[kTriggerButton]);
}

/**
 * Read the state of the top button on the joystick.
 * 
 * Look up which button has been assigned to the top and read its state.
 * 
 * @param hand This parameter is ignored for the Joystick class and is only here to complete the GenericHID interface.
 * @return The state of the top button.
 */
bool Joystick::GetTop(JoystickHand hand)
{
	return GetRawButton(m_buttons[kTopButton]);
}

/**
 * This is not supported for the Joystick.
 * This method is only here to complete the GenericHID interface.
 */
bool Joystick::GetBumper(JoystickHand hand)
{
	// Joysticks don't have bumpers.
	return false;
}

/**
 * Get the button value for buttons 1 through 12.
 * 
 * The buttons are returned in a single 16 bit value with one bit representing the state
 * of each button. The appropriate button is returned as a boolean value. 
 * 
 * @param button The button number to be read.
 * @return The state of the button.
 **/
bool Joystick::GetRawButton(uint32_t button)
{
	return ((0x1 << (button-1)) & m_ds->GetStickButtons(m_port)) != 0;
}

/**
 * Get buttons based on an enumerated type.
 * 
 * The button type will be looked up in the list of buttons and then read.
 * 
 * @param button The type of button to read.
 * @return The state of the button.
 */
bool Joystick::GetButton(ButtonType button)
{
	switch (button)
	{
	case kTriggerButton: return GetTrigger();
	case kTopButton: return GetTop();
	default:
		return false;
	}
}

/**
 * Get the channel currently associated with the specified axis.
 * 
 * @param axis The axis to look up the channel for.
 * @return The channel fr the axis.
 */
uint32_t Joystick::GetAxisChannel(AxisType axis)
{
	return m_axes[axis];
}

/**
 * Set the channel associated with a specified axis.
 * 
 * @param axis The axis to set the channel for.
 * @param channel The channel to set the axis to.
 */
void Joystick::SetAxisChannel(AxisType axis, uint32_t channel)
{
	m_axes[axis] = channel;
}

/**
 * Get the magnitude of the direction vector formed by the joystick's
 * current position relative to its origin
 * 
 * @return The magnitude of the direction vector
 */
float Joystick::GetMagnitude(){
	return sqrt(pow(GetX(),2) + pow(GetY(),2) );
}

/**
 * Get the direction of the vector formed by the joystick and its origin
 * in radians
 * 
 * @return The direction of the vector in radians
 */
float Joystick::GetDirectionRadians(){
	return atan2(GetX(), -GetY());
}

/**
 * Get the direction of the vector formed by the joystick and its origin
 * in degrees
 * 
 * uses acos(-1) to represent Pi due to absence of readily accessable Pi 
 * constant in C++
 * 
 * @return The direction of the vector in degrees
 */
float Joystick::GetDirectionDegrees(){
	return (180/acos(-1))*GetDirectionRadians();
}
