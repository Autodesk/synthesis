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

        void CANMotorController::parseCANPacket(const int32_t& /*API_ID*/, const std::vector<uint8_t>& DATA){
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
                  warnUnsupportedFeature("Writing to CAN motor controller (" + asString(getType()) + " with ID " + std::to_string((unsigned)getID()) + ") using unknown command data byte " + std::to_string((unsigned)command_byte) + " (bit " + std::to_string(i) + ")\n");
                }
            }
        }

        std::vector<uint8_t> CANMotorController::generateCANPacket(const int32_t& /*API_ID*/){
          // if(hel::compareBits(*messageID, hel::CANMotorController::ReceiveCommandIDMask::GET_POWER_PERCENT, hel::CANMotorController::ReceiveCommandIDMask::GET_POWER_PERCENT)){
          // }
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

        void CANMotorController::parseCANPacket(const int32_t& API_ID, const std::vector<uint8_t>& DATA){
            switch(API_ID){
            case CommandAPIID::DC_SET:
                {
                    if(DATA.size() == 0){
                        setPercentOutput(0.0);
                    } else if(DATA.size() == 8){
                        float power = 0.0;
                        std::memcpy(&power, DATA.data(), sizeof(float));
                        setPercentOutput(power);
                    } else {
                        assert(0);
                    }
                    break;
                }
            case CommandAPIID::HEARTBEAT:
            case CommandAPIID::FIRMWARE:
              break;
            default:
                warnUnsupportedFeature("Sending CAN packet to REV Spark MAX with ID " + std::to_string(getID()) + " and API ID " + std::to_string(API_ID));
            }
        }

        std::vector<uint8_t> CANMotorController::generateCANPacket(const int32_t& API_ID){
            std::vector<uint8_t> data;
            switch(API_ID){
            case CommandAPIID::FIRMWARE:
                {
                    uint32_t x = MIN_FIRMWARE_VERSION;
                    // Place each byte of firmware version into data vector in order
                    for(unsigned i = 0; i < 32; i += 8){
                        data.insert(data.begin(), x & 0xFF);
                        x >>= 8;
                    }
                    data.push_back((uint8_t)USE_FIRMWARE_DEBUG_BUILD);
                    data.push_back((uint8_t)HARDWARE_REVISION);
                    break;
                }
            default:
                warnUnsupportedFeature("Receiving CAN packet from REV Spark MAX with ID " + std::to_string(getID()) + " and API ID " + std::to_string(API_ID));
            }

            return data;
        }
    }
}
