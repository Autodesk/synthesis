#define _GLIBCXX_USE_CXX11_ABI 0
#include "FunctionSignature.h"
#include <vector>
#include <map>
#include "Channel.h"
#include <variant>
#include <functional>
#include <string>
#include <HAL/Encoder.h>
#include <HAL/HAL.h>
#include <HAL/handles/HandlesInternal.h>
#include "Handler.h"

using namespace std;

using StatusFrame = map<string, HALType>;

StatusFrame __current_status_frame;

HALType convertType(string type, string value) {
    hash<string> hash;
    switch(hash(type)) {
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

void callFunc(string function_name, vector<minerva::FunctionSignature::ParameterValueInfo> params) {
    hash<string> hash;
    switch(hash(function_name)) {
        case(hasher("HAL_SetRelay")):
            __current_status_frame["Relay " + stoi(params[0].value)] = convertType(params[0].type, params[0].value);
            return;
    }
}

template<typename T>
void callFunc(string function_name, vector<minerva::FunctionSignature::ParameterValueInfo> params, minerva::Channel<T> &chan) {
    hash<string> hash;
    switch(hash(function_name)) {
        case(hasher("HAL_GetRelay")):
        chan.put(__current_status_frame["Relay " + stoi(params[0].value)]);
    }
}