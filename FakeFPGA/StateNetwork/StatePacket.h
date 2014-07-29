#ifndef __STATE_PACKET_H
#define __STATE_PACKET_H

#include <stdint.h>

typedef float float32_t;

typedef struct {
	struct {
		uint32_t relayForward;
		uint32_t relayReverse;
		uint32_t digitalOutput;
		float32_t pwmValues[10];
	} dio[2];
	struct {
		uint8_t state;
	} solenoid[1];
} OutputStatePacket;

typedef struct {
	struct {
		uint32_t digitalInput;
	} dio[2];
	struct {
		int32_t value;
	} encoder[4];
	struct {
		int32_t analogValues[8];
	} ai[1];
	struct {
		int32_t value;
	} counter[8];
} InputStatePacket;

#endif