#ifndef TYPES_H
#define TYPES_H

#include <HAL/Encoder.h>
#include <HAL/HAL.h>
#include <HAL/handles/HandlesInternal.h>

#include <variant>
#include <string>
#include <map>

namespace minerva{
	using HALType = std::variant<
		int, 
		unsigned int, 
		int*, 
		unsigned long int, 
		double, 
		char*, 
		bool, 
		HAL_EncoderEncodingType, 
		long, 
		HAL_RuntimeType, 
		const char*,
	       	hal::HAL_HandleEnum,
	       	HAL_AllianceStationID
	>;
	using StatusFrame = std::map<std::string, HALType>;
}

#endif
