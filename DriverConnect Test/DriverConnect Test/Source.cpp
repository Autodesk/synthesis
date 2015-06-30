#define USE_WINAPI 1

#include <iostream>
#include <Windows.h>
#include <stdint.h>

// To DS
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

int main() {
#if USE_WINAPI
	WSADATA wsa;
	WSAStartup(MAKEWORD(2,2),&wsa);		// Hope and pray that this works.
#endif

	struct sockaddr_in robotAddress;
	struct sockaddr_in dsAddress;
	SOCKET robotSocket;
	SOCKET dsSocket;

	uint32_t network = (10 << 24) | (((9999 / 100) & 0xFF) << 16) | ((9999 % 100) << 8) | 0;

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
		fprintf(stderr, "Could not bind socket ROBOT!  Did you configure your loopback adapters? %i\n", WSAGetLastError());
		return 2;
	}

	dsSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (dsSocket < 0) {
		fprintf(stderr, "Could not create socket DS!  Did you configure your loopback adapters?\n");
		return 2;
	}

	BYTE lpacket[0x400];
	memset(&lpacket[0], 0x0, sizeof(lpacket));

	lpacket[0] = 0x00;  // packet number lesser byte
	lpacket[1] = 0x00;  // packet number greater byte
	lpacket[2] = 0x00;  // not a clue
	lpacket[3] = 0x20; // various states of the robot (teleop enabled/disabled, voltage burnout, etc)
	lpacket[4] = 0x20; // 0x20 shows robot code green, all else appears to do nothing
	lpacket[5] =   12; // left of decimal voltage, = x
	lpacket[6] = 0x25; // right of decimal voltage, = x/255
	*((uint32_t*)&lpacket[7]) = network | 2; // to lpacket[11]
	lpacket[12] = 15;
	lpacket[13] = 40;

	while (true) {
		(*((unsigned short*)&lpacket[0]))++; // make communications light show up, first two bytes increment

		const int num = 2;

		//memset(&lpacket[7], lpacket[12]+1, 12);
		//if (((*((unsigned short*)&lpacket[0])) % 6) == 5) lpacket[num]++;
		
		//std::cout << "\r" << "                 "; // clear line
		//std::cout << "\r" << (*((unsigned short*)&lpacket[3]));
		//std::cout.flush();
		sendto(dsSocket, (const char*)lpacket, 0x400, 0, (const sockaddr*)&dsAddress, sizeof(dsAddress));
		Sleep(500);
	}

	closesocket(dsSocket);
	closesocket(robotSocket);
#if USE_WINAPI
	WSACleanup();
#endif

	std::cout << "Press enter to exit." << std::endl;
	std::cin.ignore(); // wait
}