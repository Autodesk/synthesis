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
		
	private:
		std::array<AnalogOutput, tAO::kNumMXPRegisters> analog_outputs;
		std::array<AnalogInput, hal::kNumAnalogInputs> analog_inputs;
		
	};

	static RoboRIO roborio;
}

#endif
