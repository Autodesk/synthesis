#include "util.hpp"

#include<string>

namespace hel{
    bool stob(std::string a){
        try{
            return (bool)std::stoi(a);
        } catch(...){
            throw;
        }
    }

    std::string asString(bool input){
        return input ? "1" : "0";
    }
}
