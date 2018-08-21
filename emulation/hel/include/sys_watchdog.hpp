#ifndef _SYS_WATCHDOG_HPP_
#define _SYS_WATCHDOG_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tSysWatchdog.h"

namespace hel{

    /**
     * \brief Manages data associated with Ni FPGA's tSysWatchdog class
     */

    struct SysWatchdog{
    private:
        /**
         * \brief Status information of SysWatchdog
         */

        nFPGA::nRoboRIO_FPGANamespace::tSysWatchdog::tStatus status;

    public:
        /**
         * \brief Get the status information
         * \return The status information of the SysWatchdog
         */

        nFPGA::nRoboRIO_FPGANamespace::tSysWatchdog::tStatus getStatus()const noexcept;

        /**
         * \fn void setStatus(nFPGA::nRoboRIO_FPGANamespace::tSysWatchdog::tStatus s)noexcept
         * \brief Set the status information
         * \param s The new status information to use
         */

        void setStatus(nFPGA::nRoboRIO_FPGANamespace::tSysWatchdog::tStatus)noexcept;

        /**
         * Constructor for SysWatchdog
         */

        SysWatchdog()noexcept;

        /**
         * Constructor for SysWatchdog
         * \param source A SysWatchdog object to copy
         */

        SysWatchdog(const SysWatchdog&)noexcept;
    };
}

#endif
