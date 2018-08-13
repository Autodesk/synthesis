#ifndef _ACCUMULATOR_HPP_
#define _ACCUMULATOR_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAccumulator.h"

namespace hel{

    /**
     * \brief Data model for an analog accumulator
     * Accumulates analog values in a total over time while tracking count
     */

    struct Accumulator{
    private:

        /**
         * \brief Stores the accumulated value of the accumulator
         */

        nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput output;

        /**
         * \brief The center value for the accumulator
         * This is used to handle device offsets
         */

        int32_t center;

        /**
         * \brief The deadband of the accumulator
         */

        int32_t deadband;

    public:
        /**
         * \brief Get the accumulated values of the accumulator
         * \return The accumulated values of the accumulator
         */

        nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput getOutput()const noexcept;

        /**
         * \fn void setOutput(nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput out)noexcept
         * \brief Set the accumulated values of the accumulator
         * \param out The accumulated values to set for the accumulator
         */

        void setOutput(nFPGA::nRoboRIO_FPGANamespace::tAccumulator::tOutput)noexcept;

        /**
         * \brief Get the active center for the accumulator
         * \return The active center of the accumulator
         */

        int32_t getCenter()const noexcept;

        /**
         * \brief Set the center for the accumulator
         * \param c The center to set for accumulator
         */

        void setCenter(int32_t)noexcept;

        /**
         * \brief Get the active deadband for the accumulator
         * \return The active deadband for the accumulator
         */

        int32_t getDeadband()const noexcept;

        /**
         * \brief Set the deadband for the accumulator
         * \param d The deadband to set for the accumulator
         */

        void setDeadband(int32_t)noexcept;

        /**
         * Constructor for an Accumulator
         */

        Accumulator()noexcept;

        /**
         * Constructor for an Accumulator
         *
         * \param source An Accumulator object to copy
         */

        Accumulator(const Accumulator&)noexcept;
    };
}

#endif
