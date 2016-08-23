#include "FRCNetComm/FRCNetImpl.h"
#include <OSAL/OSAL.h>
#include <OSAL/Task.h>
#include <stdint.h>
#include "FRCNetComm/crc32.h"
#include "FRCNetComm/FRCFakeNetComm.h"
#include <string.h> // For memset, memcpy
#include <stdlib.h>
#include <emulator.h> // For TEAM_ID

extern "C" {
	FRCNetImpl *frcNetInstance = NULL;
}

FRCNetImpl *GetFakeNetComm() {
	if (frcNetInstance == NULL) {
		frcNetInstance = new FRCNetImpl(TEAM_ID);
		frcNetInstance->start();
	}
	return frcNetInstance;
}

FRCNetImpl::FRCNetImpl(int teamID)
{
	enabled = false;
	task = NULL;
	this->teamID = teamID;	// TODO Super sketchy hack avoid this need to auto-resolve
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

/// Starts the task backing this network implementation
void FRCNetImpl::start() {
	if (task!=NULL){
		return;
	}
	enabled = true;
	task = new Task("FRCNetImpl", &FRCNetImpl::runThreadWrapper);
	task->Start(this);
}

/// Stops the task backing this network implementation
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
	int err = WSAStartup(MAKEWORD(2,2),&wsa);		// Hope and pray that this works.
	printf("WSAStartup() returned `%i`\n", err);

	struct sockaddr_in robotAddress;
	struct sockaddr_in dsAddress;
	SOCKET robotSocket;
	SOCKET dsSocket;

	uint32_t network = (10 << 24) | (((teamID / 100) & 0xFF) << 16) | ((teamID % 100) << 8) | 0;
	fprintf(stderr, "Team number: %i\n", teamID);
	//uint32_t network = 0xFFFFFFFF; // 127.0.0.1

	robotAddress.sin_family = AF_INET;
	robotAddress.sin_addr.s_addr = htonl(network | 2);
	robotAddress.sin_port = htons( 1110 );

	dsAddress.sin_family = AF_INET;
	dsAddress.sin_addr.s_addr = htonl(network | 5);
	dsAddress.sin_port = htons( 1150 );

	robotSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (robotSocket < 0) {
		fprintf(stderr, "Could not create socket ROBOT!\n");
		return 2;
	}

	if (bind(robotSocket, (const struct sockaddr *)&robotAddress, sizeof(robotAddress)) == SOCKET_ERROR) {
		fprintf(stderr, "Could not bind socket ROBOT!  Did you configure your loopback adapters?\n");
		return 2;
	}

	dsSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (dsSocket < 0) {
		fprintf(stderr, "Could not create socket DS!  Did you configure your loopback adapters?\n");
		return 2;
	}

	char buffer[1024];
	for (int i = 0; i < 32; i++) {
		memset(&lastDynamicControlPacket [i], 0, sizeof(lastDynamicControlPacket [i]));
	}
	bool lockedResync = false;

	// Read from the DS thread
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

		// Convert 2015 packets to 2014 (TODO find a better way)
		{
			FRCCommonControlData p2014;
			FRCCommonControlData2015 p2015;
			memcpy(&p2015, &buffer, sizeof(p2015));
			p2014.packetIndex = p2015.packetIndex++;
			char alliance;
			char position;
			switch (p2015.station) {
			case 0:
				alliance = 'R';
				position = '1';
				break;
			case 1:
				alliance = 'R';
				position = '2';
				break;
			case 2:
				alliance = 'R';
				position = '3';
				break;
			case 3:
				alliance = 'B';
				position = '1';
				break;
			case 4:
				alliance = 'B';
				position = '2';
				break;
			case 5:
				alliance = 'B';
				position = '3';
				break;
			default:
				// eek scary an unimplemented state
				// call the developer or tech support
				// TODO actually throw an error
				break;
			}

			p2014.dsID_Alliance = alliance;
			p2014.dsID_Position = position;
			memcpy(&p2014.stick0Axes[0], &p2015.axis0[0], (size_t)6);
			memcpy(&p2014.stick1Axes[0], &p2015.axis1[0], (size_t)6);
			memcpy(&p2014.stick2Axes[0], &p2015.axis2[0], (size_t)6);
			memcpy(&p2014.stick3Axes[0], &p2015.axis3[0], (size_t)6);
			p2014.stick0Buttons = p2015.buttons0;
			p2014.stick1Buttons = p2015.buttons1;
			p2014.stick2Buttons = p2015.buttons2;
			p2014.stick3Buttons = p2015.buttons3;
			p2014.enabled = p2015.state & 4 ? true : false;
			p2014.autonomous = p2015.state & 2 ? true : false;
			p2014.test = p2015.state & 1 ? true : false;
			ctl.control.enabled = p2014.enabled;
			ctl.control.autonomous = p2014.autonomous;
			ctl.control.test = p2014.test;

			memcpy(&lastDataPacket, &p2014, sizeof(p2014));
		}

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
					//delete lastDynamicControlPacket [id].data; // getting a segfault??
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

		char sendBuffer[2048];
		writingSem.take();
		memset(&sendBuffer, 0, sizeof(sendBuffer));
		//memcpy(&sendBuffer, &ctl,  sizeof(FRCRobotControl));

		// Convert 2014 packets to 2015
		{
			FRCRobotControl2015 c2015;
			c2015.packetIndex = ctl.packetIndex++;
			c2015.voltage_greater = /*ctl.batteryVolts*/12; // who cares anyways
			int oscillation = (ctl.control.enabled ? 0 : (rand() % 2)); // don't judge
			c2015.voltage_lesser = /*ctl.batteryMilliVolts*/ 0x63 - oscillation; // who cares
			c2015.mode = 0;
			c2015.mode += (ctl.control.enabled ? 4 : 0); // sets the 3rd bit to the value of the 3rd bit in ctl.control
			c2015.mode += (ctl.control.test ? 1 : 0); // sets 1st bit
			c2015.mode += (ctl.control.autonomous ? 2 : 0); // sets 2nd bit
			c2015.state = 0x30; // TODO change
			memcpy(&sendBuffer, &c2015, sizeof(c2015));

			//printf("%s\n", ctl.control.enabled ? "true" : "false");
		}

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
		writingSem.give();
		uint32_t crc = crc32buf(sendBuffer, 0x400);
		crc = htonl(crc);
		memcpy(&sendBuffer[0x3fc], &crc, sizeof(DWORD));
		sendto(dsSocket,(const char *) &sendBuffer, 0x07, 0,(const sockaddr*)&dsAddress, sizeof(dsAddress));
	}
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
		memcpy (dynamicData, lastDynamicControlPacket [type].data,std::min(len+1, maxLength));
	}
	readingSem.give();
	return len;
}
