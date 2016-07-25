#include <stdint.h>
#include <CAN/JaguarCANDriver.h>
#include <NetworkCommunication/FRCComm.h>
#include <CAN/can_proto.h>
#include <stdio.h>

const int JAG_COUNT = 1 << 5;
const int JAG_PORT_MASK = JAG_COUNT - 1;
float JAG_SPEEDS[32];
bool ACKS[32];

float getJagSpeed(int i ) {return JAG_SPEEDS[i];}

void FRC_NetworkCommunication_JaguarCANDriver_sendMessage(uint32_t messageID, const uint8_t *data, uint8_t dataSize, int32_t *status){
	int devNum = (messageID & JAG_PORT_MASK) -1;
	if ((messageID & LM_API_VOLT_T_SET) == LM_API_VOLT_T_SET) {
		ACKS[devNum] = true;
		int16_t value = *((int16_t*)(data+2));
		value = ntohs(value);
		JAG_SPEEDS[devNum] = (float) value / 32767.0;
	}
}

void FRC_NetworkCommunication_JaguarCANDriver_receiveMessage(uint32_t *messageID, uint8_t *data, uint8_t *dataSize, uint32_t timeoutMs, int32_t *status){
	int devNum = (*messageID & JAG_PORT_MASK)-1;
	if (data==NULL){
		*status = 0;
		return;
	}
	if ((*messageID & LM_API_ACK) == LM_API_ACK) {
		if (!ACKS[devNum]) {
			printf("Ack failed\n");
		}
		ACKS[devNum] = false;
		*status = 0;
	}else if ((*messageID & LM_API_VOLT_T_SET) == LM_API_VOLT_T_SET) {
		int16_t intValue = (int16_t)(JAG_SPEEDS[devNum] * 32767.0);
		*((int16_t*)data) = htons(intValue);
		*dataSize = sizeof(int16_t);
		*status = 0;
	}else if (((*messageID) & CAN_MSGID_API_FIRMVER) == CAN_MSGID_API_FIRMVER) {
		int32_t thing = 101;
		*((int32_t*)data) = htonl(thing);
		*dataSize = sizeof(int32_t);
		*status = 0;
	}
	// I don't care
	*status = 0;
}