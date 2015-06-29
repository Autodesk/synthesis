/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/
#pragma once

#include "Counter.h"

/**
 * Alias for counter class.
 * Implement the gear tooth sensor supplied by FIRST. Currently there is no reverse sensing on
 * the gear tooth sensor, but in future versions we might implement the necessary timing in the
 * FPGA to sense direction.
 */
class GearTooth : public Counter
{
public:
	/// 55 uSec for threshold
	static constexpr double kGearToothThreshold = 55e-6;
	GearTooth(uint32_t channel, bool directionSensitive = false);
	GearTooth(DigitalSource *source, bool directionSensitive = false);
	GearTooth(DigitalSource &source, bool directionSensitive = false);
	virtual ~GearTooth();
	void EnableDirectionSensing(bool directionSensitive);

	virtual std::string GetSmartDashboardType();
};
