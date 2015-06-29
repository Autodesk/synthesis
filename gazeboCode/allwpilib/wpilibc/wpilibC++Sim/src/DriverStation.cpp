/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "DriverStation.h"
#include "HAL/cpp/Synchronized.hpp"
#include "Timer.h"
#include "simulation/MainNode.h"
//#include "MotorSafetyHelper.h"
#include "Utility.h"
#include "WPIErrors.h"
#include <string.h>
#include "Log.hpp"
#include "boost/mem_fn.hpp"

// set the logging level
TLogLevel dsLogLevel = logDEBUG;

#define DS_LOG(level) \
    if (level > dsLogLevel) ; \
    else Log().Get(level)

const uint32_t DriverStation::kBatteryChannel;
const uint32_t DriverStation::kJoystickPorts;
const uint32_t DriverStation::kJoystickAxes;
constexpr float DriverStation::kUpdatePeriod;
DriverStation* DriverStation::m_instance = NULL;
uint8_t DriverStation::m_updateNumber = 0;

/**
 * DriverStation contructor.
 * 
 * This is only called once the first time GetInstance() is called
 */
DriverStation::DriverStation()
	: m_digitalOut (0)
	, m_waitForDataSem(0)
	, m_approxMatchTimeOffset(-1.0)
	, m_userInDisabled(false)
	, m_userInAutonomous(false)
	, m_userInTeleop(false)
	, m_userInTest(false)
{
	// Create a new semaphore
	m_waitForDataSem = initializeMultiWait();
	m_waitForDataMutex = initializeMutexNormal();
	m_stateSemaphore = initializeMutexRecursive();
	m_joystickSemaphore = initializeMutexRecursive();

	state = msgs::DriverStationPtr(new msgs::DriverStation());
	stateSub = MainNode::Subscribe("~/ds/state",
		                   &DriverStation::stateCallback, this);
	// TODO: for loop + boost bind
	joysticks[0] = msgs::JoystickPtr(new msgs::Joystick());
	joysticksSub[0] =  MainNode::Subscribe("~/ds/joysticks/0",
		                           &DriverStation::joystickCallback0, this);
	joysticks[1] = msgs::JoystickPtr(new msgs::Joystick());
	joysticksSub[1] =  MainNode::Subscribe("~/ds/joysticks/1",
		                           &DriverStation::joystickCallback1, this);
	joysticks[2] = msgs::JoystickPtr(new msgs::Joystick());
	joysticksSub[2] =  MainNode::Subscribe("~/ds/joysticks/2",
		                           &DriverStation::joystickCallback2, this);
	joysticks[3] = msgs::JoystickPtr(new msgs::Joystick());
	joysticksSub[3] =  MainNode::Subscribe("~/ds/joysticks/5",
	                                   &DriverStation::joystickCallback3, this);
	joysticks[4] = msgs::JoystickPtr(new msgs::Joystick());
	joysticksSub[4] =  MainNode::Subscribe("~/ds/joysticks/4",
	                                   &DriverStation::joystickCallback4, this);
	joysticks[5] = msgs::JoystickPtr(new msgs::Joystick());
	joysticksSub[5] =  MainNode::Subscribe("~/ds/joysticks/5",
	                                   &DriverStation::joystickCallback5, this);

	AddToSingletonList();
}

DriverStation::~DriverStation()
{
	m_instance = NULL;
	deleteMultiWait(m_waitForDataSem);
	deleteMutex(m_waitForDataMutex);
	// TODO: Release m_stateSemaphore and m_joystickSemaphore?
}

/**
 * Return a pointer to the singleton DriverStation.
 */
DriverStation* DriverStation::GetInstance()
{
	if (m_instance == NULL)
	{
		m_instance = new DriverStation();
	}
	return m_instance;
}

/**
 * Read the battery voltage. Hardcoded to 12 volts for Simulation.
 *
 * @return The battery voltage.
 */
float DriverStation::GetBatteryVoltage()
{
	return 12.0; // 12 volts all the time!
}

/**
 * Get the value of the axis on a joystick.
 * This depends on the mapping of the joystick connected to the specified port.
 * 
 * @param stick The joystick to read.
 * @param axis The analog axis value to read from the joystick.
 * @return The value of the axis on the joystick.
 */
float DriverStation::GetStickAxis(uint32_t stick, uint32_t axis)
{
	if (axis < 0 || axis > (kJoystickAxes - 1))
	{
		wpi_setWPIError(BadJoystickAxis);
		return 0.0;
	}
	if (stick < 0 || stick > 5)
	{
		wpi_setWPIError(BadJoystickIndex);
		return 0.0;
	}
	CRITICAL_REGION(m_joystickSemaphore)
	if (joysticks[stick] == NULL || axis >= joysticks[stick]->axes().size())
	{
		return 0.0;
	}
	return joysticks[stick]->axes(axis);
	END_REGION
}

/**
 * The state of a specific button (1 - 12) on the joystick.
 * This method only works in simulation, but is more efficient than GetStickButtons.
 * 
 * @param stick The joystick to read.
 * @param button The button number to check.
 * @return If the button is pressed.
 */
bool DriverStation::GetStickButton(uint32_t stick, uint32_t button)
{
    if (stick < 0 || stick >= 6)
	{
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "stick must be between 0 and 5");
		return false;
	}
	CRITICAL_REGION(m_joystickSemaphore)
	if (joysticks[stick] == NULL || button >= joysticks[stick]->buttons().size())
	{
	        return false;
	}
	return joysticks[stick]->buttons(button-1);
	END_REGION
}

/**
 * The state of the buttons on the joystick.
 * 12 buttons (4 msb are unused) from the joystick.
 *
 * @param stick The joystick to read.
 * @return The state of the buttons on the joystick.
 */
short DriverStation::GetStickButtons(uint32_t stick)
{
    if (stick < 0 || stick >= 6)
	{
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "stick must be between 0 and 5");
		return false;
	}
	short btns = 0, btnid;
	CRITICAL_REGION(m_joystickSemaphore)
	msgs::JoystickPtr joy = joysticks[stick];
	for (btnid = 0; btnid < joy->buttons().size() && btnid < 12; btnid++)
	{
		if (joysticks[stick]->buttons(btnid))
		{
			btns |= (1 << btnid);
		}
	}
	return btns;
	END_REGION
}

// 5V divided by 10 bits
#define kDSAnalogInScaling ((float)(5.0 / 1023.0))

/**
 * Get an analog voltage from the Driver Station.
 * The analog values are returned as voltage values for the Driver Station analog inputs.
 * These inputs are typically used for advanced operator interfaces consisting of potentiometers
 * or resistor networks representing values on a rotary switch.
 *
 * @param channel The analog input channel on the driver station to read from. Valid range is 1 - 4.
 * @return The analog voltage on the input.
 */
float DriverStation::GetAnalogIn(uint32_t channel)
{
	wpi_setWPIErrorWithContext(UnsupportedInSimulation, "GetAnalogIn");
	return 0.0;
}

/**
 * Get values from the digital inputs on the Driver Station.
 * Return digital values from the Drivers Station. These values are typically used for buttons
 * and switches on advanced operator interfaces.
 * @param channel The digital input to get. Valid range is 1 - 8.
 */
bool DriverStation::GetDigitalIn(uint32_t channel)
{
	wpi_setWPIErrorWithContext(UnsupportedInSimulation, "GetDigitalIn");
	return false;
}

/**
 * Set a value for the digital outputs on the Driver Station.
 * 
 * Control digital outputs on the Drivers Station. These values are typically used for
 * giving feedback on a custom operator station such as LEDs.
 * 
 * @param channel The digital output to set. Valid range is 1 - 8.
 * @param value The state to set the digital output.
 */
void DriverStation::SetDigitalOut(uint32_t channel, bool value)
{
    wpi_setWPIErrorWithContext(UnsupportedInSimulation, "SetDigitalOut");
}

/**
 * Get a value that was set for the digital outputs on the Driver Station.
 * @param channel The digital ouput to monitor. Valid range is 1 through 8.
 * @return A digital value being output on the Drivers Station.
 */
bool DriverStation::GetDigitalOut(uint32_t channel)
{
    wpi_setWPIErrorWithContext(UnsupportedInSimulation, "GetDigitalOut");
}

bool DriverStation::IsEnabled()
{
    CRITICAL_REGION(m_stateSemaphore)
    return state != NULL ? state->enabled() : false;
    END_REGION
}

bool DriverStation::IsDisabled()
{
    return !IsEnabled();
}

bool DriverStation::IsAutonomous()
{
    CRITICAL_REGION(m_stateSemaphore)
    return state != NULL ?
      state->state() == msgs::DriverStation_State_AUTO : false;
	END_REGION;
}

bool DriverStation::IsOperatorControl()
{
    return !(IsAutonomous() || IsTest());
}

bool DriverStation::IsTest()
{
    CRITICAL_REGION(m_stateSemaphore)
    return state != NULL ?
      state->state() == msgs::DriverStation_State_TEST : false;
	END_REGION;
}

/**
 * Is the driver station attached to a Field Management System?
 * Note: This does not work with the Blue DS.
 * @return True if the robot is competing on a field being controlled by a Field Management System
 */
bool DriverStation::IsFMSAttached()
{
    return false; // No FMS in simulation
}

/**
 * Return the alliance that the driver station says it is on.
 * This could return kRed or kBlue
 * @return The Alliance enum
 */
DriverStation::Alliance DriverStation::GetAlliance()
{
	// if (m_controlData->dsID_Alliance == 'R') return kRed;
	// if (m_controlData->dsID_Alliance == 'B') return kBlue;
	// wpi_assert(false);
    return kInvalid; // TODO: Support alliance colors
}

/**
 * Return the driver station location on the field
 * This could return 1, 2, or 3
 * @return The location of the driver station
 */
uint32_t DriverStation::GetLocation()
{
    return -1; // TODO: Support locations
}

/**
 * Wait until a new packet comes from the driver station
 * This blocks on a semaphore, so the waiting is efficient.
 * This is a good way to delay processing until there is new driver station data to act on
 */
void DriverStation::WaitForData()
{
	takeMultiWait(m_waitForDataSem, m_waitForDataMutex, SEMAPHORE_WAIT_FOREVER);
}

/**
 * Return the approximate match time
 * The FMS does not currently send the official match time to the robots
 * This returns the time since the enable signal sent from the Driver Station
 * At the beginning of autonomous, the time is reset to 0.0 seconds
 * At the beginning of teleop, the time is reset to +15.0 seconds
 * If the robot is disabled, this returns 0.0 seconds
 * Warning: This is not an official time (so it cannot be used to argue with referees)
 * @return Match time in seconds since the beginning of autonomous
 */
double DriverStation::GetMatchTime()
{
	if (m_approxMatchTimeOffset < 0.0)
		return 0.0;
	return Timer::GetFPGATimestamp() - m_approxMatchTimeOffset;
}

/**
 * Report an error to the DriverStation messages window.
 * The error is also printed to the program console.
 */
void DriverStation::ReportError(std::string error)
{
	std::cout << error << std::endl;
}

/**
 * Return the team number that the Driver Station is configured for
 * @return The team number
 */
uint16_t DriverStation::GetTeamNumber()
{
    return 348;
}

void DriverStation::stateCallback(const msgs::ConstDriverStationPtr &msg)
{
    CRITICAL_REGION(m_stateSemaphore)
    *state = *msg;
	END_REGION;
    giveMultiWait(m_waitForDataSem);
}

void DriverStation::joystickCallback(const msgs::ConstJoystickPtr &msg,
                                     int i)
{
    CRITICAL_REGION(m_joystickSemaphore)
    *(joysticks[i]) = *msg;
	END_REGION;
}

void DriverStation::joystickCallback0(const msgs::ConstJoystickPtr &msg)
{
    joystickCallback(msg, 0);
}

void DriverStation::joystickCallback1(const msgs::ConstJoystickPtr &msg)
{
    joystickCallback(msg, 1);
}

void DriverStation::joystickCallback2(const msgs::ConstJoystickPtr &msg)
{
    joystickCallback(msg, 2);
}

void DriverStation::joystickCallback3(const msgs::ConstJoystickPtr &msg)
{
    joystickCallback(msg, 3);
}

void DriverStation::joystickCallback4(const msgs::ConstJoystickPtr &msg)
{
    joystickCallback(msg, 4);
}

void DriverStation::joystickCallback5(const msgs::ConstJoystickPtr &msg)
{
    joystickCallback(msg, 5);
}
