#include <vector>
#include <map>
#include <functional>
#include <string>
#include <iostream>
#include <type_traits>

#include "types.h"
#include "function_signature.h"
#include "channel.h"
#include "constants.h"
#include "handle.h"

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
				return;//TODO
			}
		case hasher("HAL_SetPWMDisabled"):
			{
				HAL_SetPWMRaw(std::get<int>(params[0].value),minerva::constants::HAL::kPwmDisabled,std::get<int*>(params[1].value));
				return;
			}
		case hasher("HAL_SetPWMElimintateDeadband"):
			{
				return;//TODO
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
				int32_t port_handle = std::get<int32_t>(params[0].value);
				double pos = std::get<double>(params[1].value);
				if(pos < 0.0){
					pos = 0.0;
				} else if(pos > 1.0){
					pos = 1.0;
				}
				int32_t raw_value = 0;//TODO math
				HAL_SetPWMRaw(port_handle,raw_value,std::get<int32_t*>(params[2].value));
				return;
			}
		case hasher("HAL_FreePWMPort"):
			{
				return;//TODO
			}
		case hasher("HAL_SetPWMSpeed"):
			{
				int32_t port_handle = std::get<int32_t>(params[0].value);
				double speed = std::get<double>(params[1].value);
				if(speed < -1.0){
					speed = -1.0;
				} else if(speed > 1.0){
					speed = 1.0;
				} else if(!std::isfinite(speed)){
					speed = 0.0;
				}
				int32_t raw_value = 0;//TODO math
				HAL_SetPWMRaw(port_handle,raw_value,std::get<int32_t*>(params[2].value));
				return;
			}
		case hasher("HAL_SetPWMPeriodicScale"):
			{
				minerva::Handle handle;
				handle.packed = {std::get<int>(params[0].value)};
				if(handle.unpacked.channel < minerva::constants::NI_FPGA::PWM::kNumPeriodScaleHdrElements){
					//TODO
				} else {
					throw "MXP Unsupported";
				}
				return;
			}
		case hasher("HAL_LatchPWMZero"):
			{
				return;//TODO
			}
		case hasher("HAL_SetPWMConfig"):
			{
				return;//TODO
			}
		case hasher("HAL_SetPWMConfigRaw"):
			{
				return;//TODO
			}
		case hasher("HAL_GetPWMConfigRaw"):
			{
				return;//TODO
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
				return;//TODO
			}
		case hasher("HAL_GetErrorMessage"):
			{
				const char* message = [&]{
					switch(std::get<int>(params[0].value)){
						case 0:
							return "";
					}
					return "";//TODO error handling
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
				minerva::Handle handle;
				handle.packed = {std::get<int>(params[0].value)};
				if(handle.unpacked.channel < minerva::constants::HAL::kNumPWMHeaders){
					std::string array_index = "PWM ";
					array_index += std::to_string(handle.unpacked.channel);
					chan.put(std::get<T>(__current_status_frame[array_index]));
				} else {
					throw "MXP Unsupported";
				}
				return;
			}
		case hasher("HAL_GetPWMPosition"):
			{
				int32_t port_handle = {std::get<int>(params[0].value)};
				
				int32_t value = HAL_GetPWMRaw(port_handle,std::get<int32_t*>(params[1].value));

				channelReturn(chan,(double)value);//TODO math
				return;
			}
		case hasher("HAL_GetPWMSpeed"):
			{
				minerva::Handle handle;
				handle.packed = {std::get<int>(params[0].value)};
				
				int32_t value = HAL_GetPWMRaw(handle.packed,std::get<int32_t*>(params[1].value));

				channelReturn(chan,(double)value);//TODO math
				
				return;
			}
		case hasher("HAL_InitializeRelayPort"):
			{
				return;//TODO
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

