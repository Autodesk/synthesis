/** 
 * \file i2c.cpp
 *  minimal implmentation of i2c protocol.
 * the current implementation simply returns 0 or false or 
 * nothing depending on function header.
 * read functions do not change the buffers params
 */
#include "HAL/I2C.h"

extern "C" {
void HAL_InitializeI2C(uint8_t port, int32_t* status){}

int32_t HAL_TransactionI2C(uint8_t port, uint8_t deviceAddress, uint8_t* dataToSend,
		uint8_t sendSize, uint8_t* dataReceived,
		uint8_t receiveSize){
		//call i2c read and i2c write for transactions.
		return 0;
	}
	int32_t HAL_WriteI2C(uint8_t port, uint8_t deviceAddress, uint8_t* dataToSend,
	 uint8_t sendSize){
	// write are always successful because there is no where for the data to go
		return 0;
	}


	int32_t HAL_ReadI2C(uint8_t port, uint8_t deviceAddress, uint8_t* buffer,
		uint8_t count){
	// rewrite buffer with 0's
	return 0;
	}
	//!not implemented because 
	//!we are not emulating i2c protocol
	//!yet
	void HAL_CloseI2C(uint8_t port){
		
	}
}
