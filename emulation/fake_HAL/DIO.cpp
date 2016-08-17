#include "HAL\DIO.h"
static bool code[101];
extern "C" {
	HAL_DigitalHandle HAL_InitializeDIOPort(HAL_PortHandle port_handle,
		HAL_Bool input, int32_t* status){
		return false;

	}
}