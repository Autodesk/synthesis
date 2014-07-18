/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef DIGITAL_MODULE_H_
#define DIGITAL_MODULE_H_

#include "Module.h"
#include "ChipObject.h"

class I2C;

const uint32_t kExpectedLoopTiming = 260;

class DigitalModule: public Module
{
	friend class I2C;
	friend class Module;

protected:
	explicit DigitalModule(uint8_t moduleNumber);
	virtual ~DigitalModule();

public:
	void SetPWM(uint32_t channel, uint8_t value);
	uint8_t GetPWM(uint32_t channel);
	void SetPWMPeriodScale(uint32_t channel, uint32_t squelchMask);
	void SetRelayForward(uint32_t channel, bool on);
	void SetRelayReverse(uint32_t channel, bool on);
	bool GetRelayForward(uint32_t channel);
	uint8_t GetRelayForward();
	bool GetRelayReverse(uint32_t channel);
	uint8_t GetRelayReverse();
	bool AllocateDIO(uint32_t channel, bool input);
	void FreeDIO(uint32_t channel);
	void SetDIO(uint32_t channel, short value);
	bool GetDIO(uint32_t channel);
	uint16_t GetDIO();
	bool GetDIODirection(uint32_t channel);
	uint16_t GetDIODirection();
	void Pulse(uint32_t channel, float pulseLength);
	bool IsPulsing(uint32_t channel);
	bool IsPulsing();
	uint32_t AllocateDO_PWM();
	void FreeDO_PWM(uint32_t pwmGenerator);
	void SetDO_PWMRate(float rate);
	void SetDO_PWMDutyCycle(uint32_t pwmGenerator, float dutyCycle);
	void SetDO_PWMOutputChannel(uint32_t pwmGenerator, uint32_t channel);
	uint16_t GetLoopTiming();

	I2C* GetI2C(uint32_t address);

	static DigitalModule* GetInstance(uint8_t moduleNumber);
	static uint8_t RemapDigitalChannel(uint32_t channel) { return 15 - channel; }; // TODO: Need channel validation
	static uint8_t UnmapDigitalChannel(uint32_t channel) { return 15 - channel; }; // TODO: Need channel validation

private:
	SEM_ID m_digitalSemaphore;
	SEM_ID m_relaySemaphore;
	SEM_ID m_doPwmSemaphore;
	tDIO *m_fpgaDIO;
};

#endif

