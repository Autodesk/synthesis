#ifndef _ANALOG_INPUTS_HPP_
#define _ANALOG_INPUTS_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAI.h"

#include <vector>
#include "bounds_checked_array.hpp"

namespace hel{

    /**
     * \brief Data model for analog inputs.
     * Holds all internal data needed to model analog inputs on the RoboRIO.
     */

    struct AnalogInputs {

        /**
         * \brief The number of analog input pins directly on the RoboRIO
         */

        static constexpr int32_t NUM_ANALOG_INPUTS_HDRS = 4;

        /**
         * \brief The number of analog input pins on the MXP
         */

        static constexpr int32_t NUM_ANALOG_INPUTS_MXP = 4;

        /**
         * \brief The total number of analog inputs on the RoboRIO
         */

        static constexpr int32_t NUM_ANALOG_INPUTS = NUM_ANALOG_INPUTS_HDRS + NUM_ANALOG_INPUTS_MXP; //hal::kNumAnalogInputs

        /**
         * \brief The value in nanovolts of the increment between analog data values
         */

        static constexpr uint32_t LSB_WEIGHT = 1.221E6;

		/**
         * \brief A scalar HAL uses when interpreting analog input values
         *
         * This value converts LSB_WEIGHT to volts
         */

		static constexpr double LSB_SCALAR = 1.0E-9;

        /**
         * \brief The offset of the result of the ADC and the actual voltage
         *
         * If 0 is used as the offset, then the analog value and voltage can be used interchangeably
         */

        static constexpr int32_t OFFSET = 0;

        /**
         * \brief Data model for individual analog input
         * Holds all internal data for a single analog input.
         */

        struct AnalogInput{
            /**
             * \brief When storing analog value history, keep 2**(oversample_bits + average_bits) samples.
             */

            uint8_t oversample_bits;

            /**
             * \brief When averaging, average 2**average_bits samples.
             */

            uint8_t average_bits;

            /**
             * \brief Currently unknown functionality.
             */
            uint8_t scan_list;

            /**
             * \brief The history of analog input values
             */

            std::vector<int32_t> values;

            /**
             * Constructor for AnalogInput
             */

            AnalogInput()noexcept;

            /**
             * Constructor for AnalogInput
             * \param source An AnalogInput object to copy
             */

            AnalogInput(const AnalogInput&)noexcept;
        };

        /**
         * \fn void setConfig(nFPGA::nRoboRIO_FPGANamespace::tAI::tConfig value)noexcept
         * \brief Sets analog input configuration.
         * Sets current analog input system to \b value.
         * \param value A tConfig object containing new configuration data.
         */

        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tAI::tConfig)noexcept;

        /**
         * \brief Get current analog input configuration.
         * Gets current analog system configuration settings.
         * \return A tConfig object representing current analog system configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tAI::tConfig getConfig()noexcept;

        /**
         * \fn void setReadSelect(nFPGA::nRoboRIO_FPGANamespace::tAI::tReadSelect value)noexcept
         * \brief Sets analog input read select.
         * Sets current analog input system to \b value. This specifies which analog input to read.
         * \param value A tReadSelect object containing addressing information for the desired analog input.
         */

        void setReadSelect(nFPGA::nRoboRIO_FPGANamespace::tAI::tReadSelect)noexcept;

        /**
         * \brief Get current analog input read select.
         * Gets current analog system read select. This specifies which analog input to read.
         * \return A tReadSelect object representing current analog system read selection.
         */

        nFPGA::nRoboRIO_FPGANamespace::tAI::tReadSelect getReadSelect()noexcept;

        /**
         * \brief Sets number of samples to keep beyond those needed for averaging.
         * \param channel A byte representing the hardware channel of the desired analog input.
         * \param value A byte representing the number of samples to collect after that need for the average.
         */

        void setOversampleBits(uint8_t, uint8_t);

        /**
         * \brief Sets number of sample to average to 2**value.
         * \param channel A byte representing the hardware channel of the desired analog input.
         * \param value A byte representing the number of samples to use in averaging.
         */

        void setAverageBits(uint8_t, uint8_t);

        /**
         * \brief Sets analog input scan list.
         * Sets a given analog inputs scan list to \b value.
         * \param channel A byte representing the hardware channel of the desired analog input.
         * \param value A byte representing the scan list.
         */
        void setScanList(uint8_t, uint8_t);

        /**
         * \brief Sets the history of analog input values.
         * \param channel A byte representing the hardware channel of the desired analog input.
         * \param value A vector history of 32-bit integers representing the value of the input.
         */

        void setValues(uint8_t, std::vector<int32_t>);

        /**
         * \brief Get current analog input configuration.
         * Gets current analog system configuration settings.
         * \param channel A byte representing the hardware channel of the desired analog input.
         * \return A byte representing the current bits to oversample.
         */

        uint8_t getOversampleBits(uint8_t);

        /**
         * \brief Get current analog input configuration.
         * Gets current analog system configuration settings.
         * \param channel A byte representing the hardware channel of the desired analog input.
         * \return A byte representing the number of bits per sample for analog input \b channel.
         */

        uint8_t getAverageBits(uint8_t);

        /**
         * \brief Get current analog input configuration.
         * Gets current analog system configuration settings.
         * \param channel A byte representing the hardware channel of the desired analog input.
         * \return A byte representing the current scan list for analog input \b channel.
         */

        uint8_t getScanList(uint8_t);

        /**
         * \brief Get the recent history of analog input values.
         * \param channel A byte representing the hardware channel of the desired analog input.
         * \return A vector of 32-bit integer representing the recent history of the analog input value for analog input \b channel.
         */

        std::vector<int32_t> getValues(uint8_t);

        /**
         * Constructor for AnalogInputs
         */

        AnalogInputs()noexcept;

        /**
         * Constructor for AnalogInputs
         *
         * \param source An AnalogInputs object to copy
         */

        AnalogInputs(const AnalogInputs&)noexcept;

    private:

        /**
         * \brief Array of all analog inputs.
         */

        BoundsCheckedArray<AnalogInput, NUM_ANALOG_INPUTS> analog_inputs;

        /**
         * \brief Current analog input configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tAI::tConfig config;

        /**
         * \brief Current analog input read select configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tAI::tReadSelect read_select;
    };
}

#endif
