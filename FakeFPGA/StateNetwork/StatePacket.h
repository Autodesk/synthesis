#ifndef __STATE_PACKET_H
#define __STATE_PACKET_H

#include <stdint.h>

typedef float float32_t;

typedef struct {
	float32_t pwmValues[8];
	float32_t canMotorValues[16];
	uint8_t solenoidValues;
	uint8_t relayValues;
} StatePacket;

#endif