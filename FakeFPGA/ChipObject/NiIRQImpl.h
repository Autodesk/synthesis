#ifndef __NIIRQ_IMPL
#define __NIIRQ_IMPL

#include <vector>
#include <OSAL/Synchronized.h>
#include <OSAL/WaitSemaphore.h>

typedef uint32_t INTERRUPT_MASK;

class NiIRQ_Impl {
public:
	static const int INTERRUPT_COUNT = sizeof(INTERRUPT_MASK) * 8;
private:
	std::vector<WaitSemaphore*> waitQueue[INTERRUPT_COUNT];
	ReentrantSemaphore waitQueueMutex;
	INTERRUPT_MASK lastSignal;
public:
	NiIRQ_Impl() {

	}

	/// Signals all the tasks waiting on any of the interrupts specified by the mask.
	/// @param mask The bitfield of interrupts to signal
	void signal(INTERRUPT_MASK mask) {
		waitQueueMutex.take();
		lastSignal = mask;
		for (int i = 0; i<INTERRUPT_COUNT; i++) {
			if (mask & (1 << i)) {	// If we want to signal this interrupt
				for (std::vector<WaitSemaphore*>::iterator itr = waitQueue[i].begin();
					itr != waitQueue[i].end(); itr++) {	// Notify all waiting threads
					(*itr)->notify();
				}
			}
		}
		waitQueueMutex.give();
	}

	/// Waits for a signal on any of the interrupts specified by the mask.
	/// @param mask The bitfield of interrupts to signal
	/// @param time The maximum wait time for a signal in milliseconds
	/// @param signaled Optional pointer that the signal's mask is written to
	/// @param timedout Optional pointer that the specifies if the function timed out
	void waitFor(uint32_t mask, unsigned long time = INFINITE, uint32_t *signaled = NULL, uint8_t *timedout = NULL) {
		WaitSemaphore *sig = new WaitSemaphore();
		// Add signals...
		waitQueueMutex.take();
		for (int i = 0; i<sizeof(INTERRUPT_MASK) * 8; i++) {
			if (mask & (1 << i)) {	// If we want to wait on this interrupt
				waitQueue[i].push_back(sig);	// Add the semaphore to the notification list
			}
		}
		waitQueueMutex.give();
		// Wait for signal...
		bool success = sig->wait(time);	// Wait for the semaphore to be notified.
		if (timedout != NULL) {
			*timedout = !success;
		}
		if (!success) return;
		// Remove signals
		waitQueueMutex.take();
		if (signaled != NULL){
			*signaled = lastSignal;
		}
		for (int i = 0; i<INTERRUPT_COUNT; i++) {
			if (mask & (1 << i)) {	// If we wanted to signal this interrupt
				// Remove the sempahore from the notification list
				for (std::vector<WaitSemaphore*>::iterator itr = waitQueue[i].begin(); itr != waitQueue[i].end(); itr++) {
					if ((*itr) == sig) {
						waitQueue[i].erase(itr);
						break;
					}
				}
			}
		}
		waitQueueMutex.give();
		delete sig;
		if (timedout != NULL) {
			*timedout = false;
		}
	}
};

#endif