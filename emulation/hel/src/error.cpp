#include "error.hpp"

#include <iostream>

namespace hel{
    std::string DSError::toString()const{
        std::string s = "(";
        s += "type:" + asString(type) + ", ";
        s += "error_code:" + std::to_string(error_code) + ", ";
        s += "details:" + details + ", ";
        s += "location:" + location + ", ";
        s += "call_stack:" + call_stack;
        return s;
    }

    DSError::DSError(bool is_error, int32_t ec, const char* det, const char* loc, const char* cs)noexcept{
        type = is_error ? Type::ERROR : Type::WARNING;
        error_code = ec;
        details = det;
        location = loc;
        call_stack = cs;
    }

    DSError::DSError(const DSError& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(type);
        COPY(error_code);
        COPY(details);
        COPY(location);
        COPY(call_stack);
#undef COPY
    }

    std::string asString(DSError::Type t){
        switch(t){
        case DSError::Type::WARNING:
            return "WARNING";
        case DSError::Type::ERROR:
            return "ERROR";
        default:
            throw UnhandledEnumConstantException("hel::DSError::Type");
        }
    }

    UnhandledEnumConstantException::UnhandledEnumConstantException(std::string s)noexcept:enum_type(s){}

    const char* UnhandledEnumConstantException::what()const throw(){
      return makeExceptionMessage("Synthesis exception: Unhandled enum constant for " + enum_type).c_str();
    }

    const char* UnhandledCase::what()const throw(){
      return makeExceptionMessage("Unhandled case").c_str();
    }

    UnsupportedFeature::UnsupportedFeature(std::string s)noexcept:details(s){}
    UnsupportedFeature::UnsupportedFeature()noexcept:UnsupportedFeature(""){}

    const char* UnsupportedFeature::what()const throw(){
      std::string s = makeExceptionMessage("Unsupported feature", false);
        if(!details.empty()){
            s += ": " + details;
        }
        s += "\n";
        return s.c_str();
    }


    InputConfigurationException::InputConfigurationException(std::string s)noexcept:details(s){}

    const char* InputConfigurationException::what()const throw(){
      return makeExceptionMessage("No matching input found in user code for input configured in robot model (" + details + ")").c_str();
    }

    std::string makeExceptionMessage(const std::string& message, bool new_line){
        return "Synthesis exception: " + message + (new_line ? "\n" : "");
    }

    std::string makeWarningMessage(const std::string& message, bool new_line){
        return "Synthesis warning: " + message + (new_line ? "\n" : "");
    }

    void warn(const std::string& message){
#ifdef HEL_ENABLE_WARNINGS
        std::cerr << makeWarningMessage(message);
#endif
    }

    void warnUnsupportedFeature(const std::string& message){
        warn("Unsupported feature: " + message);
    }
}
