#include "StateNetworkServer.h"
#include "OSAL/OSAL.h"
#include <stdio.h>
#include <thread>
#include <chrono>
#include <cmath>
#include <string.h>
#if USE_WINAPI
#include <Windows.h>
#elif USE_POSIX
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdlib.h>
#include <arpa/inet.h>
#endif

void StateNetworkServer::Open() {
#if USE_WINAPI
	// Start the winsock API
	WSADATA wsa;
	WSAStartup(MAKEWORD(2,2),&wsa);
#endif
	
	// Create a socket to send data to Unity
	udpSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (udpSocket < 0) {
		fprintf(stderr, "Could not create send socket!\n");
		exit(2);
	}

	// Create a socket to recieve data from Unity
	udpRecvSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (udpRecvSocket < 0) {
		fprintf(stderr, "Count not create receive socket.\n");
		exit(3);
	}

	// Set up socket address
	struct sockaddr_in server;
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = INADDR_ANY;
	server.sin_port = htons(RECV_PORT);

	// Attach socket to address
	if (bind(udpRecvSocket, (struct sockaddr *) &server, sizeof(server))
		== SOCKET_ERROR) {
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
	// Send to localhost
	struct sockaddr_in server;
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
	server.sin_port = htons(PORT);
	// Send it
	sendto(udpSocket, (char*) &pack, sizeof(pack), 0, (sockaddr*) &server,
		sizeof(server));
}

bool StateNetworkServer::ReceiveStatePacket(InputStatePacket *pack) {
	uint64_t available;
	fd_set set;
	FD_ZERO(&set);
	FD_SET(udpRecvSocket, &set);
	struct timeval timeout;
	timeout.tv_sec = timeout.tv_usec = 0;
	bool flag = false;
	// Only read if there is data available
	while (select(1, &set, NULL, NULL, &timeout)) {
		recv(udpRecvSocket, (char*) pack, sizeof(InputStatePacket), 0);
		flag = true;
	}
	return flag;
}

StateNetworkServer* StateNetworkServer::HAL_GetStateNetworkServerInstance() {

	static StateNetworkServer* SNSInstance = new StateNetworkServer();
	return SNSInstance;

}

float pwmValues[10];

void StateNetworkThread() {
  StateNetworkServer stateNetwork;
  stateNetwork.Open();

  OutputStatePacket packet;
  while (true) {
    std::copy(pwmValues, pwmValues + 10, packet.dio[0].pwmValues);
    stateNetwork.SendStatePacket(packet);
    std::this_thread::sleep_for(std::chrono::milliseconds(5));
  }
}

bool threadStarted = false;
void StartUnityThread() {
  if (threadStarted == true) {
    return;
  }
  std::thread(StateNetworkThread).detach();
  threadStarted = true;
}
