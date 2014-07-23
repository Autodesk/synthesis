#include "FRCNetImpl.h"
#include <OSAL/Task.h>
#include <stdint.h>
#include "crc32.h"
#include "FRCFakeNetComm.h"

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

	// Read from DS thread
	while (enabled) {
		if (resyncSem != NULL) {
			resyncSem->take();
		}
		int len = recv(robotSocket, (char*) &buffer, sizeof(buffer), 0);
		if (len < 0) {
			printf("Read failed\n");
		}
		readingSem.take();
		memcpy(&lastDataPacket, &buffer, sizeof(FRCCommonControlData));
		// Handle endians
		lastDataPacket.packetIndex = ntohs(lastDataPacket.packetIndex);
		lastDataPacket.teamID = ntohs(lastDataPacket.teamID);
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
		ctl.packetIndex = lastDataPacket.packetIndex;
		ctl.teamID = lastDataPacket.teamID;
		ctl.packetIndex = htons(ctl.packetIndex);
		ctl.teamID = htons(ctl.packetIndex);
		memset(&sendBuffer, 0, sizeof(sendBuffer));
		memcpy(&sendBuffer, &ctl, sizeof(FRCRobotControl));
		DWORD pos = 0x21;
		for (int i = 0; i<3; i++){
			DWORD slen = 0;		// Dynamic data TODO
			memcpy(&sendBuffer[pos], &slen, sizeof(slen));
			pos += 4 + slen;
		}
		DWORD crc = crc32buf(sendBuffer, 0x400);
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

void FRCNetImpl::sendControl(FRCRobotControl ctl) {
	writingSem.take();
	this->ctl = ctl;
	writingSem.give();
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