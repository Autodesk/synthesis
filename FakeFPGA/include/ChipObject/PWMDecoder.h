#ifndef __PWM_DECODER_H
#define __PWM_DECODER_H

#include <stdint.h>

#include "ChipObject/tDIOImpl.h"

class PWMDecoder {
private:
	// From PWM.h
	static const float kDefaultPwmPeriod;
	static const float kDefaultPwmCenter;
	static const int32_t kDefaultPwmStepsDown= 128;
	static const int32_t kPwmDisabled = 0;
	static const uint32_t kSystemClockTicksPerMicrosecond = 40;
public:
	static float decodePWM(nFPGA::tDIO_Impl *dio, uint8_t channel);
};

#endif