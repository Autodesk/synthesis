#include <ChipObject/tInterruptManager.h>
#include <stdio.h>

#ifndef __NI_CRITICAL_SECTION
#define __NI_CRITICAL_SECTION
#include <OSAL/Synchronized.h>
class ni::dsc::osdep::CriticalSection {
public:
	NTReentrantSemaphore sem;
};

#endif

namespace nFPGA {
	uint32_t tInterruptManager::_globalInterruptMask = 0;
	ni::dsc::osdep::CriticalSection *tInterruptManager::_globalInterruptMaskSemaphore = new ni::dsc::osdep::CriticalSection();
	tInterruptManager *tInterruptManager::_globalInterruptRef[sizeof(uint32_t) * 8];

	tInterruptManager::tInterruptManager(uint32_t interruptMask, bool watcher, tRioStatusCode *status) : tSystem(status){
		this->_interruptMask = interruptMask;
		this->_watcher = watcher;
		this->_enabled = false;
		*status = NiFpga_Status_Success;
	}

	tInterruptManager::~tInterruptManager() {
	}

	void tInterruptManager::registerHandler(tInterruptHandler handler, void *param, tRioStatusCode *status) {
		this->_handler = handler;
		this->_userParam = param;
		*status = NiFpga_Status_Success;
	}

	uint32_t tInterruptManager::watch(int32_t timeoutInMs, tRioStatusCode *status)
	{
		if (timeoutInMs == NiFpga_InfiniteTimeout) {
			timeoutInMs = INFINITE;
		}
		NiFpga_WaitOnIrqs(_DeviceHandle, NULL, _interruptMask, timeoutInMs, NULL, NULL);
		return 0;// wth guys.  plz explain
	}
	void tInterruptManager::enable(tRioStatusCode *status){
		*status = NiFpga_Status_Success;
		this->_enabled = true;
		unreserve(status);
	}
	void tInterruptManager::disable(tRioStatusCode *status){
		*status = NiFpga_Status_Success;
		this->_enabled = false;
		unreserve(status);
	}
	bool tInterruptManager::isEnabled(tRioStatusCode *status){
		*status = NiFpga_Status_Success;
		return this->_enabled;
	}

	void tInterruptManager::handler(){
		this->_handler(this->_interruptMask, this->_userParam);//Wth is this interrupt asserted mask
	}

	int tInterruptManager::handlerWrapper(tInterruptManager *pInterrupt){
		pInterrupt->handler();
		return 0;		// No error, right?  Right.
	}

	// wth do these do.
	void tInterruptManager::acknowledge(tRioStatusCode *status){
		// This isn't needed with my sketchy implementation!
	}

	void tInterruptManager::reserve(tRioStatusCode *status){
		tInterruptManager::_globalInterruptMaskSemaphore->sem.take();

		if ((tInterruptManager::_globalInterruptMask & this->_interruptMask) > 0) {
			*status = NiFpga_Status_AccessDenied;
			// You derped
			printf("Interrupt already in use!\n");
			tInterruptManager::_globalInterruptMaskSemaphore->sem.give();
			return;
		}
		tInterruptManager::_globalInterruptMask |= this->_interruptMask;
		for (int i = 0; i<sizeof(uint32_t) * 8; i++) {
			if (this->_interruptMask & (1 << i)) {
				_globalInterruptRef[i] = this;
			}
		}
		*status = NiFpga_Status_Success;
		tInterruptManager::_globalInterruptMaskSemaphore->sem.give();
	}
	void tInterruptManager::unreserve(tRioStatusCode *status){
		tInterruptManager::_globalInterruptMaskSemaphore->sem.take();
		tInterruptManager::_globalInterruptMask &= ~this->_interruptMask;
		for (int i = 0; i<sizeof(uint32_t) * 8; i++) {
			if (this->_interruptMask & (1 << i)) {
				_globalInterruptRef[i] = NULL;
			}
		}
		tInterruptManager::_globalInterruptMaskSemaphore->sem.give();
		*status = NiFpga_Status_Success;
	}
}