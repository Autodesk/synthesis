#include "FRCNetImpl.h"
#include <OSAL/Task.h>
#include <stdint.h>
#include "crc32.h"
#include "FRCFakeNetComm.h"
#include "DriverStationEnhancedIO.h"

extern "C" {
	FRCNetImpl *frcNetInstance = NULL;
}

FRCNetImpl *GetFakeNetComm() {
	if (frcNetInstance == NULL) {
		frcNetInstance = new FRCNetImpl();
		frcNetInstance->start();
	}
	return frcNetInstance;
}

FRCNetImpl::FRCNetImpl(void)
{
	enabled = false;
	task = NULL;
	teamID = 1510;
	newDataSem = NULL;
	resyncSem = NULL;
	memset(&ctl, 0, sizeof(ctl));
	for (int i = 0; i<kEmbeddedCount; i++){ 
		embeddedDynamicChunks[i].dynamicData = NULL;
		embeddedDynamicChunks[i].dynamicLen = 0;
	}
}


FRCNetImpl::~FRCNetImpl(void)
{
	if (task!=NULL){
		delete task;
	}
}

void FRCNetImpl::start() {
	if (task!=NULL){
		return;
	}
	enabled = true;
	task = new Task("FRCNetImpl", &FRCNetImpl::runThreadWrapper);
	task->Start(this);
}

void FRCNetImpl::stop() {
	if (task==NULL) {
		return;
	}
	enabled = false;
	task->Stop();	// Meh cleanup needs doing
	task = NULL;
}

DWORD FRCNetImpl::runThreadWrapper(LPVOID ptr) {
	return ((FRCNetImpl*)ptr)->runThread();
}

int FRCNetImpl::runThread() {
	WSADATA wsa;
	WSAStartup(MAKEWORD(2,2),&wsa);		// Hope and pray that this works.

	uint32_t network = (10 << 24) | (((teamID / 100) & 0xFF) << 16) | ((teamID % 100) << 8) | 0;

	robotAddress.sin_family = AF_INET;
	robotAddress.sin_addr.s_addr = htonl(network | 2);
	robotAddress.sin_port = htons( 1110 );

	dsAddress.sin_family = AF_INET;
	dsAddress.sin_addr.s_addr = htonl(network | 5);
	dsAddress.sin_port = htons( 1150 );

	robotSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (robotSocket < 0) {
		fprintf(stderr, "Could not create socket ROBOT!\n");
		scanf_s("\n");
		exit(2);
	}

	if (bind(robotSocket, (struct sockaddr *)&robotAddress, sizeof(robotAddress)) == SOCKET_ERROR) {
		fprintf(stderr, "Could not bind socket ROBOT!  Did you configure your loopback adapters?\n");
		scanf_s("\n");
		exit(2);
	}

	dsSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (dsSocket < 0) {
		fprintf(stderr, "Could not create socket DS!  Did you configure your loopback adapters?\n");
		scanf_s("\n");
		exit(2);
	}

	char buffer[1024];
	for (int i = 0; i < 32; i++)
	{
		memset (&lastDynamicControlPacket [i], 0, sizeof(lastDynamicControlPacket [i]));
	}
	bool lockedResync = false;
	// Read from DS thread
	while (enabled) {
		if (resyncSem != NULL && !lockedResync) {
			resyncSem->take();
			lockedResync = true;
		}
		int len = recv(robotSocket, (char*) &buffer, sizeof(buffer), 0);
		if (len < 0) {
			printf("Read failed\n");
		}
		readingSem.take();
		memcpy(&lastDataPacket, &buffer, sizeof(FRCCommonControlData));

		// Reading dynamic data
		{
			int head = 115;
			uint8_t size;
			uint8_t id;
			while (head < 1024 && (size = buffer[head]) > 0) {
				uint8_t id = buffer[head+1];
				lastDynamicControlPacket [id].id=id;
				lastDynamicControlPacket [id].size=size;
				if (lastDynamicControlPacket [id].data != NULL) {
					delete lastDynamicControlPacket [id].data;
					lastDynamicControlPacket [id].data = NULL;
				}
				lastDynamicControlPacket [id].data = (uint8_t*) malloc(size +2);
				memcpy(lastDynamicControlPacket [id].data, &buffer[head], size+2);
				head += size;
			}
		}

		{
			// Handle endians
			lastDataPacket.packetIndex = ntohs(lastDataPacket.packetIndex);
			lastDataPacket.teamID = ntohs(lastDataPacket.teamID);
			lastDataPacket.analog1 = ntohs(lastDataPacket.analog1);
			lastDataPacket.analog2 = ntohs(lastDataPacket.analog2);
			lastDataPacket.analog3 = ntohs(lastDataPacket.analog3);
			lastDataPacket.analog4 = ntohs(lastDataPacket.analog4);
			lastDataPacket.stick0Buttons = ntohs(lastDataPacket.stick0Buttons);
			lastDataPacket.stick1Buttons = ntohs(lastDataPacket.stick1Buttons);
			lastDataPacket.stick2Buttons = ntohs(lastDataPacket.stick2Buttons);
			lastDataPacket.stick3Buttons = ntohs(lastDataPacket.stick3Buttons);
		}
		readingSem.give();
		newDataSemInternal.notify();
		if (newDataSem != NULL) {
			newDataSem->notify();
		}
		// Shenanigans with semaphores
		if (lastDataPacket.resync) {
			if (resyncSem != NULL){
				resyncSem->give();
				Sleep(250);
				resyncSem->take();
			}
		}

		// Broadcast robot control ideas
		writingSem.take();
		memset(&sendBuffer, 0, sizeof(sendBuffer));
		memcpy(&sendBuffer, &ctl,  sizeof(FRCRobotControl));
		uint32_t pos = 0x21;
		for (int i = 0; i<kEmbeddedCount; i++){
			uint32_t slen = htonl(embeddedDynamicChunks[i].dynamicLen * (embeddedDynamicChunks[i].dynamicData != NULL ? 1 : 0));
			memcpy(&sendBuffer[pos], &slen, sizeof(slen));
			slen = ntohl(slen);
			if (slen > 0) {
				memcpy(&sendBuffer[pos + sizeof(slen)], embeddedDynamicChunks[i].dynamicData, slen);
				delete embeddedDynamicChunks[i].dynamicData;
				embeddedDynamicChunks[i].dynamicData = NULL;
				embeddedDynamicChunks[i].dynamicLen = 0;
			}
			pos += sizeof(slen) + slen;
		}
		uint32_t crc = crc32buf(sendBuffer, 0x400);
		crc = htonl(crc);
		memcpy(&sendBuffer[0x3fc], &crc, sizeof(DWORD));
		sendto(dsSocket,(const char *) &sendBuffer, 0x400, 0,(const sockaddr*)&dsAddress, sizeof(dsAddress));
		writingSem.give();
	}

	// Cleanup
	closesocket(dsSocket);
	closesocket(robotSocket);
	WSACleanup();

	return 0;
}

void FRCNetImpl::setStatus(int battery, uint8_t dsDigitalOut, 		uint8_t updateNumber, const char *userDataHigh, int userDataHighLength, 	const char *userDataLow, int userDataLowLength, uint8_t control) {
	FRCCommonControlData lastPacketCache = getLastPacket();
	// This can probably drop packets left and right
	writingSem.take();
	ctl.batteryVolts = (battery >> 8) & 0xFF;
	ctl.batteryMilliVolts = battery & 0xFF;
	ctl.teamID = lastPacketCache.teamID;
	ctl.packetIndex = lastPacketCache.packetIndex;
	ctl.control.control = control;

	ctl.packetIndex = htons(ctl.packetIndex);
	ctl.teamID = htons(ctl.packetIndex);

	setEmbeddedDynamicChunk(kEmbeddedUserDataLow, userDataLow, userDataLowLength, false);
	setEmbeddedDynamicChunk(kEmbeddedUserDataHigh, userDataHigh, userDataHighLength, false);

	writingSem.give();
}

void FRCNetImpl::setEmbeddedDynamicChunk(EmbeddedDynamicChunk chunk, const char *data, int len, bool lock) {
	if (lock){
		writingSem.take();
	}
	const char *dataCopy = (const char*) malloc(len);
	memcpy((void*) dataCopy, data, len);
	embeddedDynamicChunks[chunk].dynamicData = dataCopy;
	embeddedDynamicChunks[chunk].dynamicLen = len;
	if (lock) {
		writingSem.give();
	}
}

FRCCommonControlData FRCNetImpl::getLastPacket() {
	FRCCommonControlData copy;
	readingSem.take();
	copy = lastDataPacket;
	readingSem.give();
	return copy;
}

bool FRCNetImpl::waitForNewPacket(int wait_ms) {
	return newDataSemInternal.wait(wait_ms);
}

uint8_t FRCNetImpl::getDynamicData(uint8_t type, char *dynamicData, int32_t maxLength) {
	uint8_t len = 0;
	readingSem.take();
	len = lastDynamicControlPacket [type].size;
	if (len > 0) {
		memcpy (dynamicData, lastDynamicControlPacket [type].data,min(len+1, maxLength)); 
	}
	readingSem.give();
	return len;
}