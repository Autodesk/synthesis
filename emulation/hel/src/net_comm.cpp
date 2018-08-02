#include "roborio_manager.hpp"
#include <thread>
#include <cstdio>
#include <unistd.h>

std::thread ds_spoofer;

namespace hel{
    NetComm::NetComm()noexcept:ref_num(),occurFunction(){}
}

extern "C" {
    void NetCommRPCProxy_SetOccurFuncPointer(void (*Occur)(uint32_t)){
        auto instance = hel::RoboRIOManager::getInstance();
        auto newData = [Occur](uint32_t x) {
            usleep(10000);
            Occur(x);
        };
        instance.first->net_comm.occurFunction = newData;
        ds_spoofer = std::thread([newData](){while(1){newData(42);}});
        hel::hal_is_initialized.store(true);
        instance.second.unlock();
    }
}
