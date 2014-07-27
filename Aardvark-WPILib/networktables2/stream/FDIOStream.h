/*
 * FDIOStream.h
 *
 *  Created on: Sep 27, 2012
 *      Author: Mitchell Wills
 */

#ifndef FDIOSTREAM_H_
#define FDIOSTREAM_H_



class FDIOStream;


#include "networktables2/stream/IOStream.h"
#include <stdio.h>
#include "OSAL/OSAL.h"
#if USE_WINAPI
#include <winsock.h>
#elif USE_POSIX
#endif



class FDIOStream : public IOStream{
private:
	SOCKET f;
public:
	FDIOStream(SOCKET fd);
	virtual ~FDIOStream();
	int read(void* ptr, int numbytes);
	int write(const void* ptr, int numbytes);
	void flush();
	void close();
};



#endif /* FDIOSTREAM_H_ */
