#include "interrupt_manager.hpp"

#include "error.hpp"
#include <unistd.h>
#include <thread>

// TODO this could all probably be done better

namespace nFPGA{
    tInterruptManager::tInterruptManager(uint32_t interruptMask, bool watcher, tRioStatusCode* status): tSystem(status),_interruptMask(interruptMask), _watcher(watcher){}

    tInterruptManager::~tInterruptManager(){}

    void tInterruptManager::registerHandler(tInterruptHandler handler, void* param, tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tInterruptManager::registerHandler");
        _handler = handler;
        _userParam = param;
        // Continually call the handler
		std::thread([&]{ // FIXME
			while(true){
				_handler(_interruptMask, _userParam);
				usleep(10000); 
			}
		}).detach();
    }

    uint32_t tInterruptManager::watch(int32_t timeoutInMs, bool ignorePrevious, tRioStatusCode* /*status*/){
        // hel::warnUnsupportedFeature("Function call tInterruptManager::watch");
        if(timeoutInMs == 10000 && ignorePrevious == false) 
		return !0;
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
