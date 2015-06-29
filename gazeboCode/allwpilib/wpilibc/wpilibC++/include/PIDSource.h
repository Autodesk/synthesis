/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/
#pragma once

/**
 * PIDSource interface is a generic sensor source for the PID class.
 * All sensors that can be used with the PID class will implement the PIDSource that
 * returns a standard value that will be used in the PID code.
 */
class PIDSource
{	
public:
	enum PIDSourceParameter {kDistance, kRate, kAngle};
	virtual double PIDGet() = 0;
};
