#include "HAL\HAL.h"
#include "general.h"
#include "stdio.h"
#include "StateNetworkServer.h"
#include "custom_headers\PWM_exposed.h"
#include <algorithm>
#include <iterator>
#include <chrono>
#include "emulator.h"

extern "C" {

	HAL_PortHandle HAL_GetPort(int32_t pin)
	{
		return HAL_GetPortWithModule(1,pin);//pwm ports
	}

	HAL_PortHandle HAL_GetPortWithModule(int32_t module, int32_t pin)
	{
		if (module == 1)
		{
			return (HAL_PortHandle)&pwmChannelValues[pin-1];//array 0 index based and pins are 1 based
		}
		return (HAL_PortHandle)nullptr;//no module to be used.
	}

	void freePort(void* digital_port_pointer) {return;} //nothing allocated on heap so nothing to free

	const char* HAL_GetErrorMessage(int32_t code) { return "Error"; }
	int32_t HAL_GetFPGAVersion(int32_t* status) { return 0; }
	int64_t HAL_GetFPGARevision(int32_t* status) { return 0; }
	uint64_t HAL_GetFPGATime(int32_t* status) {return 0;}
	HAL_Bool HAL_GetFPGAButton(int32_t* status) { return false; }
	HAL_Bool HAL_GetSystemActive(int32_t* status) { return true; }
	HAL_Bool HAL_GetBrownedOut(int32_t* status) { return false; }





	/*! 
	intialize the other modules. 
	this gives the library a chance to setup all the global variable 
	data it relies on. if not called by robot_class macro before main. 
	this library will have undefined behaivor.
	in addition to setting up global this intialize spawns up
	the emulator thread

	*/
	int HALInitialize(int mode) {
		bool retvalue; // this is return code info
					   //create thread for sending packet information.
					   // winapi only

		DWORD emulatorThreadid;
		HANDLE emulatorThread = CreateThread(
			nullptr,                   // default security attributes
			0,                      // use default stack size  
			emulator,       // thread function name
			nullptr,          // argument to thread function 
			0,                      // use default creation flags 
			&emulatorThreadid);
		// returns the thread identifier
		retvalue = (emulatorThread != nullptr);
		retvalue &= intializePWM();
		return 0; // return successfully
	}

}