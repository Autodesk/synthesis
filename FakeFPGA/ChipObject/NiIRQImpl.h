#ifndef __NIIRQ_IMPL
#define __NIIRQ_IMPL

#include <vector>
#include <OSAL/Synchronized.h>
#include <OSAL/WaitSemaphore.h>

class NiIRQ_Impl {
private:
	std::vector<WaitSemaphore*> waitQueue[32];
	ReentrantSemaphore waitQueueMutex;
	uint32_t lastSignal;
public:
	NiIRQ_Impl() {

	}

	/// Signals all the tasks waiting on any of the interrupts specified by the mask.
	/// @param mask The bitfield of interrupts to signal
	void signal(uint32_t mask) {
		waitQueueMutex.take();
		lastSignal = mask;
		for (int i = 0; i<32; i++) {
			if (mask & (1 << i)) {
				for (std::vector<WaitSemaphore*>::iterator itr = waitQueue[i].begin(); itr != waitQueue[i].end(); itr++) {
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
		for (int i = 0; i<32; i++) {
			if (mask & (1 << i)) {
				waitQueue[i].push_back(sig);
			}
		}
		waitQueueMutex.give();
		// Wait for signal...
		bool success = sig->wait(time);
		if (timedout != NULL) {
			*timedout = !success;
		}
		if (!success) return;
		// Remove signals
		waitQueueMutex.take();
		if (signaled != NULL){
			*signaled = lastSignal;
		}
		for (int i = 0; i<32; i++) {
			if (mask & (1 << i)) {
				for (std::vector<WaitSemaphore*>::iterator itr = waitQueue[i].begin(); itr != waitQueue[i].end(); itr++) {\
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