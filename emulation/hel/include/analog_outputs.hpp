#ifndef _ANALOG_OUTPUTS_HPP_
#define _ANALOG_OUTPUTS_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAO.h"
#include "bounds_checked_array.hpp"

#include "bounds_checked_array.hpp"

namespace hel{

    /**
     * \struct AnalogOutputs
     * \brief Data model for analog outputs.
     * Holds all internal data needed to model analog outputs on the RoboRIO.
     */
    struct AnalogOutputs{
		static constexpr int32_t NUM_ANALOG_OUTPUTS = 2; //nFPGA::nRoboRIO_FPGANamespace::tAO::kNumMXPRegisters>

    private:
        /**
         * \var BoundsCheckedArray<uint16_t, NUM_ANALOG_OUTPUTS> mxp_outputs
         * \brief Analog output data
         *
         */
        BoundsCheckedArray<uint16_t, NUM_ANALOG_OUTPUTS> mxp_outputs;

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

        AnalogOutputs();
    };
}

#endif
