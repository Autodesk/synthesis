#include "StateNetworkServer.h"
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
	struct sockaddr_in server;		// Setup socket address
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = INADDR_ANY;
	server.sin_port = htons(PORT);

	if (bind(udpSocket, (struct sockaddr *) &server, sizeof(server))
			== SOCKET_ERROR) { // Attach socket to address
		fprintf(stderr, "Could not bind socket!\n");
		exit(2);
	}

	// Who cares about data!  We don't need to listen!
}

void StateNetworkServer::Close() {
	closesocket(udpSocket);
#if USE_WINAPI
	WSACleanup();
#endif
}

void StateNetworkServer::SendStatePacket(StatePacket pack) {
	struct sockaddr_in server;	// Send to localhost
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = inet_addr("127.0.0.1");
	server.sin_port = htons(PORT);
	sendto(udpSocket, (char*) &pack, sizeof(pack), 0, (sockaddr*) &server,
			sizeof(server));	// Send it
}
