#include "HAL\PWM.h"
#include "general.h"
#include "HAL\handles\DigitalHandleResource.h"
#include "stdio.h"
#include "PWM_exposed.h"
#include "HAL\Errors.h"
#include <mutex>
#include <array>

static hal::DigitalHandleResource<HAL_DigitalPWMHandle, HAL_DigitalPWMHandle,kPwmPins>* pwmChannels = nullptr;
std::array<unsigned short, kPwmPins> pwmChannelValues;
std::mutex lockerPWMValues;
//converts pointers to uint32_t's
static_assert(sizeof(uint32_t) <= sizeof(void *), "This file shoves uint32_ts into pointers.");


extern "C" {
	HAL_Bool HAL_CheckPWMChannel(int32_t pin) {
		void* Ppin = (void*)pin;
 		return Ppin < (&pwmChannelValues + sizeof(pwmChannelValues))
			&& Ppin >= &pwmChannelValues;
	}
	/*!
		get the pwm that was set using setpwm
		@param ret value of getPort() 
		@param errors capable of getting
	*/
	int32_t HAL_GetPWMRaw(HAL_DigitalHandle pwm_port_handle, int32_t* status)
	{
		
		if (HAL_CheckPWMChannel(pwm_port_handle)) 
		{
			std::lock_guard<std::mutex> lock (lockerPWMValues);
			unsigned short* valueptr = static_cast<unsigned short*>((void*)(int32_t)pwm_port_handle); 
			return *valueptr; //value is in pwmChannelValues array
		} 
		else 
		{
			*status = NULL_PARAMETER;
			return 0;
		}
	}
	double HAL_GetPWMSpeed(HAL_DigitalHandle pwm_port_handle, int32_t* status) {
		return HAL_GetPWMRaw(pwm_port_handle, status);
	}
	double HAL_GetPWMPosition(HAL_DigitalHandle pwm_port_handle, int32_t* status) 
	{
		return HAL_GetPWMRaw(pwm_port_handle, status);
	}
	/*!
	@brief	set port to value

	set value into in pwmChannelValues

	@param ret value of getPort()
	@param errors capable of getting
	*/
	void HAL_SetPWMRaw(HAL_DigitalHandle pwm_port_handle, int32_t value,
		int32_t* status)
	{
		if (HAL_CheckPWMChannel(pwm_port_handle))
		{
			//make sure no one is trying to read the data at the same time.
			std::lock_guard<std::mutex>lock(lockerPWMValues);
			auto* valueptr = static_cast<unsigned short*>((void*)(int32_t)pwm_port_handle);
			*valueptr = value; //value is set in pwmChannelValues array
			*status = 0;
		}
		else {
			*status = NULL_PARAMETER;
			return;
		}	
	}

	void HAL_SetPWMSpeed(HAL_DigitalHandle pwm_port_handle, double speed,
		int32_t* status) {
		HAL_SetPWMRaw(pwm_port_handle, speed, status);
	}
	void HAL_SetPWMPosition(HAL_DigitalHandle pwm_port_handle, double position,
		int32_t* status) {
		HAL_SetPWMRaw(pwm_port_handle, position, status);
	}

	void HAL_SetPWMConfig(HAL_DigitalHandle pwm_port_handle, double maxPwm,
		double deadbandMaxPwm, double centerPwm,
		double deadbandMinPwm, double minPwm, int32_t* status) {}
	void HAL_SetPWMConfigRaw(HAL_DigitalHandle pwm_port_handle, int32_t maxPwm,
		int32_t deadbandMaxPwm, int32_t centerPwm,
		int32_t deadbandMinPwm, int32_t minPwm,
		int32_t* status) {}
	void HAL_GetPWMConfigRaw(HAL_DigitalHandle pwm_port_handle, int32_t* maxPwm,
		int32_t* deadbandMaxPwm, int32_t* centerPwm,
		int32_t* deadbandMinPwm, int32_t* minPwm,
		int32_t* status) {
		*maxPwm = 0;
		*deadbandMaxPwm = 0;
		*deadbandMinPwm = 0;
		*centerPwm = 0;
		*minPwm = 0;

	}
}
/////////////////////////////////////////////////////////////////////////////////////////
//below is code for pwm_exposed.
/////////////////////////////////////////////////////////////////////////////////////////
bool intializePWM() 
{
	//18 is default value for channels.
	//hal::DigitalHandleResource::CreateResourceObject(&pwmChannels, kPwmPins);
	//the function sets pointer when it is completed.
	printf("create allocator object.\n");
	return pwmChannels != nullptr;
}
