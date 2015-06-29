/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "AnalogTrigger.h"

#include "AnalogChannel.h"
#include "AnalogModule.h"
#include "NetworkCommunication/UsageReporting.h"
#include "Resource.h"
#include "WPIErrors.h"

static Resource *triggers = NULL;

/**
 * Initialize an analog trigger from a slot and channel.
 * This is the common code for the two constructors that use a slot and channel.
 */
void AnalogTrigger::InitTrigger(uint8_t moduleNumber, uint32_t channel)
{
	Resource::CreateResourceObject(&triggers, tAnalogTrigger::kNumSystems);
	uint32_t index = triggers->Allocate("Analog Trigger");
	if (index == ~0ul)
	{
		CloneError(triggers);
		return;
	}
	m_index = (uint8_t)index;
	m_channel = channel;
	m_analogModule = AnalogModule::GetInstance(moduleNumber);

	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_trigger = tAnalogTrigger::create(m_index, &localStatus);
	m_trigger->writeSourceSelect_Channel(m_channel - 1, &localStatus);
	m_trigger->writeSourceSelect_Module(moduleNumber - 1, &localStatus);
	wpi_setError(localStatus);

	nUsageReporting::report(nUsageReporting::kResourceType_AnalogTrigger, m_channel, moduleNumber - 1);
}

/**
 * Constructor for an analog trigger given a channel number.
 * The default module is used in this case.
 *
 * @param channel The analog channel (1..8).
 */
AnalogTrigger::AnalogTrigger(uint32_t channel)
{
	InitTrigger(GetDefaultAnalogModule(), channel);
}

/**
 * Constructor for an analog trigger given both the slot and channel.
 *
 * @param moduleNumber The analog module (1 or 2).
 * @param channel The analog channel (1..8).
 */
AnalogTrigger::AnalogTrigger(uint8_t moduleNumber, uint32_t channel)
{
	InitTrigger(moduleNumber, channel);
}

/**
 * Construct an analog trigger given an analog channel.
 * This should be used in the case of sharing an analog channel between the trigger
 * and an analog input object.
 */
AnalogTrigger::AnalogTrigger(AnalogChannel *channel)
{
	InitTrigger(channel->GetModuleNumber(), channel->GetChannel());
}

AnalogTrigger::~AnalogTrigger()
{
	triggers->Free(m_index);
	delete m_trigger;
}

/**
 * Set the upper and lower limits of the analog trigger.
 * The limits are given in ADC codes.  If oversampling is used, the units must be scaled
 * appropriately.
 */
void AnalogTrigger::SetLimitsRaw(int32_t lower, int32_t upper)
{
	if (StatusIsFatal()) return;
	if (lower > upper)
	{
		wpi_setWPIError(AnalogTriggerLimitOrderError);
	}
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_trigger->writeLowerLimit(lower, &localStatus);
	m_trigger->writeUpperLimit(upper, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Set the upper and lower limits of the analog trigger.
 * The limits are given as floating point voltage values.
 */
void AnalogTrigger::SetLimitsVoltage(float lower, float upper)
{
	if (StatusIsFatal()) return;
	if (lower > upper)
	{
		wpi_setWPIError(AnalogTriggerLimitOrderError);
	}
	// TODO: This depends on the averaged setting.  Only raw values will work as is.
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_trigger->writeLowerLimit(m_analogModule->VoltsToValue(m_channel, lower), &localStatus);
	m_trigger->writeUpperLimit(m_analogModule->VoltsToValue(m_channel, upper), &localStatus);
	wpi_setError(localStatus);
}

/**
 * Configure the analog trigger to use the averaged vs. raw values.
 * If the value is true, then the averaged value is selected for the analog trigger, otherwise
 * the immediate value is used.
 */
void AnalogTrigger::SetAveraged(bool useAveragedValue)
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	if (m_trigger->readSourceSelect_Filter(&localStatus) != 0)
		wpi_setWPIErrorWithContext(IncompatibleMode, "Hardware does not support average and filtering at the same time.");
	m_trigger->writeSourceSelect_Averaged(useAveragedValue, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Configure the analog trigger to use a filtered value.
 * The analog trigger will operate with a 3 point average rejection filter. This is designed to
 * help with 360 degree pot applications for the period where the pot crosses through zero.
 */
void AnalogTrigger::SetFiltered(bool useFilteredValue)
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	if (m_trigger->readSourceSelect_Averaged(&localStatus) != 0)
		wpi_setWPIErrorWithContext(IncompatibleMode, "Hardware does not support average and filtering at the same time.");
	m_trigger->writeSourceSelect_Filter(useFilteredValue, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Return the index of the analog trigger.
 * This is the FPGA index of this analog trigger instance.
 * @return The index of the analog trigger.
 */
uint32_t AnalogTrigger::GetIndex()
{
	if (StatusIsFatal()) return ~0ul;
	return m_index;
}

/**
 * Return the InWindow output of the analog trigger.
 * True if the analog input is between the upper and lower limits.
 * @return The InWindow output of the analog trigger.
 */
bool AnalogTrigger::GetInWindow()
{
	if (StatusIsFatal()) return false;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool result = m_trigger->readOutput_InHysteresis(m_index, &localStatus) != 0;
	wpi_setError(localStatus);
	return result;
}

/**
 * Return the TriggerState output of the analog trigger.
 * True if above upper limit.
 * False if below lower limit.
 * If in Hysteresis, maintain previous state.
 * @return The TriggerState output of the analog trigger.
 */
bool AnalogTrigger::GetTriggerState()
{
	if (StatusIsFatal()) return false;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool result = m_trigger->readOutput_OverLimit(m_index, &localStatus) != 0;
	wpi_setError(localStatus);
	return result;
}

/**
 * Creates an AnalogTriggerOutput object.
 * Gets an output object that can be used for routing.
 * Caller is responsible for deleting the AnalogTriggerOutput object.
 * @param type An enum of the type of output object to create.
 * @return A pointer to a new AnalogTriggerOutput object.
 */
AnalogTriggerOutput *AnalogTrigger::CreateOutput(AnalogTriggerOutput::Type type)
{
	if (StatusIsFatal()) return NULL;
	return new AnalogTriggerOutput(this, type);
}

