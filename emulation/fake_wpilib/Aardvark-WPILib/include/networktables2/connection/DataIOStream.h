#ifndef DATAIOSTREAM_H_
#define DATAIOSTREAM_H_

#include <stdlib.h>
#include "networktables2/stream/IOStream.h"
#include <exception>
#include <string>

#ifndef _WRS_KERNEL
#include <stdint.h>
#endif

#include <stdlib.h>
#include <memory>


class DataIOStream{
public:
	DataIOStream(IOStream* stream);
	virtual ~DataIOStream();
	void writeByte(uint8_t b);
	void write2BytesBE(uint16_t s);
	void writeString(std::string& str);
	void flush();
	
	uint8_t readByte();
	uint16_t read2BytesBE();
	std::string* readString();
	
	void close();
	void SetIOStream(IOStream* stream);

//private:
	IOStream *iostream;
};


#endif
