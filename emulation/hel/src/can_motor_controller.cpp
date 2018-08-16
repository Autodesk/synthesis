#include "roborio_manager.hpp"
#include "json_util.hpp"

#include <cmath>

namespace hel{

    std::string CANMotorController::toString()const {
        std::string s = "(";
        s += "type:" + asString(type) + ", ";
        s += "id:" + std::to_string(id);
        if(type != CANDevice::Type::UNKNOWN){
            s += ", percent_output:" + std::to_string(percent_output);
            s += ", inverted:" + asString(inverted);
        }
        s += ")";
        return s;
    }

    std::string CANMotorController::serialize()const {
        std::string s = "{";
        s += "\"type\":" + quote(asString(type)) + ", ";
        s += "\"id\":" + std::to_string(id) + ", ";
        s += "\"percent_output\":" + std::to_string(percent_output) + ", ";
        s += "\"inverted\":" + std::to_string(inverted);
        s += "}";
        return s;
    }

    CANMotorController CANMotorController::deserialize(std::string input){
        CANMotorController a;
        a.type = s_to_can_device_type(unquote(pullObject("\"type\"",input)));
        a.id = std::stod(pullObject("\"id\"",input));
        a.percent_output = std::stod(pullObject("\"percent_output\"",input));
        a.inverted = stob(pullObject("\"inverted\"",input));
        return a;
    }

    CANDevice::Type CANMotorController::getType()const noexcept{
        return type;
    }

    uint8_t CANMotorController::getID()const noexcept{
        return id;
    }

    void CANMotorController::setPercentOutputData(BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> data)noexcept{
        /*
          For CAN motor controllers:
          data[x] - data[0] results in the number with the correct sign
          data[1] - data[0] is the number of 256*256's
          data[2] - data[0] is the number of 256's
          data[3] - data[0] is the number of 1's
          divide by (256*256*4) to scale from the range -256*256*4 to 256*256*4 to the range -1.0 to 1.0
        */
        percent_output = ((double)((data[1] - data[0])*256*256 + (data[2] - data[0])*256 + (data[3] - data[0])))/(256*256*4);
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

    void CANMotorController::setPercentOutput(double out)noexcept{
        percent_output = out;
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

    double CANMotorController::getPercentOutput()const noexcept{
        return percent_output;
    }

    BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> CANMotorController::getPercentOutputData()const noexcept{
        BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> data{0};
        uint32_t percent_output_int = std::fabs(percent_output) * 256 * 256 * 4;

        //divide percent_output_int among the bytes as expected by CTRE's CAN protocol
        data[1] = percent_output_int / (256*256);
        percent_output_int %= 256 * 256;
        data[2] = percent_output_int / 256;
        percent_output_int %= 256;
        data[3] = percent_output_int;

        if(percent_output < 0.0){//format as 2's compliment
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

    CANMotorController::CANMotorController()noexcept:type(CANDevice::Type::UNKNOWN),id(0),percent_output(0.0),inverted(false){}

    CANMotorController::CANMotorController(uint8_t i, CANDevice::Type t)noexcept:type(t),id(i),percent_output(0.0),inverted(false){
        assert(type == CANDevice::Type::TALON_SRX || type == CANDevice::Type::VICTOR_SPX);
    }

    CANMotorController::CANMotorController(const CANMotorController& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(type);
        COPY(id);
        COPY(percent_output);
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
