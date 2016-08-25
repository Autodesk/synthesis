/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2014. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/
#pragma once

/**
 * Interface for 3-axis accelerometers
 */
class Accelerometer
{
public:
	virtual ~Accelerometer() {};

	enum Range
	{
		kRange_2G = 0,
		kRange_4G = 1,
		kRange_8G = 2,
		kRange_16G = 3
	};

	/**
	 * Common interface for setting the measuring range of an accelerometer.
	 *
	 * @param range The maximum acceleration, positive or negative, that the
	 * accelerometer will measure.  Not all accelerometers support all ranges.
	 */
	virtual void SetRange(Range range) = 0;

	/**
	 * Common interface for getting the x axis acceleration
	 *
	 * @return The acceleration along the x axis in g-forces
	 */
	virtual double GetX() = 0;

	/**
	 * Common interface for getting the y axis acceleration
	 *
	 * @return The acceleration along the y axis in g-forces
	 */
	virtual double GetY() = 0;

	/**
	 * Common interface for getting the z axis acceleration
	 *
	 * @return The acceleration along the z axis in g-forces
	 */
	virtual double GetZ() = 0;
};
