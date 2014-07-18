/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef ANALOG_CHANNEL_H_
#define ANALOG_CHANNEL_H_

#include "ChipObject.h"
#include "SensorBase.h"
#include "PIDSource.h"
#include "LiveWindow/LiveWindowSendable.h"

class AnalogModule;

/**
 * Analog channel class.
 * 
 * Each analog channel is read from hardware as a 12-bit number representing -10V to 10V.
 * 
 * Connected to each analog channel is an averaging and oversampling engine.  This engine accumulates
 * the specified ( by SetAverageBits() and SetOversampleBits() ) number of samples before returning a new
 * value.  This is not a sliding window average.  The only difference between the oversampled samples and
 * the averaged samples is that the oversampled samples are simply accumulated effectively increasing the
 * resolution, while the averaged samples are divided by the number of samples to retain the resolution,
 * but get more stable values.
 */
class AnalogChannel : public SensorBase, public PIDSource, public LiveWindowSendable
{
public:
	static const uint8_t kAccumulatorModuleNumber = 1;
	static const uint32_t kAccumulatorNumChannels = 2;
	static const uint32_t kAccumulatorChannels[kAccumulatorNumChannels];

	AnalogChannel(uint8_t moduleNumber, uint32_t channel);
	explicit AnalogChannel(uint32_t channel);
	virtual ~AnalogChannel();

	AnalogModule *GetModule();

	int16_t GetValue();
	int32_t GetAverageValue();

	float GetVoltage();
	float GetAverageVoltage();

	uint8_t GetModuleNumber();
	uint32_t GetChannel();

	void SetAverageBits(uint32_t bits);
	uint32_t GetAverageBits();
	void SetOversampleBits(uint32_t bits);
	uint32_t GetOversampleBits();

	uint32_t GetLSBWeight();
	int32_t GetOffset();

	bool IsAccumulatorChannel();
	void InitAccumulator();
	void SetAccumulatorInitialValue(INT64 value);
	void ResetAccumulator();
	void SetAccumulatorCenter(int32_t center);
	void SetAccumulatorDeadband(int32_t deadband);
	INT64 GetAccumulatorValue();
	uint32_t GetAccumulatorCount();
	void GetAccumulatorOutput(INT64 *value, uint32_t *count);
	void SetVoltageForPID(bool shouldUseVoltageForPID);
	
	double PIDGet();
	
	void UpdateTable();
	void StartLiveWindowMode();
	void StopLiveWindowMode();
	std::string GetSmartDashboardType();
	void InitTable(ITable *subTable);
	ITable * GetTable();

private:
	void InitChannel(uint8_t moduleNumber, uint32_t channel);
	uint32_t m_channel;
	AnalogModule *m_module;
	tAccumulator *m_accumulator;
	INT64 m_accumulatorOffset;
	bool m_shouldUseVoltageForPID;
	
	ITable *m_table;
};

#endif
