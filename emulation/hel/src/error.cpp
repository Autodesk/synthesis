#include "error.hpp"

std::string hel::DSError::toString()const{
    std::string s = "(";
    s += "type:" + hel::to_string(type) + ", ";
    s += "error_code:" + std::to_string(error_code) + ", ";
    s += "details:" + details + ", ";
    s += "location:" + location + ", ";
    s += "call_stack:" + call_stack;
    return s;
}

hel::DSError::DSError(bool is_error, int32_t ec, const char* det, const char* loc, const char* cs)noexcept{
    type = is_error ? Type::ERROR : Type::WARNING;
    error_code = ec;
    details = det;
    location = loc;
    call_stack = cs;
}

std::string hel::to_string(hel::DSError::Type t){
    switch(t){
    case hel::DSError::Type::WARNING:
        return "WARNING";
    case hel::DSError::Type::ERROR:
        return "ERROR";
    default:
        throw UnhandledEnumConstantException("hel::DSError::Type");
    }
}

hel::UnhandledEnumConstantException::UnhandledEnumConstantException(std::string s)noexcept:enum_type(s){}

const char* hel::UnhandledEnumConstantException::what()const throw(){
    std::string s = "Synthesis exception: Unhandled enum constant for " + enum_type;
    return s.c_str();
}

const char* hel::UnhandledCase::what()const throw(){
    return "Synthesis exception: Unhandled case";
}

hel::UnsupportedFeature::UnsupportedFeature(std::string s)noexcept:details(s){}
hel::UnsupportedFeature::UnsupportedFeature()noexcept:UnsupportedFeature(""){}

const char* hel::UnsupportedFeature::what()const throw(){
    std::string s = "Synthesis exception: Feature unsupported by Synthesis";
    if(details.size() > 0){
        s += ": " + details;
    }
    s += "\n";
    return s.c_str();
}


hel::InputConfigurationException::InputConfigurationException(std::string s)noexcept:details(s){}

const char* hel::InputConfigurationException::what()const throw(){
    std::string s = "Synthesis exception: No matching input found in user code for input configured in robot model (" + details + ")\n";
    return s.c_str();
}
