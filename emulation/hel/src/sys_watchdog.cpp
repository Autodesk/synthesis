#include "roborio_manager.hpp"
#include "system_interface.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tSysWatchdog::tStatus SysWatchdog::getStatus()const noexcept{
        return status;
    }

    void SysWatchdog::setStatus(tSysWatchdog::tStatus s)noexcept{
        status = s;
    }

    SysWatchdog::SysWatchdog()noexcept:status(){}
    SysWatchdog::SysWatchdog(const SysWatchdog& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(status);
#undef COPY
    }

    struct SysWatchdogManager: public tSysWatchdog{
        tSystemInterface* getSystemInterface(){ //unnecessary for emulation
            return new SystemInterface();
        }

        tStatus readStatus(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->watchdog.getStatus();
        }

        bool readStatus_SystemActive(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->watchdog.getStatus().SystemActive;
        }

        bool readStatus_PowerAlive(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->watchdog.getStatus().PowerAlive;
        }

        uint16_t readStatus_SysDisableCount(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->watchdog.getStatus().SysDisableCount;
        }

        uint16_t readStatus_PowerDisableCount(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->watchdog.getStatus().PowerDisableCount;
        }

        void writeCommand(uint32_t /*value*/, tRioStatusCode* /*status*/){} //unnecessary for emulation

        uint32_t readCommand(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint32_t readChallenge(tRioStatusCode* /*status*/){ //unnecessary for emulation
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
