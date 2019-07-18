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
         * \brief The number of digital pins on the RoboRIO itself
         */

        static constexpr int32_t NUM_DIGITAL_HEADERS = 10; //hal::kNumDigitalHeaders

        /**
         * \brief The number of digital pins of the RoboRIO MXP
         */
        static constexpr int32_t NUM_DIGITAL_MXP_CHANNELS = 16; //hal::kNumDigitalMXPChannels

        /**
         * \brief It is unclear what the Ni FPGA refers to when it refers to this
         */

        static constexpr int32_t NUM_DIGITAL_PWM_OUTPUTS = 6; //hal::kNumDigitalPWMOutputs

        /**
         * \brief The maximum length of a digital pulse in microseconds
         *
         * Since the Ni FPGA manages pulse length as a byte, it is impossible to try to set pulses even this long. So, this value isn't actually used.
         */

        static constexpr uint16_t MAX_PULSE_LENGTH = 1600;

    private:

        /**
         * \brief The set digital outputs
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO outputs;

        /**
         * \brief Bit mask representing if the digital port is configured for output
         * If the digital port is enabled for its special function, output enable is not checked
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable enabled_outputs;

        /**
         * \brief The active digital pulses
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse pulses;

        /**
         * \brief The digital inputs
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI inputs;

        /**
         * \brief Bit mask for MXP pins representing if their non-DIO option should be active
         *
         * MXP special functions are enabled high
         */

        uint16_t mxp_special_functions_enabled;

        /**
         * \brief The time to pulse for in microseconds
         */

        uint8_t pulse_length;

        /**
         * \brief It is unclear whether these are MXP pins or elsewhere
         *
         * There are only six here whereas there are ten on the MXP (TODO: figure this out)
         */

        BoundsCheckedArray<uint8_t, NUM_DIGITAL_PWM_OUTPUTS> pwm;

        BoundsCheckedArray<MXPData::Config, NUM_DIGITAL_MXP_CHANNELS> mxp_configurations;

    public:
        /**
         * \brief An exception for mismatch of digital function and port configuration
         */

        struct DIOConfigurationException: public std::exception{
            /**
             * \enum Config
             * \brief Represents the different possible digital port configurations
             */

            enum class Config{DI,DO,MXP_SPECIAL_FUNCTION};
        private:
            /**
             * \brief The actual set configuration of the digital port
             */

            Config actual_configuration;

            /**
             * \brief The expected configuration for the digital port to use the digital functionality
             */

            Config expected_configuration;

            /**
             * \brief The digital port with the configuration error
             */

            uint8_t port;

        public:
            /**
             * \brief Returns the exception message
             */

            const char* what()const throw();

            /**
             * Constructor for a DIOConfigurationException
             * \param expected The expected configuration necessary for the digital IO action
             * \param config The actual set configuration
             * \param index The digital port with the misconfiguration
             */

            DIOConfigurationException(Config, Config, uint8_t)noexcept;
        };

        /**
         * \brief Get the active digital outputs
         * \return The active digital outputs
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO getOutputs()const noexcept;

        /**
         * \fn void setOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO out)noexcept
         * \brief Set the active digital outputs
         * \param out The outputs to set to
         */

        void setOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO)noexcept;

        /**
         * \brief Get the digital ports with output enabled
         * \return A bit mask representing digital ports in output mode
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable getEnabledOutputs()const noexcept;

        /**
         * \fn void setEnabledOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable enabled_out)noexcept
         * \brief Set the digital ports to output mode
         * \param enabled_out A bit mask of digital ports to set to output
         */

        void setEnabledOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable)noexcept;

        /**
         * \brief Fetch the bit mask for enabled MXP special functions
         * \return The bit mask for enabled MXP special functions
         */

        uint16_t getMXPSpecialFunctionsEnabled()const noexcept;

        /**
         * \brief Set the bit mask to enable MXP special functions
         * \param enabled_mxp_special_functions The bit mask to use
         */

        void setMXPSpecialFunctionsEnabled(uint16_t)noexcept;

        /**
         * \brief Get the active digital pulses
         * \return A bit mask representing the active digital pulses
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse getPulses()const noexcept;

        /**
         * \fn void setPulses(nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse value)noexcept
         * \brief Set the active pulses
         * \param value The active pulses
         */

        void setPulses(nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse)noexcept;

        /**
         * \brief Fetch the digital inputs
         * \return A bit mask representing the digital input states
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI getInputs()const noexcept;

        /**
         * \fn void setInputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI inputs)noexcept
         * \brief Set the digital input states
         * \param in A bit mask representing the digital input states to set to
         */

        void setInputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI)noexcept;

		MXPData::Config getMXPConfig(const unsigned)const;

		void setMXPConfig(const unsigned);

        /**
         * \brief Get the set pulse length
         * \return The length to pulse for
         */

        uint8_t getPulseLength()const noexcept;

        /**
         * \brief Set the pulse length in microseconds
         * \param length The length to pulse for
         */

        void setPulseLength(uint8_t)noexcept;

        /**
         * \brief Gets a given digital PWM generator's pulse width
         * Unclear what these PWM generators are, so it is unhandled
         * \param index The PWM generator to get the pulse width from
         */

        uint8_t getPWMPulseWidth(uint8_t)const;

        /**
         * \brief Sets a digital PWM generator pulse width
         * Unclear what these PWM generators are, so it is unhandled
         * \param index The PWM generator to set
         * \param pulse_width The pulse width to output
         */

        void setPWMPulseWidth(uint8_t, uint8_t);

        /**
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
     *\fn std::string asString(DigitalSystem::DIOConfigurationException::Config config)
     * \brief Format the DIOConfigurationException configuration as a string
     * \param config The configuration to convert
     * \return The configuration in string form
     */

    std::string asString(DigitalSystem::DIOConfigurationException::Config);
}

#endif
