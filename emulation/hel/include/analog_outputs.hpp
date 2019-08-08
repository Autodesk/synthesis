#ifndef _ANALOG_OUTPUTS_HPP_
#define _ANALOG_OUTPUTS_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAO.h"

#include "bounds_checked_array.hpp"

namespace hel{

    /**
     * \brief Data model for analog outputs.
     * Holds all internal data needed to model analog outputs on the RoboRIO.
     */
    struct AnalogOutputs{

        /**
         * \brief The number of analog outputs on the RoboRIO (positioned on the MXP)
         */

        static constexpr int32_t NUM_ANALOG_OUTPUTS = nFPGA::nRoboRIO_FPGANamespace::tAO::kNumMXPRegisters; // 2

    private:
        /**
         * \brief Analog output data
         */

        BoundsCheckedArray<uint16_t, NUM_ANALOG_OUTPUTS> mxp_outputs;

    public:
        /**
         * \brief Get the MXP output at the given port
         * \param index A byte representing the index of the analog output.
         * \return An unsigned, 16-bit integer representing the current analog output.
         */

        uint16_t getMXPOutput(uint8_t)const;

        /**
         * \brief Set the MXP value of analog output with a given index to a given value
         * \param index A byte representing the index of the analog output.
         * \param value An unsigned 16-bit integer representing the value of the analog output.
         */
        void setMXPOutput(uint8_t,uint16_t);

        /**
         * Constructor for AnalogOutputs
         */

        AnalogOutputs()noexcept;

        /**
         * Constructor for AnalogOutputs
         *
         * \param source An AnalogOutputs object to copy
         */

        AnalogOutputs(const AnalogOutputs&)noexcept;
    };
}

#endif
