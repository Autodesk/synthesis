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
	WaitSemaphore *newDataSem;
	WaitSemaphore *resyncSem;
	ReentrantSemaphore readingSem;

	char sendBuffer[2048];
public:
	FRCNetImpl();
	~FRCNetImpl();
	/**
	* THIS HANDLES ENDIANS!
	*/
	void sendControl(FRCRobotControl ctl);
	void start();
	void stop();
	FRCCommonControlData getLastPacket();
};

#endif