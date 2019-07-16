#include "roborio_manager.hpp"
#include <chrono>

#include "sync_server.hpp"
#include "sync_client.hpp"

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tGlobal.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    constexpr const float TIME_CONSTANT = 1.13; // This is the offset from local time to real world time
    Global::Global()noexcept{
        fpga_start_time = getCurrentTime();
    }

    Global::Global(const Global& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(fpga_start_time);
#undef COPY
    }

    uint64_t Global::getCurrentTime()noexcept{
        return std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::high_resolution_clock::now().time_since_epoch()/TIME_CONSTANT).count(); //TODO system time runs fast, using a scalar for now
    }

    uint64_t Global::getFPGAStartTime()const noexcept{
        return fpga_start_time;
    }

    struct GlobalManager: public tGlobal{
        tSystemInterface* getSystemInterface(){
            return new SystemInterface();
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
            return (Global::getCurrentTime() - instance.first->global.getFPGAStartTime()) >> 32;
        }

        uint16_t readVersion(tRioStatusCode* /*status*/){
            return 2018; //WPILib assumes this is the competition year
        }

        uint32_t readLocalTime(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return (uint32_t)(Global::getCurrentTime() - instance.first->global.getFPGAStartTime());
        }

        bool readUserButton(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->user_button;
        }

        uint32_t readRevision(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }
    };
}

std::thread sync_thread_send;
std::thread sync_thread_receive;

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tGlobal* tGlobal::create(tRioStatusCode* /*status*/){
            sync_thread_send = std::thread([](){
                                               asio::io_service service;
                                               hel::SyncServer serv(service);
                                           });
            sync_thread_receive = std::thread([](){
                                                  asio::io_service service;
                                                  hel::SyncClient serv(service);
                                              });
            sync_thread_send.detach();
            sync_thread_receive.detach();
            return new hel::GlobalManager();
        }
    }
}
