#ifndef _ANALOG_INPUTS_HPP_
#define _ANALOG_INPUTS_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAI.h"

#include <vector>
#include <array>

namespace hel{
    /**
     * \struct AnalogInputs
     * \brief Data model for analog inputs.
     * Holds all internal data needed to model analog inputs on the RoboRIO.
     */
    struct AnalogInputs {
        static constexpr const int32_t NUM_ANALOG_INPUTS = 8; //hal::kNumAnalogInputs

        /**
         * \struct AnalogInput
         * \brief Data model for individual analog input
         * Holds all internal data for a single analog input.
         */
        struct AnalogInput{
            /**
             * \var uint8_t oversample_bits.
             * \brief When storing analog value history, keep 2**(oversample_bits + average_bits) samples.
             */
            uint8_t oversample_bits;
            /**
             * \var uint8_t average_bits.
             * \brief When averaging, average 2**average_bits samples.
             */
            uint8_t average_bits;
            /**
             * \var uint8_t scan_list.
             * \brief Currently unknown functionality.
             */
            uint8_t scan_list;

            /**
             * \var int32_t value
             * \brief The history of analog input values
             * The most recent value is the last element.
             */

            std::vector<int32_t> values;
        };

        /**
         * \fn void setConfig(tConfig value)
         * \brief Sets analog input configuration.
         * Sets current analog input system to \b value.
         * \param value a tConfig object containing new configuration data.
         */

        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tAI::tConfig value);

        /**
         * \fn tConfig getConfig()
         * \brief Get current analog input configuration.
         * Gets current analog system configuration settings.
         * \return tConfig representing current analog system configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tAI::tConfig getConfig();

        /**
         * \fn void setReadSelect(tReadSelect value)
         * \brief Sets analog input read select.
         * Sets current analog input system to \b value. This specifies which analog input to read.
         * \param value a tReadSelect object containing addressing information for the desired analog input.
         */

        void setReadSelect(nFPGA::nRoboRIO_FPGANamespace::tAI::tReadSelect);

        /**
         * \fn tConfig getReadSelect()
         * \brief Get current analog input read select.
         * Gets current analog system read select. This specifies which analog input to read.
         * \return tReadSelect representing current analog system read selection.
         */

        nFPGA::nRoboRIO_FPGANamespace::tAI::tReadSelect getReadSelect();

        /**
         * \fn void setOversampleBits(uint8_t channel, uint8_t value)
         * \brief Sets number of samples to keep beyond those needed for averaging.
         * \param channel a byte representing the hardware channel of the desired analog input.
         * \param value a byte representing the number of samples to collect after that need for the average.
         */

        void setOversampleBits(uint8_t, uint8_t);

        /**
         * \fn void setOversampleBits(uint8_t channel, uint8_t value)
         * \brief Sets number of sample to average to 2**value.
         * \param channel a byte representing the hardware channel of the desired analog input.
         * \param value a byte representing the number of samples to use in averaging.
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
         * \fn void setValue(uint8_t channel, std::vector<int32_t> values)
         * \brief Sets the history of analog input values.
         * \param channel a byte representing the hardware channel of the desired analog input.
         * \param value a vector history of 32-bit integers representing the value of the input.
         */

        void setValues(uint8_t, std::vector<int32_t>);

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

        /**
         * \fn uint8_t getValue(uint8_t channel)
         * \brief Get the recent history of analog input values.
         * \param channel a byte representing the hardware channel of the desired analog input.
         * \return a vector of 32-bit integer representing the recent history of the analog input value for analog input \b channel.
         */

        std::vector<int32_t> getValues(uint8_t);

        AnalogInputs();

    private:

        /**
         * \var std::array<AnalogInput, NUM_ANALOG_INPUTS> analog_inputs
         * \brief Array of all analog inputs.
         * A holder array for all analog input objects.
         */

        std::array<AnalogInput, NUM_ANALOG_INPUTS> analog_inputs;

        /**
         * \var tConfig config;
         * \brief current analog input configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tAI::tConfig config;

        /**
         * \var tReadSelect read_select;
         * \brief current analog input read select configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tAI::tReadSelect read_select;
    };
}

#endif
