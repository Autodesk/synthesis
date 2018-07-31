#include "util.hpp"

bool hel::stob(std::string a){
    return (bool)std::stoi(a);
}

std::string hel::to_string(bool a){
    return a ? "1" : "0";
}

void hel::copystr(const std::string& str, char* c_arr){
    str.copy(c_arr, sizeof(c_arr));
    std::size_t terminator_index = std::max(std::min((signed int)(str.size()) - 1, (signed int)(sizeof(c_arr)) - 1), 0);
    c_arr[terminator_index] = '\0';
}


bool hel::compareBits(uint32_t a, uint32_t b, uint32_t comparison_mask){
    unsigned msb = std::max(findMostSignificantBit(a), findMostSignificantBit(b));
    for(unsigned i = 0 ; i < msb; i++){
        if(checkBitHigh(comparison_mask,i)){
            if(checkBitHigh(a,i) != checkBitHigh(b,i)){
                return false;
            }
        }
    }
    return true;
}
