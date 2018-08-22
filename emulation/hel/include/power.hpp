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
        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tPower::tStatus getStatus()const noexcept
         * \brief Get the status of the power rails
         * \return The status of the power rails
         */

        nFPGA::nRoboRIO_FPGANamespace::tPower::tStatus getStatus()const noexcept;

        /**
         * \fn void setStatus(nFPGA::nRoboRIO_FPGANamespace::tPower::tStatus s)noexcept
         * \brief Set the status of the power rails
         * \param s The status to use
         */

        void setStatus(nFPGA::nRoboRIO_FPGANamespace::tPower::tStatus)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tPower::tFaultCounts getFaultCounts()const noexcept
         * \brief Get the fault counts for the power rails
         * \return The fault counts for the power rails
         */

        nFPGA::nRoboRIO_FPGANamespace::tPower::tFaultCounts getFaultCounts()const noexcept;

        /**
         * \fn void setFaultCounts(nFPGA::nRoboRIO_FPGANamespace::tPower::tFaultCounts counts)noexcept
         * \brief Set the fault counts of the power rails
         * \param counts The fault counts to use
         */

        void setFaultCounts(nFPGA::nRoboRIO_FPGANamespace::tPower::tFaultCounts)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tPower::tDisable getDisabled()const noexcept
         * \brief Get the set of disabled power rails
         * \return The disabled power rails
         */

        nFPGA::nRoboRIO_FPGANamespace::tPower::tDisable getDisabled()const noexcept;

        /**
         * \fn void setDisabled(nFPGA::nRoboRIO_FPGANamespace::tPower::tDisable d)noexcept
         * \brief Set the set of disabled power rails
         * \param d The disabled power rails to use
         */

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
