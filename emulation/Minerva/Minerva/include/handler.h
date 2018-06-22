#include <vector>
#include <map>
#include <variant>
#include <functional>
#include <string>
#include <iostream>
#include <type_traits>

#include <HAL/Encoder.h>
#include <HAL/HAL.h>
#include <HAL/handles/HandlesInternal.h>

#include "function_signature.h"
#include "channel.h"

using namespace std;

using HALType = std::variant<int, unsigned int, int*, unsigned long int, double, char*, bool, HAL_EncoderEncodingType, long, HAL_RuntimeType, const char *, hal::HAL_HandleEnum, HAL_AllianceStationID>;
using StatusFrame = map<string, HALType>;

unsigned constexpr hasher(char const *input) {
    return *input ?
        static_cast<unsigned int>(*input) + 33 * hasher(input + 1) :
        5381;
}

HALType convertType(string type, string value);
void callFunc(string function_name, vector<minerva::FunctionSignature::ParameterValueInfo> params);
template<typename T>
void callFunc(string function_name, vector<minerva::FunctionSignature::ParameterValueInfo> params, minerva::Channel<T> &chan);


StatusFrame __current_status_frame;

HALType convertType(string type, string value) {
    
    switch(hasher(type.c_str())) {
        case(hasher("bool")):
            return value == "true"? true: false;
        case(hasher("char *")):
            return value.c_str();
        case(hasher("const char *")):
            return value.c_str();
        case(hasher("double")):
            return stod(value);
        case(hasher("HAL_AllianceStationID")):
            return stoi(value);
        case(hasher("HAL_AnalogInputHandle")):
            return stoi(value);        
        case(hasher("HAL_AnalogOutputHandle")):
            return stoi(value);
        case(hasher("HAL_Bool")):
            return stoi(value);
        case(hasher("HAL_CompressorHandle")):
            return stoi(value);
        case(hasher("HAL_CounterHandle")):
            return stoi(value);
        case(hasher("HAL_DigitalHandle")):
            return stoi(value);
        case(hasher("HAL_DigitalPWMHandle")):
            return stoi(value);
        case(hasher("HAL_EncoderEncodingType")):
            return stoi(value);
        case(hasher("HAL_EncoderHandle")):
            return stoi(value);
        case(hasher("HAL_GyroHandle")):
            return stoi(value);
        case(hasher("HAL_InterruptHandle")):
            return stoi(value);
        case(hasher("HAL_NotifierHandle")):
            return stoi(value);
        case(hasher("HAL_PortHandle")):
            return stoi(value);
        case(hasher("HAL_RelayHandle")):
            return stoi(value);
        case(hasher("HAL_RuntimeType")):
            return stoi(value);
        case(hasher("HAL_SolenoidHandle")):
            return stoi(value);
        case(hasher("int32_t")):
            return stoi(value);
        case(hasher("int64_t")):
            return stol(value);
        case(hasher("uint64_t")):
            return (unsigned long) stol(value);
        default:
            return value.c_str();
    }
}

void printType(HALType ht) {
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
            std::cout << typeid(T).name();
    }, ht);

}

void callFunc(const char * function_name, vector<minerva::FunctionSignature::ParameterValueInfo> params) {
    switch(hasher(function_name)) {
        case(hasher("HAL_SetRelay")):
            //__current_status_frame["Relay " + 1] = convertType(params[1].type, params[1].value);
            return;
    }
}

template<typename T>
void callFunc(const char *function_name, vector<minerva::FunctionSignature::ParameterValueInfo> params, minerva::Channel<T> &chan) {
    printType(convertType(params[1].type, params[1].value));

    switch(hasher(function_name)) {
        case(hasher("HAL_GetRelay")):
            //chan.put(get<T>(__current_status_frame["Relay " + 1]));
            return;
        default:
            throw std::exception();
    }
}