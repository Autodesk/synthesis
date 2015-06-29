/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Talon.h"

#include "DigitalModule.h"
#include "NetworkCommunication/UsageReporting.h"
#include "LiveWindow/LiveWindow.h"

/**
 * Common initialization code called by all constructors.
 *
 * Note that the Talon uses the following bounds for PWM values. These values should work reasonably well for
 * most controllers, but if users experience issues such as asymmetric behavior around
 * the deadband or inability to saturate the controller in either direction, calibration is recommended.
 * The calibration procedure can be found in the Talon User Manual available from CTRE.
 * 
 *   - 211 = full "forward"
 *   - 133 = the "high end" of the deadband range
 *   - 129 = center of the deadband range (off)
 *   - 125 = the "low end" of the deadband range
 *   - 49 = full "reverse"
 */
void Talon::InitTalon() {
	SetBounds(2.037, 1.539, 1.513, 1.487, .989);
	SetPeriodMultiplier(kPeriodMultiplier_2X);
	SetRaw(m_centerPwm);

	nUsageReporting::report(nUsageReporting::kResourceType_Talon, GetChannel(), GetModuleNumber() - 1);
	LiveWindow::GetInstance()->AddActuator("Talon", GetModuleNumber(), GetChannel(), this);
}

/**
 * Constructor that assumes the default digital module.
 * 
 * @param channel The PWM channel on the digital module that the Talon is attached to.
 */
Talon::Talon(uint32_t channel) : SafePWM(channel)
{
	InitTalon();
}

/**
 * Constructor that specifies the digital module.
 * 
 * @param moduleNumber The digital module (1 or 2).
 * @param channel The PWM channel on the digital module that the Talon is attached to (1..10).
 */
Talon::Talon(uint8_t moduleNumber, uint32_t channel) : SafePWM(moduleNumber, channel)
{
	InitTalon();
}

Talon::~Talon()
{
}

/**
 * Set the PWM value.  
 * 
 * The PWM value is set using a range of -1.0 to 1.0, appropriately
 * scaling the value for the FPGA.
 * 
 * @param speed The speed value between -1.0 and 1.0 to set.
 * @param syncGroup Unused interface.
 */
void Talon::Set(float speed, uint8_t syncGroup)
{
	SetSpeed(speed);
}

/**
 * Get the recently set value of the PWM.
 * 
 * @return The most recently set value for the PWM between -1.0 and 1.0.
 */
float Talon::Get()
{
	return GetSpeed();
}

/**
 * Common interface for disabling a motor.
 */
void Talon::Disable()
{
	SetRaw(kPwmDisabled);
}

/**
 * Write out the PID value as seen in the PIDOutput base object.
 * 
 * @param output Write out the PWM value as was found in the PIDController
 */
void Talon::PIDWrite(float output) 
{
	Set(output);
}

