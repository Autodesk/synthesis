#include "tAlarmImpl.h"

#include "NiFpgaState.h"
#include "NiIRQImpl.h"
#include "tGlobalImpl.h"
#include <OSAL/Task.h>
#include <OSAL/System.h>

namespace nFPGA
{
	tAlarm_Impl::tAlarm_Impl(NiFpgaState *state)
	{
		this->state = state;
		this->notifierTask = new NTTask("Notifier", &tAlarm_Impl::runNotifier);
		this->enabled = false;
		this->triggerTime = 0;
		this->notifierTask->Start(this);
	}


	tAlarm_Impl::~tAlarm_Impl(void)
	{
		delete this->notifierTask;
	}

	tSystemInterface *tAlarm_Impl::getSystemInterface() {
		return state;
	}

	DWORD tAlarm_Impl::runNotifier(LPVOID param) {
		tAlarm_Impl *impl = (tAlarm_Impl*) param;
		tRioStatusCode status;
		bool signaled = false;
		while (true) {
			unsigned int cTime = impl->state->getGlobal()->readLocalTime(&status);
			unsigned int delta = (impl->triggerTime - cTime) / 1000;
			if (!impl->enabled || impl->triggerTime < cTime) {
				if (!signaled) {
					impl->state->getIRQManager()->signal(0xFFFFFFFF);// SIGNAL ALL THE THINGS (wth?)
					signaled = true;
				}
				impl->changeSemaphore.wait();
			} else if (delta < 2) {
				sleep_ms(2);
				impl->state->getIRQManager()->signal(0xFFFFFFFF);// SIGNAL ALL THE THINGS (wth?)
				signaled = true;
			} else {
				sleep_ms(delta * 0.75);
				signaled = false;
			}
		}
		return 0;
	}

	void tAlarm_Impl::writeEnable(bool value, tRioStatusCode *status)
	{
		bool oldEnabled = enabled;
		enabled = value;
		changeSemaphore.notify();
		*status = NiFpga_Status_Success;
	}
	bool tAlarm_Impl::readEnable(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return enabled;
	}

	void tAlarm_Impl::writeTriggerTime(unsigned int value, tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		triggerTime = value;
		changeSemaphore.notify();
	}
	unsigned int tAlarm_Impl::readTriggerTime(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return triggerTime;
	}
}