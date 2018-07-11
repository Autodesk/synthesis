#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tSysWatchdog::tStatus RoboRIO::SysWatchdog::getStatus()const{
        return status;
    }

    void RoboRIO::SysWatchdog::setStatus(tSysWatchdog::tStatus s){
        status = s;
    }
    
    struct SysWatchdogManager: public tSysWatchdog{
        tSystemInterface* getSystemInterface(){ //unnecessary for emulation
           return nullptr;
        }

        tStatus readStatus(tRioStatusCode* /*status*/){
           return RoboRIOManager::getInstance()->watchdog.getStatus();
        }

        bool readStatus_SystemActive(tRioStatusCode* /*status*/){
           return RoboRIOManager::getInstance()->watchdog.getStatus().SystemActive;
        }

        bool readStatus_PowerAlive(tRioStatusCode* /*status*/){
           return RoboRIOManager::getInstance()->watchdog.getStatus().PowerAlive;
        }

        uint16_t readStatus_SysDisableCount(tRioStatusCode* /*status*/){
           return RoboRIOManager::getInstance()->watchdog.getStatus().SysDisableCount;
        }

        uint16_t readStatus_PowerDisableCount(tRioStatusCode* /*status*/){
           return RoboRIOManager::getInstance()->watchdog.getStatus().PowerDisableCount;
        }

        void writeCommand(uint16_t /*value*/, tRioStatusCode* /*status*/){} //unnecessary for emulation

        uint16_t readCommand(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint8_t readChallenge(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        void writeActive(bool /*value*/, tRioStatusCode* /*status*/){} //unnecessary for emulation

        bool readActive(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint32_t readTimer(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint16_t readForcedKills(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tSysWatchdog* tSysWatchdog::create(tRioStatusCode* /*status*/){
            return new hel::SysWatchdogManager();
        }
    }
}
