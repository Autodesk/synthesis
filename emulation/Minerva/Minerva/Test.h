#ifndef TEST_H
#define TEST_H

#include "HAL/Relay.h"

namespace minerva{
	struct Relay{
		enum Value { kOff, kOn, kForward, kReverse };
		enum Direction { kBothDirections, kForwardOnly, kReverseOnly };
		
		void Set(Value);
		Value Get()const;
		
		Direction m_direction;
		HAL_RelayHandle m_forwardHandle = HAL_kInvalidHandle;
		HAL_RelayHandle m_reverseHandle = HAL_kInvalidHandle;
	};
}

#endif