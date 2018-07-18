#include "roborio.h"
#include <thread>
#include <cstdio>
#include <unistd.h>

std::thread ds_spoofer;

namespace hel{
    NetComm::NetComm():ref_num(),occurFunction(){}
}

extern "C" {
    // TODO Fix
    void NetCommRPCProxy_SetOccurFuncPointer(void (*Occur)(uint32_t)){
        auto instance = hel::RoboRIOManager::getInstance();
        auto newData = [Occur](uint32_t x) {
            usleep(10000);
            Occur(x);
        };
        instance.first->net_comm.occurFunction = newData;
        ds_spoofer = std::thread([newData](){while(1){newData(42);}});
        instance.second.unlock();
    }
}
