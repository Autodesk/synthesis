#pragma once

#include <iostream>
#include <fstream>

#include <string>
#include <array>


constexpr const auto NUM_CPU = 1;

struct CPU {

    CPU() : input("/proc/stat") {};
    CPU(const CPU& other) = delete;
    CPU(CPU&& other) = delete;

    uint64_t sample_cpu() const noexcept {
        
    }

   uint64_t sample(uint8_t core_num) const noexcept {
        
    }

    std::array<uint64_t, NUM_CPU>& sample_all() noexcept {
        auto size = input.tellg();
        std::string str(128, '\0');
        while () {}
    }

private:
    std::ifstream input; // Reads the /proc/stat info

    std::array<uint64_t, NUM_CPU> core_load{0}; 
};

#ifdef TEST

int main() {
    
}

#endif
