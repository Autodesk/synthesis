#include "roborio.h"

void NetCommRPCProxy_SetOccurFuncPointer(void (*Occur)(uint32_t)){
    hel::RoboRIOManager::getInstance()->net_comm.occurFunction = Occur;
}
