#ifndef TYPES_H
#define TYPES_H

#include <HAL/Encoder.h>
#include <HAL/HAL.h>
#include <HAL/handles/HandlesInternal.h>

#include <variant>
#include <string>
#include <map>
#include <memory>

namespace minerva{
	using HALType = std::variant<
		int32_t, 
		uint32_t, 
		int32_t*, 
		uint64_t, 
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
	
	struct DigitalPort{//TODO change names to match naming conventions?
		uint8_t channel;
		bool configSet = false;
		bool eliminateDeadband = false;
		int32_t maxPwm = 0;
		int32_t deadbandMaxPwm = 0;
		int32_t centerPwm = 0;
		int32_t deadbandMinPwm = 0;
		int32_t minPwm = 0;

		int32_t maxNegativePwm()const;
		int32_t minPositivePwm()const;
		int32_t positiveScaleFactor()const;
		int32_t negativeScaleFactor()const;
		int32_t fullRangeScaleFactor()const;
	};

	struct Relay{
		uint8_t channel;
		bool fwd;	
	};

	/*
	struct AnalogPort{
		uint8_t channel;
		std::unique_ptr<tAccumulator> accumulator;
	};
	*/


}

#endif
