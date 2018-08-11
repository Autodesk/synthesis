#ifndef _DIGITAL_SYSTEM_HPP_
#define _DIGITAL_SYSTEM_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tDIO.h"

#include <string>
#include <exception>

#include "bounds_checked_array.hpp"
#include "mxp_data.hpp"

namespace hel{

    /**
     * \brief Models the digital IO system of the RoboRIO
     */

    struct DigitalSystem{
        /**
         * \var static constexpr int32_t NUM_DIGITAL_HEADERS
         * \brief The number of digital pins on the RoboRIO itself
         */

        static constexpr int32_t NUM_DIGITAL_HEADERS = 10; //hal::kNumDigitalHeaders

        /**
         * \var static constexpr int32_t NUM_DIGITAL_MXP_CHANNELS
         * \brief The number of digital pins of the RoboRIO MXP
         */
        static constexpr int32_t NUM_DIGITAL_MXP_CHANNELS = 16; //hal::kNumDigitalMXPChannels

        /**
         * \var static constexpr int32_t NUM_DIGITAL_PWM_OUTPUTS
         * \brief It is unclear what the Ni FPGA refers to when it refers to this
         */

        static constexpr int32_t NUM_DIGITAL_PWM_OUTPUTS = 6; //hal::kNumDigitalPWMOutputs

        /**
         * \var static constexpr uint16_t MAX_PULSE_LENGTH
         * \brief The maximum length of a digital pulse in microseconds
         *
         * Since the Ni FPGA manages pulse length as a byte, it is impossible to try to set pulses longer than this
         */

        static constexpr uint16_t MAX_PULSE_LENGTH = 1600;

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
         *
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
         * \brief It is unclear whether these are MXP pins or elsewhere
         *
         * There are only six here whereas there are ten on the MXP (TODO: figure this out)
         */

        BoundsCheckedArray<uint8_t, NUM_DIGITAL_PWM_OUTPUTS> pwm;

    public:
        /**
         * \brief An exception for mismatch of digital function and port configuration
         */

        struct DIOConfigurationException: public std::exception{
            /**
             * \enum Config
             * \brief Represents the different possible digital port configurations
             */

            enum class Config{DI,DO,MXP_SPECIAL_FUNCTION}; //TODO: use MXPData::Config instead?
        private:
            /**
             * \var Config configuration
             * \brief The set configuration of the digital port
             */

            Config configuration;

            /**
             * \var Config expected_configuration
             * \brief The expected configuration for the digital port to use the digital functionality
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
             * \brief Returns the exception message
             */

            const char* what()const throw();

            /**
             * Constructor for a DIOConfigurationException
             * \param configuration The set configuration
             * \param expected_configuration The expected configuration necessary for the digital IO action
             * \param index The digital port with the misconfiguration
             */

            DIOConfigurationException(Config, Config, uint8_t)noexcept;
        };

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO getOutputs()const noexcept
         * \brief Get the active digital outputs
         * \return The active digital outputs
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO getOutputs()const noexcept;

        /**
         * \fn void setOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO outputs)noexcept
         * \brief Set the active digital outputs
         * \param outputs The outputs to set to
         */

        void setOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable getEnabledOutputs()const noexcept
         * \brief Get the digital ports with output enabled
         * \return A bit mask representing digital ports in output mode
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable getEnabledOutputs()const noexcept;

        /**
         * \fn void setEnabledOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable enabled_outputs)noexcept
         * \brief Set the digital ports to output mode
         * \param enabled_outputs A bit mask of digital ports to set to output
         */

        void setEnabledOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable)noexcept;

        /**
         * \fn uint16_t getMXPSpecialFunctionsEnabled()const noexcept
         * \brief Fetch the bit mask for enabled MXP special functions
         * \return The bit mask for enabled MXP special functions
         */

        uint16_t getMXPSpecialFunctionsEnabled()const noexcept;

        /**
         * \fn void setMXPSpecialFunctionsEnabled(uint16_t mxp_special_functions_enabled)noexcept
         * \brief Set the bit mask to enable MXP special functions
         * \param mxp_special_functions_enabled The bit mask to use
         */

        void setMXPSpecialFunctionsEnabled(uint16_t)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse getPulses()const noexcept
         * \brief Get the active digital pulses
         * \return A bit mask representing the active digital pulses
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse getPulses()const noexcept;

        /**
         * \fn void setPulses(nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse pulses)noexcept
         * \brief Set the active pulses
         * \param pulses The active pulses
         */

        void setPulses(nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI getInputs()const noexcept
         * \brief Fetch the digital inputs
         * \return A bit mask representing the digital input states
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI getInputs()const noexcept;

        /**
         * \fn void setInputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI inputs)noexcept
         * \brief Set the digital input states
         * \param inputs A bit mask representing the digital input states to set to
         */

        void setInputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI)noexcept;

        /**
         * \fn uint8_t getPulseLength()const noexcept
         * \brief Get the set pulse length
         * \return The length to pulse for
         */

        uint8_t getPulseLength()const noexcept;

        /**
         * \fn void setPulseLength(uint8_t pulse_length)noexcept
         * \brief Set the pulse length in microseconds
         * \param pulse_length The length to pulse for
         */

        void setPulseLength(uint8_t)noexcept;

        /**
         * \fn uint8_t getPWMPulseWidth(uint8_t index)const
         * \brief Gets a given digital PWM generator's pulse width
         * Unclear what these PWM generators are, so it is unhandled
         * \param index The PWM generator to get the pulse width from
         */

        uint8_t getPWMPulseWidth(uint8_t)const;

        /**
         * \fn void setPWMPulseWidth(uint8_t index, uint8_t pulse_width)
         * \brief Sets a digital PWM generator pulse width
         * Unclear what these PWM generators are, so it is unhandled
         * \param index The PWM generator to set
         * \param pulse_width The pulse width to output
         */

        void setPWMPulseWidth(uint8_t, uint8_t);

        /**
         * \fn static MXPData::Config toMXPConfig(uint16_t output_mode, uint16_t special_func,uint8_t index)
         * \brief Interprets the configuration of an MXP digital port
         * \param output_mode The bitmask representing the MXP ports configured for output
         * \param special_func The bitmask representing MXP ports configured for special IO
         * \param index The digital MXP index (0-15)
         * \return The digital MXP configuration
         */

        static MXPData::Config toMXPConfig(uint16_t, uint16_t,uint8_t);

        /**
         * Constructor for DigitalSystem
         */

        DigitalSystem()noexcept;

        /**
         * Constructor for DigitalSystem
         *
         * \param source A DigitalSystem object to copy
         */

        DigitalSystem(const DigitalSystem&)noexcept;
    };

    /**
     * \fn std::string as_string(DigitalSystem::DIOConfigurationException::Config config)
     * \brief Format the DIOConfigurationException configuration as a string
     * \param config The configuration to convert
     * \return The configuration in string form
     */

    std::string as_string(DigitalSystem::DIOConfigurationException::Config);
}

#endif
