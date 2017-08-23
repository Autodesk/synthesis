/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef NT_SYNCHRONIZED_H
#define NT_SYNCHRONIZED_H
#include "OSAL/OSAL.h"

#define NT_CRITICAL_REGION(s) { NTSynchronized _sync(s);
#define CRITICAL_REGION NT_CRITICAL_REGION
#define NT_END_REGION }
#define END_REGION NT_END_REGION

#if USE_WINAPI
	#include <Windows.h>
#elif USE_POSIX
	#include <pthread.h>
#endif

class NTReentrantSemaphore
{
public:
	explicit NTReentrantSemaphore(){
		// vxworks m_semaphore = semMCreate(SEM_Q_PRIORITY | SEM_INVERSION_SAFE | SEM_DELETE_SAFE);
#if USE_WINAPI
		m_semaphore = CreateMutex(NULL, FALSE, NULL);
#elif USE_POSIX
		pthread_mutexattr_t attr;
		pthread_mutexattr_init(&attr);
		pthread_mutex_init(&m_semaphore, &attr);
#endif
	}

	~NTReentrantSemaphore(){
		// vxworks semDelete(m_semaphore);
#if USE_WINAPI
		CloseHandle(m_semaphore);
#elif USE_POSIX
		pthread_mutex_destroy(&m_semaphore);
#endif
	}

	bool take(){
		// vxworks semTake(m_semaphore, WAIT_FOREVER);
#if USE_WINAPI
		return WaitForSingleObject(m_semaphore, INFINITE) == 0;
#elif USE_POSIX
		return pthread_mutex_lock(&m_semaphore) == 0;
#endif
	}

	bool takeImmediate(){
		// vxworks semTake(m_semaphore, WAIT_FOREVER);
#if USE_WINAPI
		return WaitForSingleObject(m_semaphore, 0) == 0;
#elif USE_POSIX
		return pthread_mutex_trylock(&m_semaphore) == 0;
#endif
	}

	void give(){
		// vxworks semGive(m_semaphore);
#if USE_WINAPI
		ReleaseMutex(m_semaphore);
#elif USE_POSIX
		pthread_mutex_unlock(&m_semaphore);
#endif
	}

private:
#if USE_WINAPI
	HANDLE m_semaphore;
#elif USE_POSIX
	pthread_mutex_t m_semaphore;
#endif
};

/**
* Provide easy support for critical regions.
* A critical region is an area of code that is always executed under mutual exclusion. Only
* one task can be executing this code at any time. The idea is that code that manipulates data
* that is shared between two or more tasks has to be prevented from executing at the same time
* otherwise a race condition is possible when both tasks try to update the data. Typically
* semaphores are used to ensure only single task access to the data.
* Synchronized objects are a simple wrapper around semaphores to help ensure that semaphores
* are always signaled (semGive) after a wait (semTake).
*/
class NTSynchronized
{
public:
	explicit NTSynchronized(NTReentrantSemaphore&);
	explicit NTSynchronized(NTReentrantSemaphore*);
	virtual ~NTSynchronized();
private:
	NTReentrantSemaphore *m_sem;
};

typedef NTReentrantSemaphore ReentrantSemaphore ;
typedef NTSynchronized Synchronized ;

#endif
