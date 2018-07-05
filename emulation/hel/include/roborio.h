#ifndef _ROBORIO_H_
#define _ROBORIO_H_
/**
 * \file roborio.h
 * \brief Defines internal structure of mock RoboRIO
 * This file defines the RoboRIOs structure
 */



#include <array>
#include <memory>
#include <mutex>
#include <queue>

#include "HAL/ChipObject.h"
#include "athena/PortsInternal.h"

namespace hel{
    using namespace nFPGA;
    using namespace nRoboRIO_FPGANamespace;

    /**
     * \struct RoboRIO roborio.h
     * \brief Mock RoboRIO implementation
     *
     * This class represents the internals of the RoboRIO hardware, broken up into several sub-systems:
     * Analog Input, Analog Output, PWM, DIO, SPI, MXP, RS232, and I2C.
     */
    struct RoboRIO{
        /**
         * \struct AnalogOutputs roborio.h
         * \brief Data model for analog outputs.
         * Holds all internal data needed to model analog outputs on the RoboRIO.
         */
        struct AnalogOutputs{
		private:
            /**
             * \var std::array<uint16_t, tAO::kNumMXPRegisters> mxp_outputs
             * \brief Analog output data
             * 
             */
            std::array<uint16_t, tAO::kNumMXPRegisters> mxp_outputs;

        public:
            /**
             * \fn uint16_t getMXPOutput(uint8_t index)const
             * \brief Get MXP output.
             * Returns the MXP output given
             * \param index an byte representing the index of the analog output.
             * \return an unsigned 16-bit integer representing the current analog output.
             */
			uint16_t getMXPOutput(uint8_t)const;
            /**
             * \n void setMXPOutput(uint8_t index, uint16_t value)
             * \brief Set MXP output.
             * Set the MXP value of analog output with a given index to a given value
             * \param index an byte representing the index of the analog output.
             * \param value an unsigned 16-bit integer representing the value of the analog output.
             */
			void setMXPOutput(uint8_t,uint16_t);
        };
        /**
         * \struct AnalogInputs roborio.h
         * \brief Data model for analog inputs.
         * Holds all internal data needed to model analog inputs on the RoboRIO.
         */
		struct AnalogInputs {
            /**
             * \struct AnalogInput roborio.h
             * \brief Data model for individual analog input
             * Holds all internal data for a single analog input.
             */
            struct AnalogInput{
                /**
                 * \var uint8_t oversample_bits.
                 * \brief Number bits to oversample.
                 */
                uint8_t oversample_bits;
                /**
                 * \var uint8_t average_bits.
                 * \brief Number bits to to average.
                 */
                uint8_t average_bits;
                /**
                 * \var uint8_t scan_list.
                 * \brief Currently unknown functionality.
                 */
                uint8_t scan_list;
            };

            /**
             * \fn void setConfig(tConfig value)
             * \brief Sets analog input configuration.
             * Sets current analog input system to \b value.
             * \param value a tConfig object containing new configuration data.
             */

            void setConfig(tAI::tConfig value);

            /**
             * \fn tConfig getConfig()
             * \brief Get current analog input configuration.
             * Gets current analog system configuration settings.
             * \return tConfig representing current analog system configuration.
             */

            tAI::tConfig getConfig();

            /**
             * \fn void setReadSelect(tReadSelect value)
             * \brief Sets analog input read select.
             * Sets current analog input system to \b value. This specifies which analog input to read.
             * \param value a tReadSelect object containing addressing information for the desired analog input.
             */

            void setReadSelect(tAI::tReadSelect);

            /**
             * \fn tConfig getReadSelect()
             * \brief Get current analog input read select.
             * Gets current analog system read select. This specifies which analog input to read.
             * \return tReadSelect representing current analog system read selection.
             */

            tAI::tReadSelect getReadSelect();

            /**
             * \fn void setOversampleBits(uint8_t channel, uint8_t value)
             * \brief Sets analog input configuration.
             * Sets number of oversample bits on analog input channel \b channel to value \b value.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \param value a byte representing the number of bits to oversample per sample.
             */

            void setOversampleBits(uint8_t, uint8_t);

            /**
             * \fn void setOversampleBits(uint8_t channel, uint8_t value)
             * \brief Sets number of bits to sample.
             * Sets number of bits to sample on analog input channel \b channel to value \b value.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \param value a byte representing the number of bits to collect per sample.
             */

            void setAverageBits(uint8_t, uint8_t);

            /**
             * \fn void setOversampleBits(uint8_t channel, uint8_t value)
             * \brief Sets analog input scan lsit.
             * Sets a given analog inputs scan list to \b value.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \param value a byte representing the scan list.
             */
            void setScanList(uint8_t, uint8_t);

            /**
             * \fn uint8_t getOversampleBits(uint8_t channel)
             * \brief Get current analog input configuration.
             * Gets current analog system configuration settings.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \return a byte representing the current bits to oversample.
             */

            uint8_t getOversampleBits(uint8_t);

            /**
             * \fn uint8_t getAverageBits(uint8_t channel)
             * \brief Get current analog input configuration.
             * Gets current analog system configuration settings.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \return a byte representing the number of bits per sample for analog input \b channel.
             */

            uint8_t getAverageBits(uint8_t);

            /**
             * \fn uint8_t getScanList(uint8_t channel)
             * \brief Get current analog input configuration.
             * Gets current analog system configuration settings.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \return a byte representing the current scan list for analog input \b channel.
             */

            uint8_t getScanList(uint8_t);

        private:

            /**
             * \var std::array<AnalogInput, hal::kNumAnalogInputs> analog_inputs
             * \brief Array of all analog inputs.
             * A holder array for all analog input objects.
             */

            std::array<AnalogInput, hal::kNumAnalogInputs> analog_inputs;

            /**
             * \var tConfig config;
             * \brief current analog input configuration.
             */

            tAI::tConfig config;

            /**
             * \var tReadSelect read_select;
             * \brief current analog input read select configuration.
             */

            tAI::tReadSelect read_select;
        };

        /**
         * \struct PWMSystem roborio.h
         * \brief Data model for PWM system.
         * Data model for all PWMS. Holds all internal data for PWMs.
         */

		struct PWMSystem{
		private:

            /**
             * \var tConfig config
             * \brief Current PWM system configuration.
             */

			tPWM::tConfig config;

            /**
             * \struct PWM roborio.h
             * \brief Data model for individual PWM
             * Data model used for storing data about an individual PWM.
             */

			struct PWM{

                /**
                 * \var uint32_t period_scale
                 * \brief 2 bit PWM signal mask.
                 * 2-bit mask for signal masking frequency, effectively scaling the PWM value (0 = 1x 1, = 2x, 3 = 4x)
                 */

				uint32_t period_scale;

                /**
                 * \var uint16_t duty_cycle
                 * \brief PWM Duty cycle
                 * The percentage (0-65535)
                 */

                uint16_t duty_cycle;
			};

            /**
             * \var std::array<PWM, tPWM::kNumHdrRegisters> hdr;
             * \brief Array of all PWM Headers on the base RoboRIO board.
             * Array of all PWM headers on the base board of the RoboRIO (not MXP). Numbered 0-10 on the board.
             */

			std::array<PWM, tPWM::kNumHdrRegisters> hdr;

            /**
             * \var std::array<PWM, tPWM::kNumMXPRegisters> mxp;
             * \brief Array of all PWM Headers on the MXP.
             * Array of all PWM headers on the MXP.
             */

			std::array<PWM, tPWM::kNumMXPRegisters> mxp;

		public:

            /**
             * \fn tConfig getConfig()const
             * \brief Gets current PWM system configuration.
             * Gets current PWM configuration.
             * \return tConfig representing current PWM system configuration.
             */

			tPWM::tConfig getConfig()const;

            /**
             * \fn void setConfig(tConfig config)
             * \brief Sets PWM system configuration.
             * Sets new PWM system configuration.
             * \param tConfig representing new PWM system configuration.
             */

            void setConfig(tPWM::tConfig);

            /**
             * \fn uint32_t getHdrPeriodScale(uint8_t index)
             * \brief get current pwm scale for a header based PWM.
             * Get current PWM scale for a pwm on the base RoboRIO board.
             * \param index the index of the pwm.
             * \return Unsigned 32-bit integer representing the PWM period scale.
             */

			uint32_t getHdrPeriodScale(uint8_t)const;

            /**
             * \fn uint32_t getMXPPeriodScale(uint8_t index)
             * \brief Get current PWM scale for a header based pwm.
             * get current pwm scale for a pwm on the base RoboRIO board.
             * \param index the index of the PWM.
             * \return Unsigned 32-bit integer representing the PWM period scale.
             */


            void setHdrPeriodScale(uint8_t, uint32_t);

            /**
             * \fn uint32_t getMXPPeriodScale(uint8_t index)
             * \brief get current pwm scale for a header based pwm.
             * get current pwm scale for a pwm on the base roborio board.
             * \param index the index of the pwm.
             * \return unsigned 32-bit integer representing the pwm period scale.
             */


			uint32_t getMXPPeriodScale(uint8_t)const;

            /**
             * \fn uint32_t getHdrDutyCycle(uint8_t index)
             * \brief Get current PWM duty cycle.
             * Get current PWM duty cycle for header PWMs.
             * \param index the index of the PWM.
             * \return Unsigned 32-bit integer representing the PWM duty cycle.
             */


			uint32_t getHdrDutyCycle(uint8_t)const;

            /**
             * \fn void setHdrDutyCycle(uint8_t index, uint32_t value)
             * \brief Sets PWM Duty cycle for PWMs on the base board.
             * Sets PWM Duty cycle for PWMs on the base board.
             * \param index the index of the PWM.
             * \param value the new duty cycle to write to the PWM.
             */

            void setHdrDutyCycle(uint8_t, uint32_t);

            /**
             * \fn uint32_t getMXPDutyCycle(uint8_t index)
             * \brief Get current PWM duty cycle.
             * Get current PWM duty cycle for MXP PWMs.
             * \param index the index of the PWM.
             * \return Unsigned 32-bit integer representing the PWM duty cycle.
             */

			uint32_t getMXPDutyCycle(uint8_t)const;

            /**
             * \fn void setMXPDutyCycle(uint8_t index, uint32_t value)
             * \brief Sets PWM Duty cycle for PWMs on the MXP.
             * Sets PWM Duty cycle for PWMs on the MXP.
             * \param index the index of the PWM.
             * \param value the new duty cycle to write to the PWM.
             */
            void setMXPDutyCycle(uint8_t, uint32_t);
		};

        struct DIOSystem{
		private:

            /**
             * \var
             */

            tDIO::tDO outputs;

            /**
             * \var
             */

            tDIO::tOutputEnable enabled_outputs;

            /**
             * \var
             */

            tDIO::tPulse pulses;

            /**
             * \var
             */

            tDIO::tDI inputs;

            /**
             * \var
             */

			uint16_t mxp_special_functions_enabled;//TODO this needs to be checked when attempting things

            /**
             * \var
             */

			uint8_t pulse_length;

			std::array<uint8_t, hal::kNumDigitalPWMOutputs> pwm; //TODO unclear whether these are mxp pins or elsewhere (there are only six here whereas there are ten on the mxp)

		public:

            /**
             * \fn
             */

			tDIO::tDO getOutputs()const;

            /**
             * \fn
             */

			void setOutputs(tDIO::tDO);

            /**
             * \fn
             */

			tDIO::tOutputEnable getEnabledOutputs()const;

            /**
             * \fn
             */

			void setEnabledOutputs(tDIO::tOutputEnable);

            /**
             * \fn
             */

			uint16_t getMXPSpecialFunctionsEnabled()const;

            /**
             * \fn
             */

			void setMXPSpecialFunctionsEnabled(uint16_t);

            /**
             * \fn
             */

			tDIO::tPulse getPulses()const;

            /**
             * \fn
             */

			void setPulses(tDIO::tPulse);

            /**
             * \fn
             */

			tDIO::tDI getInputs()const;

            /**
             * \fn
             */

			void setInputs(tDIO::tDI);

            /**
             * \fn
             */

			uint8_t getPulseLength()const;

            /**
             * \fn
             */

			void setPulseLength(uint8_t);

            /**
             * \fn
             */

			uint8_t getPWMDutyCycle(uint8_t)const;

            /**
             * \fn
             */

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

		struct RelaySystem{
		private:
			tRelay::tValue value;

		public:
			tRelay::tValue getValue()const;
			void setValue(tRelay::tValue);
			
		};

        AnalogInputs analog_inputs;
        AnalogOutputs analog_outputs;
		CANBus can_bus;
        DIOSystem digital_system;
        PWMSystem pwm_system;
		RelaySystem relay_system;

        explicit RoboRIO() = default;

        friend class RoboRIOManager;
    private:
        RoboRIO(RoboRIO const&) = default;
    };

    /**
     * 
     */

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

    class Serializable {
        virtual std::string serialize() = 0;
    };
}

#endif
