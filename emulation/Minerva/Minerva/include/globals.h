#ifndef GLOBALS_H
#define GLOBALS_H

#include "HAL/handles/IndexedHandleResource.h"
#include "HAL/handles/DigitalHandleResource.h"
#include "HAL/Types.h"

namespace minerva{
	hal::DigitalHandleResource<HAL_DigitalHandle,minerva::DigitalPort,minerva::constants::HAL::kNumDigitalChannels + minerva::constants::HAL::kNumPWMHeaders> digitalChannelHandles;
}

#endif
