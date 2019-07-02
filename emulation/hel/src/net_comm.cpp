#include "roborio_manager.hpp"
#include <thread>
#include <cstdio>
#include <unistd.h>

std::thread ds_spoofer;

namespace hel{
    NetComm::NetComm()noexcept:ref_num(),occurFunction(){}
    NetComm::NetComm(const NetComm& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(ref_num);
        COPY(occurFunction);
#undef COPY
    }
}

extern "C" {
    void NetCommRPCProxy_SetOccurFuncPointer(void (*Occur)(uint32_t)){
        auto instance = hel::RoboRIOManager::getInstance();
        auto newData = [Occur](uint32_t x) {
            usleep(10000);
            Occur(x);
        };
        instance.first->net_comm.occurFunction = newData;
        ds_spoofer = std::thread( //call Occur repeatably in the background to signal HAL that the Driver Station has new data for it; this way it won't block and will actually receive HEL DS data
            [newData](){
                while(1){
                    newData(42); //TODO use NetComm ref_num instead of 42
                }
            }
        );
        ds_spoofer.detach();
        hel::hal_is_initialized.store(true);
        instance.second.unlock();
    }
}
