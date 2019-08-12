#include "interrupt_manager.hpp"

#include "error.hpp"
#include <unistd.h>
#include <thread>

// TODO this could all probably be done better

namespace nFPGA{
    tInterruptManager::tInterruptManager(uint32_t interruptMask, bool watcher, tRioStatusCode* status): tSystem(status),_interruptMask(interruptMask), _watcher(watcher){}

    tInterruptManager::~tInterruptManager(){}

    void tInterruptManager::registerHandler(tInterruptHandler handler, void* param, tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tInterruptManager::registerHandler\n";
        _handler = handler;
        _userParam = param;
        std::thread([&]{ while(true){_handler(_interruptMask, _userParam); usleep(10000); ; }}).detach();
    }

    uint32_t tInterruptManager::watch(int32_t /*timeoutInMs*/, bool /*ignorePrevious*/, tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tInterruptManager::watch\n";
        return 0;
    }

    void tInterruptManager::enable(tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tInterruptManager::enable\n";
        _enabled = true;
    }

    void tInterruptManager::disable(tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tInterruptManager::disable\n";
        _enabled = false;
    }

    bool tInterruptManager::isEnabled(tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tInterruptManager::isEnabled\n";
        return _enabled;
    }
}
