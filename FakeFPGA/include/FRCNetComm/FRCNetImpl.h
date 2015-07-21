#ifndef __FRC_NET_IMPL_H
#define __FRC_NET_IMPL_H

#include <OSAL/OSAL.h>
#include <OSAL/Synchronized.h>
#include <OSAL/WaitSemaphore.h>
#if USE_WINAPI
#include <Windows.h>
#elif USE_POSIX
#include <sys/socket.h>
#include <netinet/in.h>
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

	FRCNetImpl(int teamID);
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
	
	uint8_t size_joystick0;
	uint8_t size_up_to_pov0;
	uint8_t size_axes0;
	uint8_t axis0[6];
	uint8_t unknown1[1];
	uint16_t buttons0;
	uint8_t size_pov0; // unsure
	uint8_t pov0[2];
	
	uint8_t size_joystick1;
	uint8_t size_up_to_pov1;
	uint8_t size_axes1;
	uint8_t axis1[6];
	uint8_t unknown2[1];
	uint16_t buttons1;
	uint8_t size_pov1; // unsure
	uint8_t pov1[2];

	uint8_t size_joystick2;
	uint8_t size_up_to_pov2;
	uint8_t size_axes2;
	uint8_t axis2[6];
	uint8_t unknown3[1];
	uint16_t buttons2;
	uint8_t size_pov2; // unsure
	uint8_t pov2[2];
	
	uint8_t size_joystick3;
	uint8_t size_up_to_pov3;
	uint8_t size_axes3;
	uint8_t axis3[6];
	uint8_t unknown4[1];
	uint16_t buttons3;
	uint8_t size_pov3; // unsure
	uint8_t pov3[2];
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
	//uint8_t padding; // don't think this is needed
};

extern float JAG_SPEEDS[32];

#endif
