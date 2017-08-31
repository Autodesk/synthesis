// this stuff is used internally to read driver station packets, but it isn't
// exposed through the HAL

#ifndef __FRC_DRIVER_STATION_H
#define __FRC_DRIVER_STATION_H

#include <stdint.h>

#include "OSAL/Synchronized.h"
#include "OSAL/WaitSemaphore.h"
#include "OSAL/OSAL.h"

union RobotControlByte {
	uint8_t control;
	struct {
#if __LITTLE_ENDIAN
		uint8_t checkVersions : 1;
		uint8_t test : 1;
		uint8_t resync : 1;
		uint8_t fmsAttached : 1;
		uint8_t autonomous:1;
		uint8_t enabled : 1;
		uint8_t notEStop :1;
		uint8_t reset :1;
#elif __BIG_ENDIAN
		uint8_t reset :1;
		uint8_t notEStop :1;
		uint8_t enabled : 1;
		uint8_t autonomous:1;
		uint8_t fmsAttached : 1;
		uint8_t resync : 1;
		uint8_t test : 1;
		uint8_t checkVersions : 1;
#endif
	};
};

struct FRCRobotControl {
	RobotControlByte control;
	uint8_t batteryVolts;
	uint8_t batteryMilliVolts;
	uint8_t digitalOutputs;
	uint8_t unknown1[4];
	uint16_t teamID;		// This is big endian
	uint8_t macAddress[6];
	union {
		uint8_t version[8];
		struct {
			uint8_t year[2];
			uint8_t day[2];
			uint8_t month[2];
			uint8_t revision[2];
		};
	};
	uint8_t unknown2[6];
	uint16_t packetIndex;		// This is big endian
};

typedef struct
{
	uint8_t size;
	uint8_t id;
	uint8_t *data;
} DynamicControlData;

/*
0		|	Upper byte for packet identification
1		|	Lower byte for packet identification
2		|	Unknown: 01
3		|	State	|	0 = Disabled Teleop, 1 = Disabled Test, 2 = Disabled Autonomous, 4 = Enabled Teleop, 5 = Enabled Test, 6 = Enabled Autonomous,
4		|	Reboot? 18 = No reboot, 1C = reboot
5		|	Team Station	|	Red 1 = 00; Red 2 = 00; Red 3 = 00; Blue 1 = 00; Blue 2 = 00; Blue 3
6		|	sizeof joystick: 0E
7		|	sizeof everything to pov: 0C
8		|	sizeof axes: 06
9		|	Axis 1
10		|	Axis 2
11		|	Axis 3
12		|	Axis 4
13		|	Axis 5
14		|	Axis 6
15		|	Unknown: 0A
16		|	Buttons extended	| Bit number corresponds with button number
17		|	Buttons	|	Bit number corresponds with button number
18		|	sizeof pov?: 01 
19 & 20	|	POV		|	FFFF = Center; 005A = Right; 0087 = Right-Bottom; 00B4 = Bottom; 00E1 = Left-Bottom; 010E = Left; 013B = Left-Top; 0000 = Top; 002D = Right-Top;
Repeats 6-20 for every other controller
*/
struct FRCCommonControlData2015 {
	uint16_t packetIndex;
	uint8_t unknown0[1];
	uint8_t state;
	uint8_t command;
	uint8_t station;
};

/*

0		|	Upper byte for packet identification
1		|	Lower byte for packet identification
2		|	No idea, but set to 0x01
3		|	Mode of the robot
4		|	0x20 shows robot code green, others appear red
5		|	left of decimal voltage, val = x
6		|	right of decimal voltage, val = x/255
7		|	padding?

*/

struct FRCRobotControl2015 {
	uint16_t packetIndex;
	uint8_t unknown0[1];
	uint8_t mode;
	uint8_t state;
	uint8_t voltage_greater;
	uint8_t voltage_lesser;
};

extern float JAG_SPEEDS[32];

struct FRCJoystick {
  uint8_t num_axes;
  int8_t axes[12];
  uint8_t num_buttons;
  uint16_t buttons;
  //TODO: pov
};

struct FRCCommonControlData {
	uint16_t packetIndex;
	union {
		uint8_t control;
		struct {
#if __LITTLE_ENDIAN
			uint8_t checkVersions : 1;
			uint8_t test : 1;
			uint8_t resync : 1;
			uint8_t fmsAttached : 1;
			uint8_t autonomous:1;
			uint8_t enabled : 1;
			uint8_t notEStop :1;
			uint8_t reset :1;
#elif __BIG_ENDIAN
			uint8_t reset :1;
			uint8_t notEStop :1;
			uint8_t enabled : 1;
			uint8_t autonomous:1;
			uint8_t fmsAttached : 1;
			uint8_t resync : 1;
			uint8_t test : 1;
			uint8_t checkVersions : 1;
#endif
		};
	};
	uint8_t dsDigitalIn;
	uint16_t teamID;

	char dsID_Alliance;
	char dsID_Position;

  uint8_t num_joysticks;
  FRCJoystick joysticks[6];

	uint64_t cRIOChecksum;
	uint32_t FPGAChecksum0;
	uint32_t FPGAChecksum1;
	uint32_t FPGAChecksum2;
	uint32_t FPGAChecksum3;

	char versionData[8];
};

#endif
