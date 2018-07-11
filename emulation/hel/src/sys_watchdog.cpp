#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    struct SysWatchdogManager: public tSysWatchdog{
        tSystemInterface* getSystemInterface(){
           //TODO
        }

        tStatus readStatus(tRioStatusCode* status){
           //TODO
        }

        bool readStatus_SystemActive(tRioStatusCode* status){
           //TODO
        }

        bool readStatus_PowerAlive(tRioStatusCode* status){
           //TODO
        }

        unsigned short readStatus_SysDisableCount(tRioStatusCode* status){
           //TODO
        }

        unsigned short readStatus_PowerDisableCount(tRioStatusCode* status){
           //TODO
        }

        void writeCommand(unsigned short value, tRioStatusCode* status){
           //TODO
        }

        unsigned short readCommand(tRioStatusCode* status){
           //TODO
        }

        unsigned char readChallenge(tRioStatusCode* status){
           //TODO
        }

        void writeActive(bool value, tRioStatusCode* status){
           //TODO
        }

        bool readActive(tRioStatusCode* status){
           //TODO
        }

        unsigned int readTimer(tRioStatusCode* status){
           //TODO
        }

        unsigned short readForcedKills(tRioStatusCode* status){
           //TODO
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tSysWatchdog* tSysWatchdog::create(tRioStatusCode* status){
            *status = 0;
            return new hel::SysWatchdogManager();
        }
    }
}
