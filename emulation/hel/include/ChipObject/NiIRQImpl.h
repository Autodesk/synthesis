#ifndef __NIIRQ_IMPL
#define __NIIRQ_IMPL

#include <vector>
#include <OSAL/Synchronized.h>
#include <OSAL/WaitSemaphore.h>
#include <stdint.h>

typedef uint32_t INTERRUPT_MASK;

class NiIRQ_Impl {
public:
	static const int INTERRUPT_COUNT = sizeof(INTERRUPT_MASK) * 8;
private:
	std::vector<WaitSemaphore*> waitQueue[INTERRUPT_COUNT];
	ReentrantSemaphore waitQueueMutex;
	INTERRUPT_MASK lastSignal;
public:
	NiIRQ_Impl();

	/// Signals all the tasks waiting on any of the interrupts specified by the mask.
	/// @param mask The bitfield of interrupts to signal
	void signal(INTERRUPT_MASK mask);

	/// Waits for a signal on any of the interrupts specified by the mask.
	/// @param mask The bitfield of interrupts to signal
	/// @param time The maximum wait time for a signal in milliseconds
	/// @param signaled Optional pointer that the signal's mask is written to
	/// @param timedout Optional pointer that the specifies if the function timed out
	void waitFor(uint32_t mask, unsigned long time = INFINITE, uint32_t *signaled = NULL, uint8_t *timedout = NULL);
};

#endif