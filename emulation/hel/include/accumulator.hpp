#ifndef _ACCUMULATOR_HPP_
#define _ACCUMULATOR_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAccumulator.h"

namespace hel{

    /**
     * \struct Accumulator
     * \brief Data model for an analog accumulator
     * Accumulates analog values in a total over time while tracking count
     */

    struct Accumulator{
    private:

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput output
         * \brief Stores the accumulated value of the accumulator
         */

        nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput output;

        /**
         * \var int32_t center
         * \brief The center value for the accumulator
         * This is used to handle device offsets
         */

        int32_t center;

        /**
         * \var int32_t deadband
         * \brief The deadband of the accumulator
         */

        int32_t deadband;

    public:
        nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput getOutput()const;
        void setOutput(nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput);
        int32_t getCenter()const;
        void setCenter(int32_t);
        int32_t getDeadband()const;
        void setDeadband(int32_t);
        Accumulator();
    };
}

#endif
