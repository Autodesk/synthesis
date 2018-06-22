#include <vector>
#include <map>
#include <functional>
#include <string>
#include <iostream>
#include <type_traits>

#include "types.h"
#include "function_signature.h"
#include "channel.h"

unsigned constexpr hasher(char const *input) {
    return *input ?
        static_cast<unsigned int>(*input) + 33 * hasher(input + 1) :
        5381;
}

void callFunc(std::string function_name, std::vector<minerva::FunctionSignature::ParameterValueInfo> params);
template<typename T>
void callFunc(std::string function_name, std::vector<minerva::FunctionSignature::ParameterValueInfo> params, minerva::Channel<T> &chan);


minerva::StatusFrame __current_status_frame;

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
        case(hasher("HAL_SetRelay")):
            std::string array_index = "Relay ";
            array_index += std::get<int>(params[0].value);
            __current_status_frame[array_index] = params[1].value;
            return;
    }
}

template<typename T>
void callFunc(const char *function_name, std::vector<minerva::FunctionSignature::ParameterValueInfo> params, minerva::Channel<T> &chan) {
    switch(hasher(function_name)) {
        case(hasher("HAL_GetRelay")):
            std::string array_index = "Relay ";
            array_index += std::get<HAL_Bool>(params[0].value);
            chan.put(std::get<T>(__current_status_frame[array_index]));
            return;
    }
}
