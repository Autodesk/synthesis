/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef NT_SYNCHRONIZED_H
#define NT_SYNCHRONIZED_H

#define NT_CRITICAL_REGION(s) { NTSynchronized _sync(s);
#define CRITICAL_REGION NT_CRITICAL_REGION
#define NT_END_REGION }
#define END_REGION NT_END_REGION

#include <Windows.h>

class NTReentrantSemaphore
{
public:
	explicit NTReentrantSemaphore(){
		// vxworks m_semaphore = semMCreate(SEM_Q_PRIORITY | SEM_INVERSION_SAFE | SEM_DELETE_SAFE);
		m_semaphore = CreateMutex(NULL, FALSE, NULL);
	};
	~NTReentrantSemaphore(){
		// vxworks semDelete(m_semaphore);
		CloseHandle(m_semaphore);
	};
	bool take(){
		// vxworks semTake(m_semaphore, WAIT_FOREVER);
		return WaitForSingleObject(m_semaphore, INFINITE) == 0;
	};
	bool takeImmediate(){
		// vxworks semTake(m_semaphore, WAIT_FOREVER);
		return WaitForSingleObject(m_semaphore, 0) == 0;
	};
	void give(){
		// vxworks semGive(m_semaphore);
		ReleaseMutex(m_semaphore);
	};
private:
	HANDLE m_semaphore;
};

typedef NTReentrantSemaphore ReentrantSemaphore ;

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

typedef NTSynchronized Synchronized;

#endif
