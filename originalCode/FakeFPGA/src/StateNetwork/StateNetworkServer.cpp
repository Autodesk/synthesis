#include "StateNetwork/StateNetworkServer.h"
#include <OSAL/OSAL.h>
#include <stdio.h>
#if USE_WINAPI
#include <Windows.h>
#elif USE_POSIX
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdlib.h>
#include <arpa/inet.h>
#endif

#include <string.h>

StateNetworkServer::StateNetworkServer(void) {
	udpSocket = 0;
	udpRecvSocket = 0;
}

StateNetworkServer::~StateNetworkServer(void) {
}

void StateNetworkServer::Open() {
#if USE_WINAPI
	WSADATA wsa;
	WSAStartup(MAKEWORD(2,2),&wsa);		// Start the winsock API
#endif

	udpSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);// Create a socket to yell at Unity
	if (udpSocket < 0) {
		fprintf(stderr, "Could not create socket!\n");
		exit(2);
	}

	// Who cares about data!  We don't need to listen!.... but we do
	udpRecvSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);	// Create a socket to get scolded by unity
	if (udpRecvSocket < 0) {
		fprintf(stderr, "Count not create receive socket.\n");
		exit(3);
	}

	struct sockaddr_in server;		// Setup socket address
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = INADDR_ANY;
	server.sin_port = htons(RECV_PORT);

	if (bind(udpRecvSocket, (struct sockaddr *) &server, sizeof(server))
		== SOCKET_ERROR) { // Attach socket to address
			fprintf(stderr, "Could not bind receive socket!\n");
			exit(2);
	}
}

void StateNetworkServer::Close() {
	closesocket(udpSocket);
	closesocket(udpRecvSocket);
#if USE_WINAPI
	WSACleanup();
#endif
}

void StateNetworkServer::SendStatePacket(OutputStatePacket pack) {
	struct sockaddr_in server;	// Send to localhost
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
	server.sin_port = htons(PORT);
	sendto(udpSocket, (char*) &pack, sizeof(pack), 0, (sockaddr*) &server,
		sizeof(server));	// Send it
}

bool StateNetworkServer::ReceiveStatePacket(InputStatePacket *pack) {
	ULONG available;
	fd_set set;
	FD_ZERO(&set);
	FD_SET(udpRecvSocket, &set);
	struct timeval timeout;
	timeout.tv_sec = timeout.tv_usec = 0;
	bool flag = false;
	while (select(1, &set, NULL, NULL, &timeout)) {	// Only read if there is data available
		recv(udpRecvSocket, (char*) pack, sizeof(InputStatePacket), 0);
		flag = true;
	}
	return flag;
}