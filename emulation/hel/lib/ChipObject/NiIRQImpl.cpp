#include "ChipObject/NiIRQImpl.h"
#include <stdint.h>

NiIRQ_Impl::NiIRQ_Impl() {

}

void NiIRQ_Impl::signal(INTERRUPT_MASK mask) {
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

void NiIRQ_Impl::waitFor(uint32_t mask, unsigned long time, uint32_t *signaled, uint8_t *timedout) {
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
	if (success && timedout != NULL) {
		*timedout = false;
	}
}