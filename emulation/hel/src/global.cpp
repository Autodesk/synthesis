#include "roborio.h"
#include <chrono>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    RoboRIO::Global::Global(){
    	fpga_start_time = getCurrentTime();//std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
    }
    
    uint64_t RoboRIO::Global::getCurrentTime(){
        return std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
    }

    uint64_t RoboRIO::Global::getFPGAStartTime()const{
        return fpga_start_time;
    }

    struct GlobalManager: public tGlobal{
        tSystemInterface* getSystemInterface(){
            return nullptr;
        }

        void writeLEDs(tLEDs /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation
        
        void writeLEDs_Comm(uint8_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation

        void writeLEDs_Mode(uint8_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation

        void writeLEDs_RSL(bool /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation

        tLEDs readLEDs(tRioStatusCode* /*status*/){//unnecessary for emulation
            return *(new tGlobal::tLEDs);
        }

        uint8_t readLEDs_Comm(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint8_t readLEDs_Mode(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        bool readLEDs_RSL(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return false;
        }

        uint32_t readLocalTimeUpper(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return (RoboRIO::Global::getCurrentTime() - instance.first->global.getFPGAStartTime()) >> 32;
        }
        
        uint16_t readVersion(tRioStatusCode* /*status*/){
          return 2018; //WPILib assumes this is the competition year
        }

        uint32_t readLocalTime(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return (uint32_t)(RoboRIO::Global::getCurrentTime() - instance.first->global.getFPGAStartTime());
        }

        bool readUserButton(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->user_button;
        }

        uint32_t readRevision(tRioStatusCode* /*status*/){
            return 0; //TODO?
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
    	tGlobal* tGlobal::create(tRioStatusCode* /*status*/){
    		return new hel::GlobalManager();
    	}
    }
}
