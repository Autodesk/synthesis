#include "roborio_manager.hpp"
#include "json_util.hpp"

#include <cmath>

namespace hel{

    std::string CANMotorControllerBase::toString()const {
        std::string s = "(";
        s += "type:" + asString(getType()) + ", ";
        s += "id:" + std::to_string(getID()) + ", ";
        s += "percent_output:" + std::to_string(percent_output) + ", ";
        s += "inverted:" + asString(inverted);
        s += ")";
        return s;
    }

    std::string CANMotorControllerBase::serialize()const {
        std::string s = "{";
        s += "\"type\":" + quote(asString(getType())) + ", ";
        s += "\"id\":" + std::to_string(getID()) + ", ";
        s += "\"percent_output\":" + std::to_string(percent_output) + ", ";
        s += "\"inverted\":" + std::to_string(inverted);
        s += "}";
        return s;
    }

    std::shared_ptr<CANMotorControllerBase> CANMotorControllerBase::deserialize(std::string /*input*/){
        return std::make_shared<ctre::CANMotorController>(); // Need for deserialization removed in other pull request
    }

    void CANMotorControllerBase::setPercentOutput(double out)noexcept{
        percent_output = out;
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

    double CANMotorControllerBase::getPercentOutput()const noexcept{
        return percent_output;
    }

    void CANMotorControllerBase::setInverted(bool i)noexcept{
        inverted = i;
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

    CANMotorControllerBase::CANMotorControllerBase()noexcept:CANDevice(), percent_output(0.0), inverted(false){}

    CANMotorControllerBase::CANMotorControllerBase(CANMessageID message_id)noexcept:CANDevice(message_id), percent_output(0.0), inverted(false){
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
 }

    CANMotorControllerBase::CANMotorControllerBase(const CANMotorControllerBase& source)noexcept: CANDevice(source){
#define COPY(NAME) NAME = source.NAME
        COPY(percent_output);
        COPY(inverted);
#undef COPY
    }

    namespace ctre{
        CANMotorController::CANMotorController()noexcept: CANMotorControllerBase(){}

        CANMotorController::CANMotorController(CANMessageID message_id)noexcept:CANMotorControllerBase(message_id){
            assert(getType() == CANMessageID::Type::TALON_SRX || getType() == CANMessageID::Type::VICTOR_SPX);
        }

        CANMotorController::CANMotorController(const CANMotorController& source)noexcept: CANMotorControllerBase(source){
//#define COPY(NAME) NAME = source.NAME
//#undef COPY
        }

        void CANMotorController::parseCANPacket(const int32_t& /*API_ID*/, const std::vector<uint8_t>& DATA)noexcept{
            assert(DATA.size() == MessageData::SIZE);

            uint8_t command_byte = DATA[MessageData::COMMAND_BYTE];

            /*
                From testing with CTRE motor controllers:
                DATA[x] - DATA[0] results in the number with the correct sign
                DATA[1] - DATA[0] is the number of 256*256's
                DATA[2] - DATA[0] is the number of 256's
                DATA[3] - DATA[0] is the number of 1's
                divide by (256*256*4) to scale from the range -256*256*4 to 256*256*4 to the range -1.0 to 1.0
            */
            setPercentOutput(((double)( (DATA[1] - DATA[0])*256*256 + (DATA[2] - DATA[0])*256 + (DATA[3] - DATA[0])) ) / (256*256*4));
            setInverted(hel::checkBitHigh(command_byte,SendCommandByteMask::INVERT));

            for(unsigned i = 0; i < 8; i++){ //check for unrecognized command bits
                if(
                     i != SendCommandByteMask::INVERT &&
                     checkBitHigh(command_byte,i)
                ){
                    std::cerr<<"Synthesis warning: Unsupported feature: Writing to CAN motor controller ("<<asString(getType()) <<" with ID "<<((unsigned)getID())<<") using unknown command data byte "<<((unsigned)command_byte)<<" (bit "<<i<<")\n";
                }
            }
        }

        std::vector<uint8_t> CANMotorController::generateCANPacket(const int32_t& /*API_ID*/)noexcept{
            return {}; // TODO
        }
    }

    namespace rev{
        CANMotorController::CANMotorController()noexcept: CANMotorControllerBase(){}

        CANMotorController::CANMotorController(CANMessageID message_id)noexcept:CANMotorControllerBase(message_id){
            assert(getType() == CANMessageID::Type::SPARK_MAX);
        }

    CANMotorController::CANMotorController(const CANMotorController& source)noexcept: CANMotorControllerBase(source){
//#define COPY(NAME) NAME = source.NAME
//#undef COPY
        }

    void CANMotorController::parseCANPacket(const int32_t& /*API_ID*/, const std::vector<uint8_t>& /*DATA*/)noexcept{
            setPercentOutput(0); // TODO
        }

        std::vector<uint8_t> CANMotorController::generateCANPacket(const int32_t& /*API_ID*/)noexcept{
            return {}; // TODO
        }
    }
}
