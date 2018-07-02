#ifndef _ROBORIO_H_
#define _ROBORIO_H_

#include "HAL/ChipObject.h"
#include "athena/PortsInternal.h"

#include<array>

namespace minerva{

	using namespace nFPGA;
 	using namespace nRoboRIO_FPGANamespace;

	struct RoboRIO{
		struct AnalogOutput{
				uint16_t mxp_value;
		};

		struct AnalogInput{
			tAI::tConfig config;
			uint8_t oversample_bits;
			uint8_t average_bits;
			uint8_t scan_list;
			tAI::tReadSelect read_select;
		};

		struct PWMSystem{
			tPWM::tConfig config; // configuration for PWM IO
			std::array<uint32_t, tPWM::kNumPeriodScaleHdrElements> hdr_period_scale; //2-bit mask for signal masking frequency, effectively scaling the PWM value (0 = 1x 1, = 2x, 3 = 4x)
			std::array<uint32_t, tPWM::kNumPeriodScaleMXPElements> mxp_period_scale;
			std::array<uint16_t, tPWM::kNumPeriodScaleHdrElements> hdr_values; //these are the pwm values 
			std::array<uint16_t, tPWM::kNumPeriodScaleMXPElements> mxp_values;
		};
		
		struct DIOSystem{
			tDIO::tDO outputs;
			tDIO::tOutputEnable enabled_outputs;
			
			struct PWMData{
				uint8_t id;
				uint8_t hardware_channel;
				uint8_t duty_cycle;			
			};
			
			std::array<PWMData, hal::kNumDigitalPWMOutputs> pwm_data;
			//std::array<uint8_t, 
		};

		std::array<AnalogOutput, tAO::kNumMXPRegisters> analog_outputs;
		std::array<AnalogInput, hal::kNumAnalogInputs> analog_inputs;
		PWMSystem pwm_system;
		DIOSystem digital_system;
	};

	static RoboRIO roborio;
}

#endif
