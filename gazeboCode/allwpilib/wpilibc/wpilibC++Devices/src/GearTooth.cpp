/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "GearTooth.h"
#include "LiveWindow/LiveWindow.h"

constexpr double GearTooth::kGearToothThreshold;

/**
 * Common code called by the constructors.
 */
void GearTooth::EnableDirectionSensing(bool directionSensitive)
{
	if (directionSensitive)
	{
		SetPulseLengthMode(kGearToothThreshold);
	}
}

/**
 * Construct a GearTooth sensor given a channel.
 *
 * @param channel The DIO channel that the sensor is connected to. 0-9 are on-board, 10-25 are on the MXP.
 * @param directionSensitive True to enable the pulse length decoding in hardware to specify count direction.
 */
GearTooth::GearTooth(uint32_t channel, bool directionSensitive)
	: Counter(channel)
{
	EnableDirectionSensing(directionSensitive);
    LiveWindow::GetInstance()->AddSensor("GearTooth", channel, this);
}

/**
 * Construct a GearTooth sensor given a digital input.
 * This should be used when sharing digital inputs.
 *
 * @param source A pointer to the existing DigitalSource object (such as a DigitalInput)
 * @param directionSensitive True to enable the pulse length decoding in hardware to specify count direction.
 */
GearTooth::GearTooth(DigitalSource *source, bool directionSensitive)
	: Counter(source)
{
	EnableDirectionSensing(directionSensitive);
}

/**
 * Construct a GearTooth sensor given a digital input.
 * This should be used when sharing digital inputs.
 *
 * @param source A reference to the existing DigitalSource object (such as a DigitalInput)
 * @param directionSensitive True to enable the pulse length decoding in hardware to specify count direction.
 */
GearTooth::GearTooth(DigitalSource &source, bool directionSensitive): Counter(source)
{
	EnableDirectionSensing(directionSensitive);
}

/**
 * Free the resources associated with a gear tooth sensor.
 */
GearTooth::~GearTooth()
{
}


std::string GearTooth::GetSmartDashboardType() {
	return "GearTooth";
}
