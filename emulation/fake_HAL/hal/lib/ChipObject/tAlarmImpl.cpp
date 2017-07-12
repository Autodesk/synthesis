#include "ChipObject/tAlarmImpl.h"

#include "ChipObject/NiFpgaState.h"
#include "ChipObject/NiIRQImpl.h"
#include "ChipObject/tGlobalImpl.h"
#include <OSAL/Task.h>
#include <OSAL/System.h>

namespace nFPGA
{
	tAlarm_Impl::tAlarm_Impl(NiFpgaState *state)
	{
		this->state = state;
		this->notifierTask = new NTTask("Notifier", &tAlarm_Impl::runNotifier);
		this->enabled = (uint32_t*) &state->fpgaRAM[kAlarm_Enable_Address];
		this->triggerTime = (uint32_t*) &state->fpgaRAM[kAlarm_TriggerTime_Address];
		*enabled = false;
		*triggerTime = 0;
		this->notifierTask->Start(this);
	}


	tAlarm_Impl::~tAlarm_Impl(void)
	{
		if (this->state->alarm == this) {
			this->state->alarm = NULL;
		}
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
			unsigned int delta = (*(impl->triggerTime) - cTime) / 1000;	// Determine how long before the time we want
			if (!impl->enabled || *(impl->triggerTime) < cTime) {	// If we aren't enabled or the timer has expired
				if (!signaled) {	// If we haven't already signaled then signal
					impl->state->getIRQManager()->signal(1 << kTimerInterruptNumber);
					signaled = true;
				}
				impl->changeSemaphore.wait();	// Wait for another alarm to be set
			} else if (delta < 2) {			// If it is really close (2ms) wait directly, then signal
				sleep_ms(2);
				impl->state->getIRQManager()->signal(1 << kTimerInterruptNumber);
				signaled = true;
			} else {
				sleep_ms(delta * 0.75);	// Wait for part of the delta
				signaled = false;
			}
		}
		return 0;
	}

	void tAlarm_Impl::writeEnable(bool value, tRioStatusCode *status)
	{
		bool oldEnabled = enabled;
		*enabled = value;
		changeSemaphore.notify();	// Notify the sleeper thread that this alarm was enabled
		*status = NiFpga_Status_Success;
	}
	bool tAlarm_Impl::readEnable(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return *enabled;
	}

	void tAlarm_Impl::writeTriggerTime(unsigned int value, tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		*triggerTime = value;
		changeSemaphore.notify();	// Notify the sleeper thread that a new alarm was added
	}
	unsigned int tAlarm_Impl::readTriggerTime(tRioStatusCode *status)
	{
		*status = NiFpga_Status_Success;
		return *triggerTime;
	}
}