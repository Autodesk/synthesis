#ifndef __T_ALARM_IMPL
#define __T_ALARM_IMPL
#include <ChipObject/tAlarm.h>
#include <OSAL/OSAL.h>
#include <OSAL/WaitSemaphore.h>
class NTTask;

namespace nFPGA
{
	class NiFpgaState;

	/// Broadcasts an interrupt on IRQ 28 once the specified trigger time has been reached.
	class tAlarm_Impl :
		public nFPGA::nFRC_2012_1_6_4::tAlarm
	{
	private:
		static const int kAlarm_TriggerTime_Address = 0x8444;
		static const int kAlarm_Enable_Address = 0x8448;

	private:
		static const uint32_t kTimerInterruptNumber = 28;

		NiFpgaState *state;
		uint32_t *enabled;
		uint32_t *triggerTime;
		NTTask *notifierTask;
		WaitSemaphore changeSemaphore;

		static DWORD WINAPI runNotifier(LPVOID arg);
	public:
		tAlarm_Impl(NiFpgaState *state);
		virtual ~tAlarm_Impl(void);

		virtual tSystemInterface* getSystemInterface();

		virtual void writeEnable(bool value, tRioStatusCode *status);
		virtual bool readEnable(tRioStatusCode *status);

		virtual void writeTriggerTime(unsigned int value, tRioStatusCode *status);
		virtual unsigned int readTriggerTime(tRioStatusCode *status);

	};

}
#endif