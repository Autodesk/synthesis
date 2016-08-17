#include "HAL\SPI.h"
/*! @file spi.cpp
 *  minimal implmentation of spi protocol.
 * 
 * the current implementation simply returns 0 or false or 
 * nothing depending on function header.
 * read functions do not change the buffers.
 */
extern "C" {

	void HAL_InitializeSPI(uint8_t port, int32_t *status){
	}
	int32_t HAL_TransactionSPI(uint8_t port, uint8_t *dataToSend, uint8_t *dataReceived, uint8_t size){
		return 0; 
	}
	int32_t HAL_WriteSPI(uint8_t port, uint8_t* dataToSend, uint8_t sendSize)
	{ 
		return 0; 
	}
	int32_t  HAL_ReadSPI(uint8_t port, uint8_t *buffer, uint8_t count){
		return 0; 
	}
	void  HAL_ReadSPI(uint8_t port){
	}
	void HAL_CloseSPI(uint8_t port, uint32_t speed){
	}
	void HAL_SetSPISpeed(uint8_t port, int msb_first, int sample_on_trailing, int clk_idle_high){
	}
	void spiSetChipSelectActiveHigh(uint8_t port, int32_t *status){
       	}
	void spiSetChipSelectActiveLow(uint8_t port, int32_t *status){
      	}
	int32_t spiGetHandle(uint8_t port){ 
		return 0; 
	}
	void spiSetHandle(uint8_t port, int32_t handle){
	}

	void spiInitAccumulator(uint8_t port, uint32_t period, uint32_t cmd,
		uint8_t xfer_size, uint32_t valid_mask,
		uint32_t valid_value, uint8_t data_shift,
		uint8_t data_size, bool is_signed, bool big_endian,
		int32_t *status){
	}
	void spiFreeAccumulator(uint8_t port, int32_t *status){
	}
	void spiResetAccumulator(uint8_t port, int32_t *status){
	}
	void spiSetAccumulatorCenter(uint8_t port, int32_t center,
	int32_t *status){
	}
	void spiSetAccumulatorDeadband(uint8_t port, int32_t deadband,
	int32_t *status){
	}
	int32_t spiGetAccumulatorLastValue(uint8_t port, int32_t *status){ 
		return 0; 
	}
	int64_t spiGetAccumulatorValue(uint8_t port, int32_t *status){ 
		return 0; 
	}
	uint32_t spiGetAccumulatorCount(uint8_t port, int32_t *status) { 
		return 0; 
	}
	double spiGetAccumulatorAverage(uint8_t port, int32_t *status){ 
		return 0.0; 
	}
	void spiGetAccumulatorOutput(uint8_t port, int64_t *value, uint32_t *count,
		int32_t *status){
	}

}// extern "C" 
