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
	}
};

#endif