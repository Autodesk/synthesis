#include "HAL/Digital.hpp"
extern "C" {
void i2CInitialize(uint8_t port, int32_t* status){}

int32_t i2CTransaction(uint8_t port, uint8_t deviceAddress, uint8_t* dataToSend,
                       uint8_t sendSize, uint8_t* dataReceived,
                       uint8_t receiveSize){
	//call i2c read and i2c write for transactions.
}
int32_t i2CWrite(uint8_t port, uint8_t deviceAddress, uint8_t* dataToSend,
                 uint8_t sendSize){
// write are always successful because there is no where for the data to go
}


int32_t i2CRead(uint8_t port, uint8_t deviceAddress, uint8_t* buffer,
                uint8_t count)
{
// rewrite buffer with 0's
}
	void i2CClose(uint8_t port){
		//not implemented because 
		//we are not emulating i2c protocol
	}
}
