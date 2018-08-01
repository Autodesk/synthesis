#include "interrupt_manager.hpp"

#include "error.hpp"

namespace nFPGA{
    tInterruptManager::tInterruptManager(uint32_t /*interruptMask*/, bool /*watcher*/, tRioStatusCode* status) : tSystem(status){}

    tInterruptManager::~tInterruptManager(){}

    void tInterruptManager::registerHandler(tInterruptHandler /*handler*/, void* /*param*/, tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Function call tInterruptManager::registerHandler\n";
    }

    uint32_t tInterruptManager::watch(int32_t /*timeoutInMs*/, bool /*ignorePrevious*/, tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Function call tInterruptManager::watch\n";
        return 0;
    }

    void tInterruptManager::enable(tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Function call tInterruptManager::enable\n";
    }

    void tInterruptManager::disable(tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Function call tInterruptManager::disable\n";
    }

    bool tInterruptManager::isEnabled(tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Function call tInterruptManager::isEnabled\n";
        return false;
    }
}
