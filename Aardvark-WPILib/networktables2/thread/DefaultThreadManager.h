/*
 * DefaultThreadManager.h
 * Desktop
 *
 *  Created on: Sep 21, 2012
 *      Author: Mitchell Wills
 */

#ifndef DEFAULTTHREADMANAGER_H_
#define DEFAULTTHREADMANAGER_H_


class DefaultThreadManager;
class PeriodicNTThread;

#include "networktables2/thread/PeriodicRunnable.h"
#include "networktables2/thread/NTThreadManager.h"
#include "networktables2/thread/NTThread.h"
#include "OSAL/Task.h"

#include <Windows.h>

class DefaultThreadManager : public NTThreadManager{
public:
	virtual NTThread* newBlockingPeriodicThread(PeriodicRunnable* r, const char* name);
};

class PeriodicNTThread : public NTThread {
private:
	const char* name;
	NTTask *thread;
	PeriodicRunnable* r;
	bool run;
	int _taskMain();
	static DWORD WINAPI taskMain(LPVOID o);
public:
	PeriodicNTThread(PeriodicRunnable* r, const char* name);
	virtual ~PeriodicNTThread();
	virtual void stop();
	virtual bool isRunning();
};

#endif /* DEFAULTTHREADMANAGER_H_ */
