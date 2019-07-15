#include "interrupt_manager.hpp"

#include "error.hpp"

#include <thread>

// TODO this could all probably be done better

namespace nFPGA{
    tInterruptManager::tInterruptManager(uint32_t interruptMask, bool watcher, tRioStatusCode* status): tSystem(status),_interruptMask(interruptMask), _watcher(watcher){}

    tInterruptManager::~tInterruptManager(){}

    void tInterruptManager::registerHandler(tInterruptHandler handler, void* param, tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tInterruptManager::registerHandler");
        _handler = handler;
        _userParam = param;
        std::thread([&]{ while(true){_handler(_interruptMask, _userParam); }}).detach();
    }

    uint32_t tInterruptManager::watch(int32_t /*timeoutInMs*/, bool /*ignorePrevious*/, tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tInterruptManager::watch");
        return 0;
    }

    void tInterruptManager::enable(tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tInterruptManager::enable");
        _enabled = true;
    }

    void tInterruptManager::disable(tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tInterruptManager::disable");
        _enabled = false;
    }

    bool tInterruptManager::isEnabled(tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tInterruptManager::isEnabled");
        return _enabled;
    }
}
