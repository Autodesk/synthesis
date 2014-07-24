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
public:
	enum EmbeddedDynamicChunk {
		kEmbeddedUserDataHigh = 0,
		kEmbeddedErrors = 1,
		kEmbeddedUserDataLow = 2,
		kEmbeddedCount
	};
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
	DynamicControlData lastDynamicControlPacket [32];
	WaitSemaphore newDataSemInternal;
	ReentrantSemaphore readingSem;
	ReentrantSemaphore writingSem;

	char sendBuffer[2048];
	FRCRobotControl ctl;
	struct {
		uint32_t dynamicLen;
		const char *dynamicData;
	} embeddedDynamicChunks[kEmbeddedCount];
public:
	WaitSemaphore *newDataSem;
	ReentrantSemaphore *resyncSem;

	FRCNetImpl();
	~FRCNetImpl();
	/**
	* THIS HANDLES ENDIANS!
	*/
	void setStatus(int battery, uint8_t dsDigitalOut,
		uint8_t updateNumber, const char *userDataHigh, int userDataHighLength,
		const char *userDataLow, int userDataLowLength, uint8_t control);
	void setEmbeddedDynamicChunk(EmbeddedDynamicChunk chunk, const char *errors, int errorsLength, bool lock = true);
	void start();
	void stop();
	FRCCommonControlData getLastPacket();
	bool waitForNewPacket(int wait_ms);
	uint8_t getDynamicData(uint8_t type, char *dynamicData,
		int32_t maxLength);
};

#endif