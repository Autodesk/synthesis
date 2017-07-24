/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef __NTTASK_H__
#define __NTTASK_H__

#include "ErrorBase.h"
#include <stdint.h>
#include "OSAL/OSAL.h"

/**
 * WPI task is a wrapper for the native Task object.
 * All WPILib tasks are managed by a static task manager for simplified cleanup.
 **/
class NTTask: public frc::ErrorBase {
public:
	enum NTTaskPriority {
		kBackgroundBegin = 0x00010000,
		kBackgroundEnd = 0x00020000,
		kAboveNormal = 1,
		kBelowNormal = -1,
		kHighest = 2,
		kIdle = -15,
		kLowest = -2,
		kNormal = 0,
		kCritical = 15,
	};

	NTTask(const char* name, PTHREAD_START_ROUTINE function, NTTaskPriority priority =
			kNormal);
	virtual ~NTTask();

	bool Start(void * arg0);

	bool Restart();
	bool Stop();

	bool IsReady();
	bool IsSuspended();

	bool Suspend();
	bool Resume();

	bool Verify();
	const char* GetName();

	PTHREAD_START_ROUTINE m_function;
	void * m_Arg;
private:
	char* m_taskName;
	NTTaskPriority m_priority;

	bool StartInternal();
	bool valid;
#if USE_WINAPI
	HANDLE m_Handle;
#elif USE_POSIX
	pthread_t m_Handle;
	static void* funcWrapper(void *task);
#endif
	bool HandleError(char *lpszFunction, int code = 0);DISALLOW_COPY_AND_ASSIGN(NTTask);
};

typedef NTTask Task ;

#endif // __TASK_H__
