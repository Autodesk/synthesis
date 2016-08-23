#include "HAL\Power.h"
extern "C"
{
	float HAL_getVinVoltage(int32_t *status){ 
		return 12.1f; 
		*status = 0;
	}
	float HAL_getVinCurrent(int32_t *status){
		return 0.0f; 
	}
	float HAL_getUserVoltage6V(int32_t *status){
		return 0.0f; 
	}
	float HAL_getUserCurrent6V(int32_t *status) {
		return 0.0f; 
	}
	bool HAL_getUserActive6V(int32_t *status){
		return true; 
	}
	int HAL_getUserCurrentFaults6V(int32_t *status){
		return 0; 
	}
	float HAL_getUserVoltage5V(int32_t *status){
		return 0.0f; 
	}
	float HAL_getUserCurrent5V(int32_t *status){
		return 0.0f; 
	}
	bool HAL_getUserActive5V(int32_t *status){
		return false; 
	}
	int HAL_getUserCurrentFaults5V(int32_t *status){
		return 0; 
	}
	float HAL_getUserVoltage3V3(int32_t *status){
		return 0.0f; 
	}
	float HAL_getUserCurrent3V3(int32_t *status){
		return 0.0f; 
	}
	bool HAL_getUserActive3V3(int32_t *status) {
		return false; 
	}
	int HAL_getUserCurrentFaults3V3(int32_t *status) {
		return 0; 
	}
}
