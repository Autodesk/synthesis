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

hel::DSError::DSError(bool is_error, int32_t ec, const char* det, const char* loc, const char* cs){
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

hel::UnhandledEnumConstantException::UnhandledEnumConstantException(std::string s):enum_type(s){}

const char* hel::UnhandledEnumConstantException::what()const throw(){
    std::string s = "Exception: unhandled enum constant for " + enum_type;
    return s.c_str();
}

const char* hel::UnhandledCase::what()const throw(){
    return "Exception: unhandled case";
}
