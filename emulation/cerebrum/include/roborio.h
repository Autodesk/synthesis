#ifndef _ROBORIO_H_
#define _ROBORIO_H_

#include "HAL/ChipObject.h"
#include "athena/PortsInternal.h"

#include<array>

namespace cerebrum{

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

		struct PWMSystem{
		private:
			tPWM::tConfig config; // configuration for PWM IO
		
			struct PWM{
				uint32_t period_scale;//2-bit mask for signal masking frequency, effectively scaling the PWM value (0 = 1x 1, = 2x, 3 = 4x)
				uint16_t duty_cycle;
			};
		
			std::array<PWM, tPWM::kNumHdrRegisters> hdr; 
			std::array<PWM, tPWM::kNumMXPRegisters> mxp;
		
		public:
			tPWM::tConfig getConfig()const;
			void setConfig(tPWM::tConfig);

			uint32_t getHdrPeriodScale(uint8_t)const; 
			void setHdrPeriodScale(uint8_t, uint32_t);
			
			uint32_t getMXPPeriodScale(uint8_t)const; 
			void setMXPPeriodScale(uint8_t, uint32_t);
			
			uint32_t getHdrDutyCycle(uint8_t)const; 
			void setHdrDutyCycle(uint8_t, uint32_t);
			
			uint32_t getMXPDutyCycle(uint8_t)const; 
			void setMXPDutyCycle(uint8_t, uint32_t);
		};
		
		struct DIOSystem{
			tDIO::tDO outputs;
			tDIO::tOutputEnable enabled_outputs;
			tDIO::tPulse pulses;
			tDIO::tDI inputs;

			uint8_t pulse_length;

			std::array<uint8_t, hal::kNumDigitalPWMOutputs> pwm;
		};

		std::array<AnalogOutput, tAO::kNumMXPRegisters> analog_outputs;
		PWMSystem pwm_system;
		DIOSystem digital_system;
        AnalogInputs analog_inputs;
	};

	static RoboRIO roborio_state;
}

#endif
