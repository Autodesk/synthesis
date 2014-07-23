#ifndef __FRC_NET_IMPL_H
#define __FRC_NET_IMPL_H

#include <OSAL/OSAL.h>
#include <OSAL/Synchronized.h>
#include <OSAL/WaitSemaphore.h>
#if USE_WINAPI
#include <Windows.h>
#endif

#include "FRCNetStructures.h"

class NTTask;

class FRCNetImpl
{
private:
	NTTask *task;	
	bool enabled;
	int runThread();
	static DWORD WINAPI runThreadWrapper(LPVOID ptr);
	int teamID;

	struct sockaddr_in robotAddress;
	struct sockaddr_in dsAddress;
	SOCKET robotSocket;
	SOCKET dsSocket;
	
	/**
	* THIS HANDLES ENDIANS!
	*/
	FRCCommonControlData lastDataPacket;
	WaitSemaphore newDataSemInternal;
	ReentrantSemaphore readingSem;
	ReentrantSemaphore writingSem;

	char sendBuffer[2048];
	FRCRobotControl ctl;
public:
	WaitSemaphore *newDataSem;
	ReentrantSemaphore *resyncSem;

	FRCNetImpl();
	~FRCNetImpl();
	/**
	* THIS HANDLES ENDIANS!
	*/
	void sendControl(FRCRobotControl ctl);
	void start();
	void stop();
	FRCCommonControlData getLastPacket();
	bool waitForNewPacket(int wait_ms);
};

#endif