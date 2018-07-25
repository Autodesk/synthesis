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
        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput getOutput()const
         * \brief Get the accumulated values of the accumulator
         * \return the accumulated values of the accumulator
         */

        nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput getOutput()const;

        /**
         * \fn void setOutput(nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput output)
         * \brief Set the accumulated values of the accumulator
         * \param output the accumulated values to set for the accumulator
         */

        void setOutput(nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput);

        /**
         * \fn int32_t getCenter()const
         * \brief Get the active center for the accumulator
         * \return the active center of the accumulator
         */

        int32_t getCenter()const;

        /**
         * \fn void setCenter(int32_t center)
         * \brief Set the center for the accumulator
         * \param center the center to set for accumulator
         */

        void setCenter(int32_t);

        /**
         * \fn int32_t getDeadband()const
         * \brief Get the active deadband for the accumulator
         * \return the active deadband for the accumulator
         */

        int32_t getDeadband()const;

        /**
         * \fn void setDeadband(int32_t deadband)
         * \brief Set the deadband for the accumulator
         * \param deadband the deadband to set for the accumulator
         */

        void setDeadband(int32_t);
        Accumulator();
    };
}

#endif
