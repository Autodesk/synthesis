#include <vector>
#include <map>
#include <functional>
#include <string>
#include <iostream>
#include <type_traits>

#include "HAL/handles/HandlesInternal.h"

#include "types.h"
#include "function_signature.h"
#include "channel.h"
#include "constants.h"
#include "handle.h"
#include "globals.h"

//TODO Remove after we get an arm compiler working and can replaced this by compiling with HAL library
namespace hal{
	HandleBase::HandleBase(){}
	HandleBase::~HandleBase(){}
	void HandleBase::ResetHandles(){}
	void HandleBase::ResetGlobalHandles(){}
	
	HAL_Handle createHandle(short,HAL_HandleEnum,short){
		return 0;
	}
}

unsigned constexpr hasher(char const *input) {
	return *input ?
		static_cast<unsigned int>(*input) + 33 * hasher(input + 1) :
		5381;
}

void callFunc(std::string function_name, std::vector<minerva::FunctionSignature::ParameterValueInfo> params);
template<typename T>
void callFunc(std::string function_name, std::vector<minerva::FunctionSignature::ParameterValueInfo> params, minerva::Channel<T> &chan);

minerva::StatusFrame __current_status_frame;
minerva::StatusFrame __decoded_status_frame;

void printType(minerva::HALType ht) {
	std::visit([](auto&& arg){
		using T = std::decay_t<decltype(arg)>;
		if constexpr (std::is_same_v<T, int>)
			std::cout << "int\n";
		if constexpr (std::is_same_v<T, unsigned int>)
			std::cout << "uint\n";
		if constexpr (std::is_same_v<T, int*>)
			std::cout << "int*\n";
		if constexpr (std::is_same_v<T, unsigned long int>)
			std::cout << "unsigned long int\n";
		if constexpr (std::is_same_v<T, double>)
			std::cout << "double\n";
		if constexpr (std::is_same_v<T, const char*>)
			std::cout << "char*\n";
		if constexpr (std::is_same_v<T, bool>)
			std::cout << "bool\n";
		if constexpr (std::is_same_v<T, HAL_EncoderEncodingType>)
			std::cout << "HAL_EncoderEncodingType\n";
		if constexpr (std::is_same_v<T, long>)
			std::cout << "int*\n";
		if constexpr (std::is_same_v<T, HAL_RuntimeType>)
			std::cout << "HAL_RuntimeType\n";
		if constexpr (std::is_same_v<T, hal::HAL_HandleEnum>)
			std::cout << "HAL_HandleEnum\n";
		if constexpr (std::is_same_v<T, HAL_AllianceStationID>)
			std::cout << "HAL_AllianceStationID*\n";
		if constexpr (std::is_same_v<T, HAL_Bool>)
			std::cout << "Shouldn't Happen";
		else
			throw std::exception();
	}, ht);

}

//handles all of the HAL functions that return void
void callFunc(const char * function_name, std::vector<minerva::FunctionSignature::ParameterValueInfo> params) {
	switch(hasher(function_name)) {
		case hasher("HAL_SetRelay"):
			{
				std::string array_index = "Relay ";
				array_index += std::to_string(std::get<int>(params[0].value));
				__current_status_frame[array_index] = params[1].value;
				std::cout<<"\n\""<<array_index<<"\"\n";
				return;
			}
		case hasher("HAL_FreeRelayPort"):
			{
				//TODO
				return;
			}
		case hasher("HAL_SetPWMDisabled"):
			{
				HAL_SetPWMRaw(std::get<int>(params[0].value),minerva::constants::HAL::kPwmDisabled,std::get<int*>(params[1].value));
				return;
			}
		case hasher("HAL_SetPWMElimintateDeadband"):
			{
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(std::get<HAL_DigitalHandle>(params[0].value),hal::HAL_HandleEnum::PWM);
				if(port == nullptr){
					return;
				}
				port->eliminateDeadband = std::get<HAL_Bool>(params[1].value);
				return;
			}
		case hasher("HAL_SetPWMRaw"):
			{
				minerva::Handle handle;
				handle.packed = {std::get<int>(params[0].value)};
				if(handle.unpacked.channel < minerva::constants::HAL::kNumPWMHeaders){
					std::string array_index = "PWM ";
					array_index += std::to_string(handle.unpacked.channel);
					__current_status_frame[array_index] = params[1].value;
				} else {
					throw "MXP Unsupported";
				}
				return;
			}
   		case hasher("HAL_SetPWMPosition"):
			{
				HAL_DigitalHandle port_handle = std::get<HAL_DigitalHandle>(params[0].value);
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);
				
				if(port == nullptr){
					return;
				}
				
				if(!port->configSet){
					return;
				}

				double pos = std::get<double>(params[1].value);

				if(pos < 0.0){
					pos = 0.0;
				} else if(pos > 1.0){
					pos = 1.0;
				}

				int32_t raw_value = static_cast<int32_t>(
					(pos * static_cast<double>(port->fullRangeScaleFactor())) +
					port->minPwm					
				);

				if(raw_value == minerva::constants::HAL::kPwmDisabled){
					return;
				}			
	
				HAL_SetPWMRaw(port_handle,raw_value,std::get<int32_t*>(params[2].value));

				return;
			}
		case hasher("HAL_FreePWMPort"):
			{
				HAL_DigitalHandle port_handle = std::get<HAL_DigitalHandle>(params[0].value);
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);

				if(port == nullptr){
					return;
				}
				
				minerva::digitalChannelHandles.Free(port_handle,hal::HAL_HandleEnum::PWM);

				if(port->channel > minerva::constants::NI_FPGA::PWM::kNumHdrRegisters -1){
					throw "MXP Unsupported";
				}
				return;
			}
		case hasher("HAL_SetPWMSpeed"):
			{
				HAL_DigitalHandle port_handle = std::get<HAL_DigitalHandle>(params[0].value);
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);
				
				if(port == nullptr){
					return;
				}
				
				if(!port->configSet){
					return;
				}
				
				double speed = std::get<double>(params[1].value);
				
				if(speed < -1.0){
					speed = -1.0;
				} else if(speed > 1.0){
					speed = 1.0;
				} else if(!std::isfinite(speed)){
					speed = 0.0;
				}
				
				int32_t raw_value = 0;
					
				if(speed == 0){
					raw_value = port->centerPwm;
				} else if(speed > 0.0){
					raw_value = static_cast<int32_t>(
						speed * static_cast<double>(port->positiveScaleFactor()) + static_cast<double>(port->minPositivePwm()) + 0.5
					);
				} else {
					raw_value = static_cast<int32_t>(
						speed * static_cast<double>(port->negativeScaleFactor()) + static_cast<double>(port->maxNegativePwm()) + 0.5
					);
				}

				HAL_SetPWMRaw(port_handle,raw_value,std::get<int32_t*>(params[2].value));
				
				return;
			}
		case hasher("HAL_SetPWMPeriodicScale"):
			{
				HAL_DigitalHandle port_handle = std::get<HAL_DigitalHandle>(params[0].value);
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);
				
				if(port == nullptr){
					return;
				}

				if(port->channel < minerva::constants::NI_FPGA::PWM::kNumPeriodScaleHdrElements){
					//TODO
				} else {
					throw "MXP Unsupported";
				}
				return;
			}
		case hasher("HAL_LatchPWMZero"):
			{
				HAL_DigitalHandle port_handle = std::get<HAL_DigitalHandle>(params[0].value);
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);

				if(port == nullptr){
					return;
				}
				//TODO
				return;
			}
		case hasher("HAL_SetPWMConfig"):
			{
				HAL_DigitalHandle port_handle = std::get<HAL_DigitalHandle>(params[0].value);
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);
				
				if(port == nullptr){
					return;
				}

				/*TODO
				double loopTime = HAL_GetPWMLoopTiming(status) / (kSystemClockTicksPerMicrosecond * 1e3);

				int32_t maxPwm = static_cast<int32_t>((max - kDefaultPwmCenter) / loopTime + kDefaultPwmStepsDown - 1);
				int32_t deadbandMaxPwm = static_cast<int32_t>((deadbandMax - kDefaultPwmCenter) / loopTime + kDefaultPwmStepsDown - 1);
				int32_t centerPwm = static_cast<int32_t>((center - kDefaultPwmCenter) / loopTime + kDefaultPwmStepsDown - 1);
				int32_t deadbandMinPwm = static_cast<int32_t>((deadbandMin - kDefaultPwmCenter) / loopTime + kDefaultPwmStepsDown - 1);
				int32_t minPwm = static_cast<int32_t>((min - kDefaultPwmCenter) / loopTime + kDefaultPwmStepsDown - 1);
				
				port->maxPwm = maxPwm;
				port->deadbandMaxPwm = deadbandMaxPwm;
				port->deadbandMinPwm = deadbandMinPwm;
				port->centerPwm = centerPwm;
				port->minPwm = minPwm;
				port->configSet = true;
				*/
				return;
			}
		case hasher("HAL_SetPWMConfigRaw"):
			{
				HAL_DigitalHandle port_handle = std::get<HAL_DigitalHandle>(params[0].value);
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);

				if(port == nullptr){
					return;
				}

				port->maxPwm = std::get<int32_t>(params[1].value);
				port->deadbandMaxPwm = std::get<int32_t>(params[2].value);
				port->centerPwm = std::get<int32_t>(params[3].value);
				port->deadbandMinPwm = std::get<int32_t>(params[4].value);
				port->minPwm = std::get<int32_t>(params[5].value);
				
				return;
			}
		case hasher("HAL_GetPWMConfigRaw"):
			{
				HAL_DigitalHandle port_handle = std::get<HAL_DigitalHandle>(params[0].value);
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);
				
				if(port == nullptr){
					return;
				}
			
				*std::get<int32_t*>(params[1].value) = port->maxPwm; 
				*std::get<int32_t*>(params[2].value) = port->deadbandMaxPwm;
				*std::get<int32_t*>(params[3].value) = port->centerPwm;
				*std::get<int32_t*>(params[4].value) = port->deadbandMinPwm;
				*std::get<int32_t*>(params[5].value) = port->minPwm;
				return;
			}
		case hasher("HAL_BaseInitialize"):
			{
				return;
			}
	}
}

template<typename T,typename U>
void channelReturn(minerva::Channel<T>& channel, U val){
	minerva::HALType a = val;
	channel.put(std::get<T>(a));
}

//for HAL functions which do return something
template<typename T>
void callFunc(const char *function_name, std::vector<minerva::FunctionSignature::ParameterValueInfo> params, minerva::Channel<T> &chan) {
	switch(hasher(function_name)) {
		case hasher("HAL_GetRelay"):
			{
				std::string array_index = "Relay ";
				array_index += std::to_string(std::get<HAL_Bool>(params[0].value));
				chan.put(std::get<T>(__current_status_frame[array_index]));
				std::cout<<"\n\""<<array_index<<"\"\n";
				return;
			}
		case hasher("HAL_CheckRelayChannel"):
			{
				int32_t channel = std::get<int>(params[0].value);
				channelReturn(chan,channel < minerva::constants::HAL::kNumRelayHeaders && channel >= 0);
				return;
			}
		case hasher("HAL_GetPort"):
			{
				int32_t channel = std::get<int>(params[0].value);
				if(channel < 0 || channel >= 255){
					channelReturn(chan,HAL_kInvalidHandle);
				} else {
					channelReturn(chan,channel);
				}
				return;
			}
		case hasher("HAL_InitializePWMPort"):
			{
				int16_t channel = [&]{
					minerva::Handle handle;
					handle.packed = {std::get<int>(params[0].value)};
					return handle.unpacked.channel;
				}();
				
				int32_t* status = std::get<int32_t*>(params[1].value);

				if(channel == minerva::constants::HAL::InvalidHandleIndex || channel >= minerva::constants::HAL::kNumPWMChannels){
					channelReturn(chan,HAL_kInvalidHandle);
					return;
				}
				
				uint8_t orig_channel = static_cast<uint8_t>(channel);

				if(orig_channel < minerva::constants::HAL::kNumPWMHeaders){
					channel += minerva::constants::HAL::kNumPWMChannels;
				} else {
					throw "MXP Unsupported";
				}
				
				HAL_DigitalHandle port_handle = minerva::digitalChannelHandles.Allocate(channel,hal::HAL_HandleEnum::PWM,status);

				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);

				if(port == nullptr){
					channelReturn(chan,HAL_kInvalidHandle);
					return;
				}

				port->channel = orig_channel;

				HAL_SetPWMConfig(port_handle,2.0,1.501,1.5,1.499,1.0,status);
				
				//TODO implement MXP support 

				return;
			}
		case hasher("HAL_GetErrorMessage"):
			{
				const char* message = [&]{
					switch(std::get<int>(params[0].value)){
						case 0:
							return "";
					}
					//TODO error handling
					return "";
				}();
				channelReturn(chan,message);
				return;
			}
		case hasher("HAL_Report"):
			{
				channelReturn(chan,0);//definition hidden in NI FPGA
				return;
			}	
		case hasher("HAL_GetPWMRaw"):
			{
				HAL_DigitalHandle port_handle = minerva::digitalChannelHandles.Allocate(std::get<HAL_DigitalHandle>(params[0].value),hal::HAL_HandleEnum::PWM,std::get<int32_t*>(params[1].value));
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);
		
				if(port == nullptr){
					channelReturn(chan,0);
					return;
				}
		
				if(port->channel < minerva::constants::HAL::kNumPWMHeaders){
					std::string array_index = "PWM ";
					array_index += std::to_string(port->channel);
					channelReturn(chan,__current_status_frame[array_index]);
				} else {
					throw "MXP Unsupported";
				}
				return;
			}
		case hasher("HAL_GetPWMPosition"):
			{
				HAL_DigitalHandle port_handle = minerva::digitalChannelHandles.Allocate(std::get<HAL_DigitalHandle>(params[0].value),hal::HAL_HandleEnum::PWM,std::get<int32_t*>(params[1].value));
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);
		
				if(port == nullptr){
					channelReturn(chan,0);
					return;
				}
				
				if(!port->configSet){
					channelReturn(chan,0);
					return;
				}		
				
				int32_t value = HAL_GetPWMRaw(port_handle,std::get<int32_t*>(params[1].value));

				if(value < port->minPwm){
					channelReturn(chan,0); 
					return;
				} else if(value > port->maxPwm){
					channelReturn(chan,1.0);
					return;
				} else {
					channelReturn(chan,static_cast<double>(value - port->minPwm)/ static_cast<double>(port->fullRangeScaleFactor()));
					return;
				}

				return;
			}
		case hasher("HAL_GetPWMSpeed"):
			{
				HAL_DigitalHandle port_handle = minerva::digitalChannelHandles.Allocate(std::get<HAL_DigitalHandle>(params[0].value),hal::HAL_HandleEnum::PWM,std::get<int32_t*>(params[1].value));
				std::shared_ptr<minerva::DigitalPort> port = minerva::digitalChannelHandles.Get(port_handle,hal::HAL_HandleEnum::PWM);
		
				if(port == nullptr){
					channelReturn(chan,0);
					return;
				}
				
				if(!port->configSet){
					channelReturn(chan,0);
					return;
				}		
				
				int32_t value = HAL_GetPWMRaw(port_handle,std::get<int32_t*>(params[1].value));

				if(value == minerva::constants::HAL::kPwmDisabled){
					channelReturn(chan,0.0);
					return;
				} else if(value > port->maxPwm){
					channelReturn(chan,0); 
					return;
				} else if(value < port->minPwm){
					channelReturn(chan,1.0);
					return;
				} else if(value > port->minPositivePwm()){
					channelReturn(chan,static_cast<double>(value - port->minPositivePwm()) / static_cast<double>(port->positiveScaleFactor()));
					return;
				} else if(value < port->maxNegativePwm()){
    				channelReturn(chan,static_cast<double>(value - port->maxNegativePwm()) / static_cast<double>(port->negativeScaleFactor()));
					return;
				} else {
					channelReturn(chan,0);
					return;
				}

				return;
			}
		case hasher("HAL_InitializeRelayPort"):
			{
				//TODO
				return;
			}
		case hasher("HAL_Initialize"):
			{
				HAL_BaseInitialize(0);//expects a new status to modify, but we aren't using that
				HAL_InitializeDriverStation();
				channelReturn(chan,true);//return HAL initialized successfully
				return;
			}
		case hasher("HAL_SetJoystickOutputs"):
			{
				channelReturn(chan,0);//return 0 since it expects an int return value,but WPILib does nothing with it
				//TODO
				return;
			}
		case hasher("HAL_GetNumAccumulators"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumAccumulators);
				return;
			}
		case hasher("HAL_GetNumAnalogTriggers"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumAnalogTriggers);
				return;
			}
		case hasher("HAL_GetNumAnalogInputs"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumAnalogInputs);
				return;
			}
		case hasher("HAL_GetNumAnalogOutputs"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumAnalogOutputs);
				return;
			}
		case hasher("HAL_GetNumCounters"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumCounters);
				return;
			}
		case hasher("HAL_GetNumDigitalHeaders"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumDigitalHeaders);
				return;
			}
		case hasher("HAL_GetNumPWMHeaders"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumPWMHeaders);
				return;
			}
		case hasher("HAL_GetNumDigitalChannels"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumDigitalChannels);
				return;
			}
		case hasher("HAL_GetNumPWMChannels"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumPWMChannels);
				return;
			}
		case hasher("HAL_GetNumDigitalPWMOutputs"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumDigitalPWMOutputs);
				return;
			}
		case hasher("HAL_GetNumEncoders"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumEncoders);
				return;
			}
		case hasher("HAL_GetNumInterrupts"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumInterrupts);
				return;
			}
		case hasher("HAL_GetNumRelayChannels"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumRelayChannels);
				return;
			}
		case hasher("HAL_GetNumRelayHeaders"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumRelayHeaders);
				return;
			}
		case hasher("HAL_GetNumPCMModules"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumPCMModules);
				return;
			}
		case hasher("HAL_GetNumSolenoidChannels"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumSolenoidChannels);
				return;
			}
		case hasher("HAL_GetNumPDPModules"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumPDPModules);
				return;
			}
		case hasher("HAL_GetNumPDPChannels"):
			{
				channelReturn(chan,minerva::constants::HAL::kNumPDPChannels);
				return;
			}
	}
}

