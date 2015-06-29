/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef ANALOG_TRIGGER_H_
#define ANALOG_TRIGGER_H_

#include "AnalogTriggerOutput.h"
#include "SensorBase.h"

class AnalogChannel;
class AnalogModule;

class AnalogTrigger: public SensorBase
{
	friend class AnalogTriggerOutput;
public:
	AnalogTrigger(uint8_t moduleNumber, uint32_t channel);
	explicit AnalogTrigger(uint32_t channel);
	explicit AnalogTrigger(AnalogChannel *channel);
	virtual ~AnalogTrigger();

	void SetLimitsVoltage(float lower, float upper);
	void SetLimitsRaw(int32_t lower, int32_t upper);
	void SetAveraged(bool useAveragedValue);
	void SetFiltered(bool useFilteredValue);
	uint32_t GetIndex();
	bool GetInWindow();
	bool GetTriggerState();
	AnalogTriggerOutput *CreateOutput(AnalogTriggerOutput::Type type);

private:
	void InitTrigger(uint8_t moduleNumber, uint32_t channel);

	uint8_t m_index;
	tAnalogTrigger *m_trigger;
	AnalogModule *m_analogModule;
	uint32_t m_channel;
};

#endif

