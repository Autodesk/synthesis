#ifndef _DIGITAL_SYSTEM_HPP_
#define _DIGITAL_SYSTEM_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tDIO.h"

#include <string>
#include <exception>

#include "bounds_checked_array.hpp"
#include "mxp_data.hpp"

namespace hel{
    struct DigitalSystem{
		static constexpr int32_t NUM_DIGITAL_HEADERS = 10; //hal::kNumDigitalHeaders
		static constexpr int32_t NUM_DIGITAL_MXP_CHANNELS = 16; //hal::kNumDigitalMXPChannels
		static constexpr int32_t NUM_DIGITAL_PWM_OUTPUTS = 6; //hal::kNumDigitalPWMOutputs
    static constexpr uint16_t MAX_PULSE_LENGTH = 1600; //microseconds; since Ni FPGA manages this as a byte, the pulse length will always be shorter than this

    private:

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO outputs
         * \brief The set digital outputs
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO outputs;

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable enabled_outputs
         * \brief Bit mask representing if the digital port is configured for output
         * If the digital port is enabled for its special function, output enable is not checked
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable enabled_outputs;

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse pulses
         * \brief The active digital pulses
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse pulses;

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI inputs
         * \brief The digital inputs
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI inputs;

        /**
         * \var uint16_t mxp_special_functions_enabled
         * \brief Bit mask for MXP pins representing if their non-DIO option should be active
         * MXP special functions are enabled high
         */

        uint16_t mxp_special_functions_enabled;

        /**
         * \var uint8_t pulse_length
         * \brief The time to pulse for in microseconds
         */

        uint8_t pulse_length;

        /**
         * \var BoundsCheckedArray<uint8_t, NUM_DIGITAL_PWM_OUTPUTS> pwm
         */

        BoundsCheckedArray<uint8_t, NUM_DIGITAL_PWM_OUTPUTS> pwm; //TODO unclear whether these are mxp pins or elsewhere (there are only six here whereas there are ten on the mxp)

    public:
        /**
         * \struct DIOConfigurationException: public std::exception
         * \brief An exception for mismatch of digital function and port configuration
         */

        struct DIOConfigurationException: public std::exception{
            /**
             * \enum class Config
             * \brief Represents the different possible digital port configurations
             */

            enum class Config{DI,DO,MXP_SPECIAL_FUNCTION};
        private:
            /**
             * \var Config configuration
             * \brief The set configuration of the digital port
             */

            Config configuration;

            /**
             * \var Config expected_configuration
             * \brief The expected configuration for the digital port to use the digital functionity
             */

            Config expected_configuration;

            /**
             * \var uint8_t port
             * \brief The digital port with the configuration error
             */

            uint8_t port;

        public:
            /**
             * \fn const char* what()const throw
             * \brief Returns the exception messaage
             */

            const char* what()const throw();

            DIOConfigurationException(Config, Config, uint8_t)noexcept;
        };

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO getOutputs()const
         * \brief Get the active digital outputs
         * \return The active digital outputs
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO getOutputs()const noexcept;

        /**
         * \fn void setOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO outputs)
         * \brief Set the active digital outputs
         * \param outputs the outputs to set to
         */

        void setOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable getEnabledOutputs()const
         * \brief Get the digital ports with output enabled
         * \return a bit mask representing digital ports in output mode
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable getEnabledOutputs()const noexcept;

        /**
         * \fn void setEnabledOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable)noexcept;
         * \brief Set the digital ports to output mode
         * \param enabled_outputs a bit mask of digital ports to set to output
         */

        void setEnabledOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable)noexcept;

        /**
         * \fn uint16_t getMXPSpecialFunctionsEnabled()const
         * \brief Fetch the bit mask for enabled mxp special functions
         * \return the bit mask for enabled mxp special functions
         */

        uint16_t getMXPSpecialFunctionsEnabled()const noexcept;

        /**
         * \fn void setMXPSpecialFunctionsEnabled(uint16_t mxp_special_functions_enabled)
         * \brief Set the bit mask to enable mxp special functions
         * \param mxp_special_functions_enabled the bit mask to use
         */

        void setMXPSpecialFunctionsEnabled(uint16_t)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse getPulses()const
         * \brief Get the active digital pulses
         * \return a bit mask representing the active digital pulses
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse getPulses()const noexcept;

        /**
         * \fn void setPulses(nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse pulses)
         * \brief Set the active pulses
         * \param pulses the active pulses
         */

        void setPulses(nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI getInputs()const
         * \brief Fetch the digital inputs
         * \return a bit mask representing the digital input states
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI getInputs()const noexcept;

        /**
         * \fn void setInputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI inputs)
         * \brief Set the digital input states
         * \param inputs a bit mask representing the digital input states to set to
         */

        void setInputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI)noexcept;

        /**
         * \fn uint8_t getPulseLength()const
         * \brief Get the set pulse length
         * \return the length to pulse for
         */

        uint8_t getPulseLength()const noexcept;

        /**
         * \fn void setPulseLength(uint8_t pulse_length)
         * \brief Set the pulse length in microseconds
         * \param pulse_length the length to pulse for
         */

        void setPulseLength(uint8_t)noexcept;

        /**
         * \fn uint8_t getPWMPulseWidth(uint8_t index)const
         */

        uint8_t getPWMPulseWidth(uint8_t)const;

        /**
         * \fn void setPWMPulseWidth(uint8_t index, uint8_t pulse_width)
         */

        void setPWMPulseWidth(uint8_t, uint8_t);

		static MXPData::Config toMXPConfig(uint16_t, uint16_t,uint8_t);

        DigitalSystem()noexcept;
        DigitalSystem(const DigitalSystem&)noexcept;
    };

    std::string to_string(DigitalSystem::DIOConfigurationException::Config);
}

#endif
