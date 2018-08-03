#include "roborio_manager.hpp"
#include "json_util.hpp"

#include <cmath>

namespace hel{

	std::string CANMotorController::toString()const {
        std::string s = "(";
        s += "type:" + hel::to_string(type) + ", ";
        s += "id:" + std::to_string(id);
        if(type != CANDevice::Type::UNKNOWN){
            s += ", speed:" + std::to_string(speed);
            s += ", inverted:" + hel::to_string(inverted);
        }
        s += ")";
        return s;
    }

    std::string CANMotorController::serialize()const {
        std::string s = "{";
        s += "\"type\":" + hel::quote(hel::to_string(type)) + ", ";
        s += "\"id\":" + std::to_string(id) + ", ";
        s += "\"speed\":" + std::to_string(speed) + ", ";
        s += "\"inverted\":" + std::to_string(inverted);
        s += "}";
        return s;
    }

    CANMotorController CANMotorController::deserialize(std::string s){
        CANMotorController a;
        a.type = s_to_can_device_type(hel::unquote(hel::pullValue("\"type\"",s)));
        a.id = std::stod(hel::pullValue("\"id\"",s));
        a.speed = std::stod(hel::pullValue("\"speed\"",s));
        a.inverted = hel::stob(hel::pullValue("\"inverted\"",s));
        return a;
    }

    CANDevice::Type CANMotorController::getType()const noexcept{
        return type;
    }

    uint8_t CANMotorController::getID()const noexcept{
        return id;
    }

    void CANMotorController::setSpeedData(BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> data)noexcept{
        /*
          For CAN motor controllers:
          data[x] - data[0] results in the number with the correct sign
          data[1] - data[0] is the number of 256*256's
          data[2] - data[0] is the number of 256's
          data[3] - data[0] is the number of 1's
          divide by (256*256*4) to scale from -256*256*4 to 256*256*4 to -1.0 to 1.0
        */
        speed = ((double)((data[1] - data[0])*256*256 + (data[2] - data[0])*256 + (data[3] - data[0])))/(256*256*4);
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

    void CANMotorController::setSpeed(double s)noexcept{
        speed = s;
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

    double CANMotorController::getSpeed()const noexcept{
        return speed;
    }

    BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> CANMotorController::getSpeedData()const noexcept{
        BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> data{0};
        uint32_t speed_int = std::fabs(speed) * 256 * 256 * 4;

        data[1] = speed_int / (256*256);
        speed_int %= 256 * 256;
        data[2] = speed_int / 256;
        speed_int %= 256;
        data[3] = speed_int;

        if(speed < 0.0){
            data[0] = 255;
            data[1] = 255 - data[1];
            data[2] = 255 - data[2];
            data[3] = 255 - data[3];
        }
        return data;
    }

    void CANMotorController::setInverted(bool i)noexcept{
        inverted = i;
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

	CANMotorController::CANMotorController()noexcept:type(CANDevice::Type::UNKNOWN),id(0),speed(0.0),inverted(false){}

    CANMotorController::CANMotorController(const CANMotorController& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(type);
        COPY(id);
        COPY(speed);
        COPY(inverted);
#undef COPY
    }

    CANMotorController::CANMotorController(uint32_t message_id)noexcept:CANMotorController(){
        type = CANDevice::pullDeviceType(message_id);
        assert(type == CANDevice::Type::TALON_SRX || type == CANDevice::Type::VICTOR_SPX);
        id = CANDevice::pullDeviceID(message_id);
		auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }
}
