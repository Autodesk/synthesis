#include "roborio_manager.hpp"

namespace hel{

    std::string asString(CANDevice::Type type){
        switch(type){
        case CANDevice::Type::TALON_SRX:
            return "TALON_SRX";
        case CANDevice::Type::VICTOR_SPX:
            return "VICTOR_SPX";
        case CANDevice::Type::UNKNOWN:
            return "UNKNOWN";
        case CANDevice::Type::PCM:
            return "PCM";
        case CANDevice::Type::PDP:
            return "PDP";
        default:
            throw UnhandledEnumConstantException("hel::CANDevice::Type");
        }
    }

    CANDevice::Type s_to_can_device_type(std::string input){
        switch(hasher(input.c_str())){
        case hasher("TALON_SRX"):
            return CANDevice::Type::TALON_SRX;
        case hasher("VICTOR_SPX"):
            return CANDevice::Type::VICTOR_SPX;
        case hasher("PCM"):
            return CANDevice::Type::PCM;
        case hasher("PDP"):
            return CANDevice::Type::PDP;
        case hasher("UNKNOWN"):
            return CANDevice::Type::UNKNOWN;
        default:
            throw UnhandledCase();
        }
    }

    uint8_t CANDevice::pullDeviceID(uint32_t message_id)noexcept{
        return message_id & IDMask::DEVICE_ID;
    }

    CANDevice::Type CANDevice::pullDeviceType(uint32_t message_id)noexcept{
        if(compareBits(message_id, IDMask::VICTOR_SPX_TYPE, IDMask::DEVICE_TYPE)){
            return Type::VICTOR_SPX;
        } else if(compareBits(message_id, IDMask::TALON_SRX_TYPE, IDMask::DEVICE_TYPE)){
            return Type::TALON_SRX;
        } else if(compareBits(message_id, IDMask::PCM_TYPE, IDMask::DEVICE_TYPE)){
            return Type::PCM;
        } else if(compareBits(message_id, IDMask::PDP_TYPE, IDMask::DEVICE_TYPE)){
            return Type::PDP;
        }
        return Type::UNKNOWN;
    }
}
