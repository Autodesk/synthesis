#include "roborio.h"
#include <thread>
#include <cstdio>
#include <unistd.h>

std::thread __ds_spoofer; // TODO make this sadness leave

extern "C" {
    // TODO Fix
    void NetCommRPCProxy_SetOccurFuncPointer(void (*Occur)(uint32_t)){
        auto newData = [Occur](uint32_t x) {
            usleep(10000);
            Occur(x);
        };
        hel::RoboRIOManager::getInstance()->net_comm.occurFunction = newData;
        __ds_spoofer = std::thread([newData](){while(1){newData(42);}});
    }
}
