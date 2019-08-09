#include "roborio_manager.hpp"

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
        instance.first->net_comm.occurFunction = Occur;
        instance.second.unlock();
    }
}
