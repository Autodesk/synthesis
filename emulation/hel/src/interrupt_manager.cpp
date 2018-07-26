#include "interrupt_manager.hpp"

namespace nFPGA{
    tInterruptManager::tInterruptManager(uint32_t interruptMask, bool watcher, tRioStatusCode* status) : tSystem(status){}
    tInterruptManager::~tInterruptManager(){}
    void tInterruptManager::registerHandler(tInterruptHandler handler, void* param, tRioStatusCode* status){}
    uint32_t tInterruptManager::watch(int32_t timeoutInMs, bool ignorePrevious, tRioStatusCode* status){}
    void tInterruptManager::enable(tRioStatusCode* status){}
    void tInterruptManager::disable(tRioStatusCode* status){}
    bool tInterruptManager::isEnabled(tRioStatusCode* status){}
}
