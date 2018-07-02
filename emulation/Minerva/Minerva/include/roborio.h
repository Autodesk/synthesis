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
        struct AnalogInputs {
            struct AnalogInput{
                uint8_t oversample_bits;
                uint8_t average_bits;
                uint8_t scan_list;
            };

            void setConfig(tAI::tConfig value);
            tAI::tConfig getConfig();

            void setReadSelect(tAI::tReadSelect);
            tAI::tReadSelect getReadSelect();

            void setOversampleBits(uint8_t, uint8_t);
            void setAverageBits(uint8_t, uint8_t);
            void setScanList(uint8_t, uint8_t);

            uint8_t getOversampleBits(uint8_t);
            uint8_t getAverageBits(uint8_t);
            uint8_t getScanList(uint8_t);

        private:
            std::array<AnalogInput, hal::kNumAnalogInputs> analog_inputs;
            tAI::tConfig config;
            tAI::tReadSelect read_select;
        };

		std::array<AnalogOutput, tAO::kNumMXPRegisters> analog_outputs;
        AnalogInputs analog_inputs;
    };

	static RoboRIO roborio_state;
}

#endif
