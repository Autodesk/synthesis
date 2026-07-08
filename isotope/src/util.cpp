#include "util.h"

#include <Fusion/Components/Component.h>
#include <Fusion/Components/Occurrence.h>

#include <array>
#include <iomanip>
#include <random>
#include <sstream>
#include <string>

std::string guid_component(const adsk::core::Ptr<adsk::fusion::Component>& component) {
    if (!component) {
        return "";
    }

    return component->entityToken() + "_" + component->id();
}

std::string guid_occurrence(const adsk::core::Ptr<adsk::fusion::Occurrence>& occurrence) {
    if (!occurrence) {
        return "";
    }

    return occurrence->entityToken() + "_" + guid_component(occurrence->component());
}

std::string uuid4() {
    static thread_local std::random_device rd;
    static thread_local std::mt19937_64 gen(rd());
    static thread_local std::uniform_int_distribution<uint64_t> dis;

    std::array<uint8_t, 16> bytes;
    for (size_t i = 0; i < 16; i += 8) {
        uint64_t random_val = dis(gen);
        for (size_t j = 0; j < 8; ++j) {
            bytes[i + j] = static_cast<uint8_t>((random_val >> (j * 8)) & 0xFF);
        }
    }

    bytes[6] = (bytes[6] & 0x0F) | 0x40;
    bytes[8] = (bytes[8] & 0x3F) | 0x80;

    std::ostringstream oss;
    oss << std::hex << std::setfill('0');
    for (size_t i = 0; i < 16; ++i) {
        oss << std::setw(2) << static_cast<int>(bytes[i]);
        if (i == 3 || i == 5 || i == 7 || i == 9) {
            oss << "-";
        }
    }

    return oss.str();
}
