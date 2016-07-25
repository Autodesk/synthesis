/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef DIGITAL_SOURCE_H
#define DIGITAL_SOURCE_H

#include "InterruptableSensorBase.h"

/**
 * DigitalSource Interface.
 * The DigitalSource represents all the possible inputs for a counter or a quadrature encoder. The source may be
 * either a digital input or an analog input. If the caller just provides a channel, then a digital input will be
 * constructed and freed when finished for the source. The source can either be a digital input or analog trigger
 * but not both.
 */
class DigitalSource: public InterruptableSensorBase
{
public:
	virtual ~DigitalSource();
	virtual uint32_t GetChannelForRouting() = 0;
	virtual uint32_t GetModuleForRouting() = 0;
	virtual bool GetAnalogTriggerForRouting() = 0;
	virtual void RequestInterrupts(tInterruptHandler handler, void *param) = 0;
	virtual void RequestInterrupts() = 0;
};

#endif

