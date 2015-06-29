/*
* SocketStreamFactory.cpp
*
*  Created on: Nov 3, 2012
*      Author: Mitchell Wills
*/


#include <cstring>
#include <stdlib.h>
#include <stdio.h>
#include <sys/types.h>
#include "OSAL/OSAL.h"

#if USE_WINAPI
#include <Windows.h>
#elif USE_POSIX
#include <arpa/inet.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#endif

#include "networktables2/stream/FDIOStream.h"
#include "networktables2/stream/SocketStreamFactory.h"


SocketStreamFactory::SocketStreamFactory(const char* _host, int _port):host(_host), port(_port){}

SocketStreamFactory::~SocketStreamFactory(){}

IOStream* SocketStreamFactory::createStream(){
#ifdef _WRS_KERNEL
	//crio client not supported
	return NULL;
#else
	struct sockaddr_in serv_addr;
	struct hostent *server;

	int sockfd = socket(AF_INET, SOCK_STREAM, 0);
	if (sockfd < 0){
		//error("ERROR opening socket");
		return NULL;
	}
	server = gethostbyname(host);
	if (server == NULL) {
		//fprintf(stderr,"ERROR, no such host\n");
		return NULL;
	}
	memset(&serv_addr, 0, sizeof(serv_addr));
	serv_addr.sin_family = AF_INET;
	memcpy(&serv_addr.sin_addr.s_addr, server->h_addr, server->h_length);
	serv_addr.sin_port = htons(port);
	if (connect(sockfd,(struct sockaddr *) &serv_addr,sizeof(serv_addr)) < 0) {
		//error("ERROR connecting");
		return NULL;
	}//TODO close fd if an error occured

	//int on = 1;
	//setsockopt(sockfd, IPPROTO_TCP, TCP_NODELAY, (char *)&on, sizeof(on));

	return new FDIOStream(sockfd);
#endif
}
