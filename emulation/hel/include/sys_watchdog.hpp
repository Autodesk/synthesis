#ifndef _SYS_WATCHDOG_HPP_
#define _SYS_WATCHDOG_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tSysWatchdog.h"

namespace hel{

    struct SysWatchdog{
    private:
        nFPGA::nRoboRIO_FPGANamespace::tSysWatchdog::tStatus status;

    public:
        nFPGA::nRoboRIO_FPGANamespace::tSysWatchdog::tStatus getStatus()const;
        void setStatus(nFPGA::nRoboRIO_FPGANamespace::tSysWatchdog::tStatus);
        SysWatchdog();
    };
}

#endif
