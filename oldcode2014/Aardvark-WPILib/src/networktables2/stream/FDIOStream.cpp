/*
* FDIOStream.cpp
*
*  Created on: Sep 27, 2012
*      Author: Mitchell Wills
*/

#include "networktables2/stream/FDIOStream.h"
#include "networktables2/util/IOException.h"
#include "networktables2/util/EOFException.h"

#include <errno.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <iostream>
#include <sys/types.h>
#include <sys/stat.h>
#include <OSAL/OSAL.h>

#if USE_WINAPI
#include <Windows.h>
#elif USE_POSIX
#include <sys/socket.h>
#endif

FDIOStream::FDIOStream(SOCKET _fd){
	f = _fd;
}

FDIOStream::~FDIOStream(){
	close();
}

int FDIOStream::read(void* ptr, int numbytes){
	if(numbytes==0)
		return 0;
	char* bufferPointer = (char*)ptr;
	int totalRead = 0;

	struct timeval timeout;
	//fd_set fdSet;

	while (totalRead < numbytes) {
		/*FD_ZERO(&fdSet);
		FD_SET(fd, &fdSet);
		timeout.tv_sec = 1;
		timeout.tv_usec = 0;
		int select_result = select(FD_SETSIZE, &fdSet, NULL, NULL, &timeout);
		if ( select_result < 0)
		throw IOException("Select returned an error on read");*/

		int numRead = 0;
		//if (FD_ISSET(fd, &fdSet)) {
		//numRead = fread(bufferPointer,sizeof(char), numbytes-totalRead, f);
		numRead = recv(f, bufferPointer, sizeof(char) * (numbytes-totalRead), 0);

		if(numRead == 0){
			throw EOFException();
		}
		else if (numRead < 0) {
			perror("read error: ");
			fflush(stderr);
			throw IOException("Error on FDIO read");
		}
		bufferPointer += numRead;
		totalRead += numRead;
		//}
	}
	return totalRead;
}
int FDIOStream::write(const void* ptr, int numbytes){
	int numWrote = send(f, ((char*)ptr), numbytes, 0);
	//int numWrote = fwrite(ptr, 1, numbytes, f);
	if(numWrote==numbytes)
		return numWrote;
	perror("write error: ");
	fflush(stderr);
	throw IOException("Could not write all bytes to fd stream");

}
void FDIOStream::flush(){
	//if(fflush(f)==EOF)
	//	throw EOFException();
}
void FDIOStream::close(){
	//fclose(f);//ignore any errors closing
	//pclose(fd);
}

