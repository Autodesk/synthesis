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

#include <OSAL/Task.h>

namespace nFPGA {
	uint32_t tInterruptManager::_globalInterruptMask = 0;
	ni::dsc::osdep::CriticalSection *tInterruptManager::_globalInterruptMaskSemaphore = new ni::dsc::osdep::CriticalSection();

	tInterruptManager::tInterruptManager(uint32_t interruptMask, bool watcher, tRioStatusCode *status) : tSystem(status){
		this->_interruptMask = interruptMask;
		this->_watcher = watcher;
		this->_enabled = false;
		this->_handler = NULL;
		*status = NiFpga_Status_Success;
		if (!watcher) {
			enable(status);
		}
	}

	tInterruptManager::~tInterruptManager() {
	}

	class tInterruptManager::tInterruptThread {
	private:
		friend class tInterruptManager;
		NTTask task;
		static DWORD WINAPI invokeInternal(LPVOID param){
			return tInterruptManager::handlerWrapper((tInterruptManager*) param);
		}
		tInterruptThread() :
			task("Interruptwaiter", &tInterruptThread::invokeInternal)
		{
		}
	};

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
		reserve(status);
		bool old = this->_enabled;
		this->_enabled = true;
		if (old) {
			this->_thread->task.Stop();
		}
		this->_thread = new tInterruptThread();
		this->_thread->task.Start(this);
	}
	void tInterruptManager::disable(tRioStatusCode *status){
		*status = NiFpga_Status_Success;
		unreserve(status);
		bool old = this->_enabled;
		this->_enabled = false;
		if (old) {
			this->_thread->task.Stop();
		}
	}
	bool tInterruptManager::isEnabled(tRioStatusCode *status){
		*status = NiFpga_Status_Success;
		return this->_enabled;
	}

	void tInterruptManager::handler(){
		// Don't use this.  It is stupid.  Doesn't provide irqsAsserted
	}

	/// Background task to wait on the IRQ signal until seen, then call the handler.
	int tInterruptManager::handlerWrapper(tInterruptManager *pInterrupt){
		while (pInterrupt->_enabled) {
			NiFpga_Bool failed = false;
			uint32_t irqsAsserted = pInterrupt->_interruptMask;
			NiFpga_WaitOnIrqs(_DeviceHandle, NULL, pInterrupt->_interruptMask, INFINITE, &irqsAsserted, &failed);	// Wait until interrupted
			if (!failed && pInterrupt->_handler!=NULL) {
				pInterrupt->_handler(irqsAsserted, pInterrupt->_userParam);		// Notify whomever subscribed
			}
		}
		return 0;		// No error, right?  Right.
	}

	void tInterruptManager::acknowledge(tRioStatusCode *status){
		// Supposed to tell the IRQ manager that this successfully processed the IRQ.  But that is difficult
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
		*status = NiFpga_Status_Success;
		tInterruptManager::_globalInterruptMaskSemaphore->sem.give();
	}
	void tInterruptManager::unreserve(tRioStatusCode *status){
		tInterruptManager::_globalInterruptMaskSemaphore->sem.take();
		tInterruptManager::_globalInterruptMask &= ~this->_interruptMask;
		tInterruptManager::_globalInterruptMaskSemaphore->sem.give();
		*status = NiFpga_Status_Success;
	}
}