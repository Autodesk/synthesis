#include "ChipObject/PWMDecoder.h"
#include <NetworkCommunication/UsageReporting.h>	// For object types

const float PWMDecoder::kDefaultPwmPeriod = 5.05;
const float PWMDecoder::kDefaultPwmCenter = 1.5;

/// Decodes the PWM value of the given DIO into a scalar.
/// @param dio The digital IO module
/// @reg_index The zero-based PWM port
float PWMDecoder::decodePWM(nFPGA::tDIO_Impl *dio, uint8_t reg_index) {
	tRioStatusCode status;
	int32_t value = dio->readPWMValue(reg_index, &status);
	double loopTime = dio->readLoopTiming(&status)/(kSystemClockTicksPerMicrosecond*1e3);

	double max, deadbandMax, center, deadbandMin, min;
	uint8_t type = dio->pwmTypes[reg_index];

	// Shamelessly taken from the various WPILib classes
	switch (type) {
	case nUsageReporting::kResourceType_Victor:
		max = 2.027;
		deadbandMax = 1.525;
		center = 1.507;
		deadbandMin = 1.49;
		min = 1.026;
		break;
	case nUsageReporting::kResourceType_Jaguar:
		max=2.31;
		deadbandMax = 1.55;
		center = 1.507;
		deadbandMin= 1.454;
		min = .697;
		break;
	case nUsageReporting::kResourceType_Servo:
		max = 2.27;
		deadbandMax = 1.513;
		center = 1.507;
		deadbandMin = 1.5;
		min = 0.743;
		break;
	case nUsageReporting::kResourceType_PWM:
	case nUsageReporting::kResourceType_Talon:
		max = 2.037;
		deadbandMax = 1.539;
		center = 1.513;
		deadbandMin = 1.487;
		min = 0.989;
		break;
	default:
		return 0;
	}

	// Also from WPILib
	int32_t m_maxPwm = (int32_t)((max-kDefaultPwmCenter)/loopTime+kDefaultPwmStepsDown-1);
	int32_t m_deadbandMaxPwm = (int32_t)((deadbandMax-kDefaultPwmCenter)/loopTime+kDefaultPwmStepsDown-1);
	int32_t m_deadbandMinPwm = (int32_t)((deadbandMin-kDefaultPwmCenter)/loopTime+kDefaultPwmStepsDown-1);
	int32_t m_minPwm = (int32_t)((min-kDefaultPwmCenter)/loopTime+kDefaultPwmStepsDown-1);

	if (value == kPwmDisabled)
	{
		return 0.0;
	}
	else if (value > m_maxPwm)
	{
		return 1.0;
	}
	else if (value < m_minPwm)
	{
		return -1.0;
	}
	else if (value > m_deadbandMaxPwm)
	{
		return (float)(value - m_deadbandMaxPwm) / (float) (m_maxPwm - m_deadbandMaxPwm);
	}
	else if (value < m_deadbandMinPwm)
	{
		return (float)(value - m_deadbandMinPwm) / (float)(m_deadbandMinPwm - m_minPwm);
	}
	else
	{
		return 0.0;
	}
}