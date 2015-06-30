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

	lpacket[0] = 0x1;
	lpacket[3] = 0xFF;
	lpacket[4] = 0x30;

	BYTE packet[6] = {
		0x01, 0x00, // ping
		0x01,       // no one knows
		0x00,       // robot is disabled
		0x10,       // idle message
		0x00        // no one knows
	};

	while (true) {
		sendto(dsSocket, (const char*)lpacket, sizeof(lpacket), 0, (const sockaddr*)&dsAddress, sizeof(dsAddress));
		Sleep(50);
	}

	closesocket(dsSocket);
	closesocket(robotSocket);
#if USE_WINAPI
	WSACleanup();
#endif

	std::cout << "Press enter to exit." << std::endl;
	std::cin.ignore(); // wait
}