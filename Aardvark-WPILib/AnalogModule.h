/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef ANALOG_MODULE_H_
#define ANALOG_MODULE_H_

#include "ChipObject.h"
#include "Module.h"

/**
 * Analog Module class.
 * Each module can independently sample its channels at a configurable rate.
 * There is a 64-bit hardware accumulator associated with channel 1 on each module.
 * The accumulator is attached to the output of the oversample and average engine so that the center
 * value can be specified in higher resolution resulting in less error.
 */
class AnalogModule: public Module
{ 
    friend class Module;

public:
	static const long kTimebase = 40000000; ///< 40 MHz clock
	static const long kDefaultOversampleBits = 0;
	static const long kDefaultAverageBits = 7;
	static constexpr float kDefaultSampleRate = 50000.0;

	void SetSampleRate(float samplesPerSecond);
	float GetSampleRate();
	void SetAverageBits(uint32_t channel, uint32_t bits);
	uint32_t GetAverageBits(uint32_t channel);
	void SetOversampleBits(uint32_t channel, uint32_t bits);
	uint32_t GetOversampleBits(uint32_t channel);
	int16_t GetValue(uint32_t channel);
	int32_t GetAverageValue(uint32_t channel);
	float GetAverageVoltage(uint32_t channel);
	float GetVoltage(uint32_t channel);
	uint32_t GetLSBWeight(uint32_t channel);
	int32_t GetOffset(uint32_t channel);
	int32_t VoltsToValue(int32_t channel, float voltage);

	static AnalogModule* GetInstance(uint8_t moduleNumber);

protected:
	explicit AnalogModule(uint8_t moduleNumber);
	virtual ~AnalogModule();

private:
	static SEM_ID m_registerWindowSemaphore;

	uint32_t GetNumActiveChannels();
	uint32_t GetNumChannelsToActivate();
	void SetNumChannelsToActivate(uint32_t channels);

	tAI *m_module;
	bool m_sampleRateSet;
	uint32_t m_numChannelsToActivate;
};

#endif
