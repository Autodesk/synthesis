#ifndef _POWER_HPP_
#define _POWER_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tPower.h"

namespace hel{

    /**
     * \brief Data model for RoboRIO voltmeter and power manager
     */

    struct Power{
    private:

        /**
         * \brief The active state of the power supply rails
         */

        nFPGA::nRoboRIO_FPGANamespace::tPower::tStatus status;

        /**
         * \brief A running count of faults for each rail
         */

        nFPGA::nRoboRIO_FPGANamespace::tPower::tFaultCounts fault_counts;

        /**
         * \brief Which power rails have been disabled
         */

        nFPGA::nRoboRIO_FPGANamespace::tPower::tDisable disabled;

    public:
        nFPGA::nRoboRIO_FPGANamespace::tPower::tStatus getStatus()const noexcept;
        void setStatus(nFPGA::nRoboRIO_FPGANamespace::tPower::tStatus)noexcept;
        nFPGA::nRoboRIO_FPGANamespace::tPower::tFaultCounts getFaultCounts()const noexcept;
        void setFaultCounts(nFPGA::nRoboRIO_FPGANamespace::tPower::tFaultCounts)noexcept;
        nFPGA::nRoboRIO_FPGANamespace::tPower::tDisable getDisabled()const noexcept;
        void setDisabled(nFPGA::nRoboRIO_FPGANamespace::tPower::tDisable)noexcept;

        /**
         * Constructor for Power
         */

        Power()noexcept;

        /**
         * Constructor for Power
         * \param source A Power object to copy
         */

        Power(const Power&)noexcept;
    };
}

#endif
