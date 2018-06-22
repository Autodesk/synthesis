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

using namespace std;

using HALType = std::variant<int, unsigned int, int*, unsigned long int, double, char*, bool, HAL_EncoderEncodingType, long, HAL_RuntimeType, const char *, hal::HAL_HandleEnum>;
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