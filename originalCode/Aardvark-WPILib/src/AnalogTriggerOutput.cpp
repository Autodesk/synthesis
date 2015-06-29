/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "AnalogTriggerOutput.h"
#include "AnalogTrigger.h"
#include "NetworkCommunication/UsageReporting.h"
#include "WPIErrors.h"

/**
 * Create an object that represents one of the four outputs from an analog trigger.
 * 
 * Because this class derives from DigitalSource, it can be passed into routing functions
 * for Counter, Encoder, etc.
 * 
 * @param trigger A pointer to the trigger for which this is an output.
 * @param outputType An enum that specifies the output on the trigger to represent.
 */
AnalogTriggerOutput::AnalogTriggerOutput(AnalogTrigger *trigger, AnalogTriggerOutput::Type outputType)
	: m_trigger (trigger)
	, m_outputType (outputType)
{
	nUsageReporting::report(nUsageReporting::kResourceType_AnalogTriggerOutput, trigger->GetIndex(), outputType);
}

AnalogTriggerOutput::~AnalogTriggerOutput()
{
}

/**
 * Get the state of the analog trigger output.
 * @return The state of the analog trigger output.
 */
bool AnalogTriggerOutput::Get()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool result = false;
	switch(m_outputType)
	{
	case kInWindow:
		result = m_trigger->m_trigger->readOutput_InHysteresis(m_trigger->m_index, &localStatus);
	case kState:
		result = m_trigger->m_trigger->readOutput_OverLimit(m_trigger->m_index, &localStatus);
	case kRisingPulse:
	case kFallingPulse:
		wpi_setWPIError(AnalogTriggerPulseOutputError);
		return false;
	}
	wpi_setError(localStatus);
	return result;
}

/**
 * @return The value to be written to the channel field of a routing mux.
 */
uint32_t AnalogTriggerOutput::GetChannelForRouting()
{
	return (m_trigger->m_index << 2) + m_outputType;
}

/**
 * @return The value to be written to the module field of a routing mux.
 */
uint32_t AnalogTriggerOutput::GetModuleForRouting()
{
	return m_trigger->m_index >> 2;
}

/**
 * @return The value to be written to the module field of a routing mux.
 */
bool AnalogTriggerOutput::GetAnalogTriggerForRouting()
{
	return true;
}

/**
 * Request interrupts asynchronously on this analog trigger output.
 * TODO: Hardware supports interrupts on Analog Trigger outputs... WPILib should too
 */
void AnalogTriggerOutput::RequestInterrupts(tInterruptHandler handler, void *param)
{
}

/**
 * Request interrupts synchronously on this analog trigger output.
 * TODO: Hardware supports interrupts on Analog Trigger outputs... WPILib should too
 */
void AnalogTriggerOutput::RequestInterrupts()
{
}

