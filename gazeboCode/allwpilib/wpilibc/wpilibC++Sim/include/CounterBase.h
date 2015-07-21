/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/
#pragma once

/**
 * Interface for counting the number of ticks on a digital input channel.
 * Encoders, Gear tooth sensors, and counters should all subclass this so it can be used to
 * build more advanced classes for control and driving.
 *
 * All counters will immediately start counting - Reset() them if you need them
 * to be zeroed before use.
 */
class CounterBase
{
public:
	enum EncodingType
	{
		k1X,
		k2X,
		k4X
	};

	virtual ~CounterBase() {}
	virtual int32_t Get() = 0;
	virtual void Reset() = 0;
	virtual double GetPeriod() = 0;
	virtual void SetMaxPeriod(double maxPeriod) = 0;
	virtual bool GetStopped() = 0;
	virtual bool GetDirection() = 0;
};
