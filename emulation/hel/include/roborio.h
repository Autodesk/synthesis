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

        AnalogOutputs analog_outputs;
        PWMSystem pwm_system;
        DIOSystem digital_system;
        AnalogInputs analog_inputs;

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
