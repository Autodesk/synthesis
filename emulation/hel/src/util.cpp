#include "util.hpp"

bool hel::stob(std::string a){
    return (bool)std::stoi(a);
}

std::string hel::to_string(bool a){
    return a ? "1" : "0";
}
