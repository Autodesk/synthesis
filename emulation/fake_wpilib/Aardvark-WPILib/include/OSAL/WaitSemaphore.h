#ifndef __WAIT_SEMAPHORE_H
#define __WAIT_SEMAPHORE_H

#include <OSAL/OSAL.h>

#if USE_WINAPI
#include <Windows.h>
#elif USE_POSIX
#include <pthread.h>
#include <time.h>
#include <stdint.h>
#endif

class WaitSemaphore {
private:
#if USE_WINAPI
	HANDLE _handle;
#elif USE_POSIX
	pthread_cond_t signal;
	pthread_mutex_t mutex;
#endif
public:
	WaitSemaphore(void) {
#if USE_WINAPI
		_handle = CreateEvent(0, FALSE, FALSE, 0);
#elif USE_POSIX
		pthread_cond_init(&signal, NULL);
		pthread_mutex_init(&mutex, NULL);
#endif
	}

	~WaitSemaphore(void) {
#if USE_WINAPI
		CloseHandle(_handle);
#elif USE_POSIX
		pthread_cond_destroy(&signal);
		pthread_mutex_destroy(&mutex);
#endif
	}

	/**
	 * time in millis
	 */
	bool wait(unsigned long waitTime = INFINITE) {
#if USE_WINAPI
		return WaitForSingleObject(_handle, waitTime) == 0;
#elif USE_POSIX
		if (waitTime != INFINITE) {
			uint64_t msTime = time(NULL) + waitTime;
			timespec absTime;
			absTime.tv_sec = msTime / 1000;
			absTime.tv_nsec = (msTime % 1000) * 1000000;
			if (pthread_mutex_timedlock(&mutex, &absTime) != 0) {
				// Lock failed.
				return false;
			}
			if (pthread_cond_timedwait(&signal, &mutex, &absTime) != 0) {
				pthread_mutex_unlock(&mutex); // Just for safety
				return false;
			}
			pthread_mutex_unlock(&mutex); // Because that is how timedwait works
			return true;
		} else {
			if (pthread_mutex_lock(&mutex) != 0) {
				// Lock failed.
				return false;
			}
			if (pthread_cond_wait(&signal, &mutex) != 0) {
				pthread_mutex_unlock(&mutex); // Just for safety
				return false;
			}
			pthread_mutex_unlock(&mutex); // Because that is how wait works
			return true;
		}
#endif
	}

	void notify() {
#if USE_WINAPI
		SetEvent(_handle);
#elif USE_POSIX
		pthread_mutex_lock(&mutex);
		pthread_cond_broadcast(&signal);
		pthread_mutex_unlock(&mutex);
#endif
	}
};

#endif
