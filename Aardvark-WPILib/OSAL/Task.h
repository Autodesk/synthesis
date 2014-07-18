/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef __NTTASK_H__
#define __NTTASK_H__

#include "ErrorBase.h"

/**
 * WPI task is a wrapper for the native Task object.
 * All WPILib tasks are managed by a static task manager for simplified cleanup.
 **/
class NTTask : public ErrorBase
{
public:
	static const UINT32 kDefaultPriority = 101;
	static const INT32 kInvalidTaskID = -1;

	NTTask(const char* name, PTHREAD_START_ROUTINE function, INT32 priority = kDefaultPriority, UINT32 stackSize = 20000);
	virtual ~NTTask();

	bool Start(void * arg0);

	bool Restart();
	bool Stop();

	bool IsReady();
	bool IsSuspended();

	bool Suspend();
	bool Resume();

	bool Verify();

	INT32 GetPriority();
	bool SetPriority(INT32 priority);
	const char* GetName();
	INT32 GetID();

	PTHREAD_START_ROUTINE m_function;
	void * m_Arg;
private:
	char* m_taskName;

	bool StartInternal();
	HANDLE m_Handle;
	DWORD m_ID;
	UINT32 m_stackSize;
	INT32 m_priority;
	bool HandleError(char *lpszFunction, int code = 0);
	DISALLOW_COPY_AND_ASSIGN(NTTask);
};
typedef NTTask Task ;
#endif // __TASK_H__
