#include "can_device.hpp"
#include "error.hpp"
#include "util.hpp"

namespace hel{

    std::string CANMessageID::toString()const{
        std::string s;
        s += asString(manufacturer) + " " + asString(type);
        s += " (ID " + std::to_string(id) + ", API ID " + std::to_string(api_id) + ")";
        return s;
    }

    std::string asString(CANMessageID::Type type){
        switch(type){
        case CANMessageID::Type::TALON_SRX:
            return "TALON_SRX";
        case CANMessageID::Type::VICTOR_SPX:
            return "VICTOR_SPX";
        case CANMessageID::Type::SPARK_MAX:
            return "SPARK_MAX";
        case CANMessageID::Type::PCM:
            return "PCM";
        case CANMessageID::Type::PDP:
            return "SPARK_MAX";
        case CANMessageID::Type::UNKNOWN:
            return "UNKNOWN";
        default:
            throw UnhandledEnumConstantException("hel::CANMessageID::Type");
        }
    }

    CANMessageID::Type s_to_can_device_type(std::string input){
        switch(hasher(input.c_str())){
        case hasher("TALON_SRX"):
            return CANMessageID::Type::TALON_SRX;
        case hasher("VICTOR_SPX"):
            return CANMessageID::Type::VICTOR_SPX;
        case hasher("SPARK_MAX"):
            return CANMessageID::Type::SPARK_MAX;
        case hasher("PCM"):
            return CANMessageID::Type::PCM;
        case hasher("PDP"):
            return CANMessageID::Type::PDP;
        case hasher("UNKNOWN"):
            return CANMessageID::Type::UNKNOWN;
        default:
            throw UnhandledCase();
        }
    }

    std::string asString(CANMessageID::Manufacturer type){
        switch(type){
        case CANMessageID::Manufacturer::BROADCAST:
            return "BROADCAST";
        case CANMessageID::Manufacturer::NI:
            return "NI";
        case CANMessageID::Manufacturer::LM:
            return "LM";
        case CANMessageID::Manufacturer::DEKA:
            return "DEKA";
        case CANMessageID::Manufacturer::CTRE:
            return "CTRE";
        case CANMessageID::Manufacturer::REV:
            return "REV";
        case CANMessageID::Manufacturer::MS:
            return "MS";
        case CANMessageID::Manufacturer::TEAM_USE:
            return "TEAM_USE";
        default:
            throw UnhandledEnumConstantException("hel::CANMessageID::Manufacturer " + std::to_string((unsigned)type));
        }
    }

    CANMessageID::Type parseDeviceType(CANMessageID::Manufacturer manufacturer, int32_t type){
        // Device type constants determined from testing
        switch(manufacturer){
        case CANMessageID::Manufacturer::CTRE:
            switch(type){
            case 1:
                return CANMessageID::Type::VICTOR_SPX;
            case 2:
                return CANMessageID::Type::TALON_SRX;
            case 8:
                return CANMessageID::Type::PDP;
            case 9:
                return CANMessageID::Type::PCM;
            default:
                return CANMessageID::Type::UNKNOWN;
            }
            break;
        case CANMessageID::Manufacturer::REV:
            switch(type){
            case 2:
                return CANMessageID::Type::SPARK_MAX;
            default:
                return CANMessageID::Type::UNKNOWN;
            }
            break;
        default:
            return CANMessageID::Type::UNKNOWN;
        }
    }

    int32_t convertDeviceType(CANMessageID::Type type){
        // Device type constants determined from testing
        switch(type){
        case CANMessageID::Type::VICTOR_SPX:
            return 1;
        case CANMessageID::Type::TALON_SRX:
            return 2;
        case CANMessageID::Type::PDP:
            return 8;
        case CANMessageID::Type::PCM:
            return 9;
        case CANMessageID::Type::SPARK_MAX:
            return 2;
        case CANMessageID::Type::UNKNOWN:
        default:
            assert(0);
        }
    }

    CANMessageID CANMessageID::parse(uint32_t message_id){
        CANMessageID a;
        a.manufacturer = static_cast<CANMessageID::Manufacturer>((message_id >> 16) & 0xFF);
        a.type         = parseDeviceType(a.manufacturer, (message_id >> 24) & 0x1F);
        a.api_id       = (message_id >> 6) & 0x3FF;
        a.id           = message_id & 0x3F;
        return a;
    }

    uint32_t CANMessageID::generate(Type t, Manufacturer m, int32_t a, uint8_t i)noexcept{
        // Logic taken from HAL
        assert(i <= MAX_CAN_BUS_ADDRESS);
        int32_t type_int = convertDeviceType(t);
        uint32_t message = 0;
        message |= (type_int & 0x1F) << 24;
        message |= (static_cast<int32_t>(m) & 0xFF) << 16;
        message |= (a & 0x3FF) << 6;
        message |= (i & 0x3F);
        return message;
    }

    CANMessageID::Type CANMessageID::getType()const noexcept{
        return type;
    }

    CANMessageID::Manufacturer CANMessageID::getManufacturer()const noexcept{
        return manufacturer;
    }

    int32_t CANMessageID::getAPIID()const noexcept{
        return api_id;
    }

    uint8_t CANMessageID::getID()const noexcept{
        return id;
    }

    CANDevice::CANDevice()noexcept:type(CANMessageID::Type::UNKNOWN),id(0){}

    CANDevice::CANDevice(CANMessageID device)noexcept:type(device.getType()),id(device.getID()){}

    CANDevice::CANDevice(const CANDevice& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(type);
        COPY(id);
#undef COPY
    }
    CANMessageID::Type CANDevice::getType()const noexcept{
        return type;
    }

    uint8_t CANDevice::getID()const noexcept{
        return id;
    }
}
