#ifndef _ROBORIO_H_
#define _ROBORIO_H_

#include <array>
#include <memory>
#include <mutex>
#include <queue>

#include "HAL/ChipObject.h"
#include "athena/PortsInternal.h"

namespace hel{
    using namespace nFPGA;
    using namespace nRoboRIO_FPGANamespace;

    struct RoboRIO{
        struct AnalogOutputs{
		private:
        	std::array<uint16_t, tAO::kNumMXPRegisters> mxp_outputs;
		
		public:
			uint16_t getMXPOutput(uint8_t)const;
			void setMXPOutput(uint8_t,uint16_t);
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
		private:
			tDIO::tDO outputs;
            tDIO::tOutputEnable enabled_outputs;
            tDIO::tPulse pulses;
            tDIO::tDI inputs;
            
			uint16_t mxp_special_functions_enabled;//TODO this needs to be checked when attempting things

			uint8_t pulse_length;        	

			std::array<uint8_t, hal::kNumDigitalPWMOutputs> pwm; //TODO unclear whether these are mxp pins or elsewhere (there are only six here whereas there are ten on the mxp)
	
		public:
			tDIO::tDO getOutputs()const;
			void setOutputs(tDIO::tDO);	

			tDIO::tOutputEnable getEnabledOutputs()const;
			void setEnabledOutputs(tDIO::tOutputEnable);

			uint16_t getMXPSpecialFunctionsEnabled()const;
			void setMXPSpecialFunctionsEnabled(uint16_t);
	
			tDIO::tPulse getPulses()const;
			void setPulses(tDIO::tPulse);

			tDIO::tDI getInputs()const;
			void setInputs(tDIO::tDI);

			uint8_t getPulseLength()const;
			void setPulseLength(uint8_t);
        
			uint8_t getPWMDutyCycle(uint8_t)const;
			void setPWMDutyCycle(uint8_t, uint8_t);
		};

		struct CANBus{
			struct Message{
				uint32_t id;
				std::array<uint8_t, 8> data;
				uint8_t data_size: 4;
				uint32_t time_stamp;

				static constexpr int32_t CAN_SEND_PERIOD_NO_REPEAT = 0;
				static constexpr int32_t CAN_SEND_PERIOD_STOP_REPEATING = -1;

				static constexpr uint32_t CAN_IS_FRAME_REMOTE = 0x80000000;
				static constexpr uint32_t CAN_IS_FRAME_11BIT = 0x40000000;

				static constexpr uint32_t CAN_29BIT_MESSAGE_ID_MASK = 0x1FFFFFFF;
				static constexpr uint32_t CAN_11BIT_MESSAGE_ID_MASK = 0x000007FF;
			};
		private:
			std::queue<Message> in_message_queue;
			std::queue<Message> out_message_queue;
	
		public:
			void enqueueMessage(Message);
			Message getNextMessage()const;
			void popNextMessage();
		};

        AnalogInputs analog_inputs;
        AnalogOutputs analog_outputs;
		CANBus can_bus;
        DIOSystem digital_system;
        PWMSystem pwm_system;

        explicit RoboRIO() = default;

        friend class RoboRIOManager;
    private:
        RoboRIO(RoboRIO const&) = default;
    };

    class RoboRIOManager {

    public:
        static std::shared_ptr<RoboRIO> getInstance() {
            if (instance == nullptr) {
                instance = std::make_shared<RoboRIO>();
            }
            return instance;
        }
        static RoboRIO getCopy() {
            return RoboRIO((*RoboRIOManager::getInstance()));
        }
    private:
        RoboRIOManager() {}
        static std::shared_ptr<RoboRIO> instance;
    public:
        RoboRIOManager(RoboRIOManager const&) = delete;
        void operator=(RoboRIOManager const&) = delete;
    };

    static RoboRIO roborio_state;
}

#endif
