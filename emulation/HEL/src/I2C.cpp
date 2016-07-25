/** 
 * \file i2c.cpp
 *  minimal implmentation of i2c protocol.
 * the current implementation simply returns 0 or false or 
 * nothing depending on function header.
 * read functions do not change the buffers.
 */
#include "HAL/Digital.hpp"
extern "C" {

extern "C" {
void i2CInitialize(uint8_t port, int32_t* status){}

int32_t i2CTransaction(uint8_t port, uint8_t deviceAddress, uint8_t* dataToSend,
                       uint8_t sendSize, uint8_t* dataReceived,
                       uint8_t receiveSize)
{
	//call i2c read and i2c write for transactions.
	return 0;
}
int32_t i2CWrite(uint8_t port, uint8_t deviceAddress, uint8_t* dataToSend,
                 uint8_t sendSize)
{
// write are always successful because there is no where for the data to go
	return 0;
}


int32_t i2CRead(uint8_t port, uint8_t deviceAddress, uint8_t* buffer,
                uint8_t count)
{
// rewrite buffer with 0's
	return 0;
}
	void i2CClose(uint8_t port){
		//not implemented because 
		//we are not emulating i2c protocol
	}
}
