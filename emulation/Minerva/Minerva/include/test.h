#ifndef TEST_H
#define TEST_H

#include "HAL/Relay.h"

namespace minerva{
	struct Relay{
		enum Value { kOff, kOn, kForward, kReverse };
		enum Direction { kBothDirections, kForwardOnly, kReverseOnly };
		
		void Set(Value);
		Value Get()const;
		
		Direction m_direction = kBothDirections;
		HAL_RelayHandle m_forwardHandle = 1;
		HAL_RelayHandle m_reverseHandle = HAL_kInvalidHandle;
	};
}

#endif