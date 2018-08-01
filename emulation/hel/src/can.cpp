#include "roborio_manager.hpp"
#include "json_util.hpp"

#include <algorithm>
#include <cmath>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{

    std::string to_string(CANDevice::Type type){
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

    CANDevice::Type s_to_can_device_type(std::string s){
        switch(hasher(s.c_str())){
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

    std::string CANMotorController::toString()const{
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

    std::string CANMotorController::serialize()const{
        std::string s = "{";
        s += "\"type\":" + hel::quote(hel::to_string(type)) + ", ";
        s += "\"id\":" + std::to_string(id);
        s += "\"speed\":" + std::to_string(speed);
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

    CANDevice::Type CANMotorController::getType()const{
        return type;
    }

    uint8_t CANMotorController::getID()const{
        return id;
    }

    void CANMotorController::setSpeedData(BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> data){
        /*
          For CAN motor controllers:
          data[x] - data[0] results in the number with the correct sign
          data[1] - data[0] is the number of 256*256's
          data[2] - data[0] is the number of 256's
          data[3] - data[0] is the number of 1's
          divide by (256*256*4) to scale from -256*256*4 to 256*256*4 to -1.0 to 1.0
        */
        speed = ((double)((data[1] - data[0])*256*256 + (data[2] - data[0])*256 + (data[3] - data[0])))/(256*256*4);
    }

    void CANMotorController::setSpeed(double s){
        speed = s;
    }

    double CANMotorController::getSpeed()const{
        return speed;
    }

    BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> CANMotorController::getSpeedData()const{
        BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> data;
        uint32_t speed_int = std::fabs(speed) * 256 * 256 * 4;
        for(uint8_t& a: data){
            a = 0;
        }

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

    void CANMotorController::setInverted(bool i){
        inverted = i;
    }

    uint8_t CANDevice::pullDeviceID(uint32_t message_id){
        return message_id & IDMask::DEVICE_ID;
    }

    CANDevice::Type CANDevice::pullDeviceType(uint32_t message_id){
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

    CANMotorController::CANMotorController():type(CANDevice::Type::UNKNOWN),id(0),speed(0.0),inverted(false){}

    CANMotorController::CANMotorController(uint32_t message_id):CANMotorController(){
        type = CANDevice::pullDeviceType(message_id);
        assert(type == CANDevice::Type::TALON_SRX || type == CANDevice::Type::VICTOR_SPX);
        id = CANDevice::pullDeviceID(message_id);
    }
}

extern "C"{
    static const uint32_t SILENT_UNKNOWN_DEVICE_ID = 262271;

    void FRC_NetworkCommunication_CANSessionMux_sendMessage(uint32_t messageID, const uint8_t* data, uint8_t dataSize, int32_t /*periodMs*/, int32_t* /*status*/){
        if(messageID == SILENT_UNKNOWN_DEVICE_ID){
            return;
        }

        hel::CANDevice::Type target_type = hel::CANDevice::pullDeviceType(messageID);
        switch(target_type){
        case hel::CANDevice::Type::TALON_SRX:
        case hel::CANDevice::Type::VICTOR_SPX:
        {
            auto instance = hel::RoboRIOManager::getInstance();

            hel::CANMotorController can_device = {messageID};

            hel::BoundsCheckedArray<uint8_t, hel::CANMotorController::MessageData::SIZE> data_array;
            std::copy(data, data + dataSize, data_array.begin());

            uint8_t command_byte = data[hel::CANMotorController::MessageData::COMMAND_BYTE];

            if(hel::checkBitHigh(command_byte,hel::CANMotorController::SendCommandByteMask::SET_POWER_PERCENT)){
                can_device.setSpeedData(data_array);
            }
            if(hel::checkBitHigh(command_byte,hel::CANMotorController::SendCommandByteMask::SET_INVERTED)){
                can_device.setInverted(true);
            }
            instance.first->can_motor_controllers[can_device.getID()] = can_device;
            {
                for(unsigned i = 0; i < 8; i++){
                    if(hel::checkBitHigh(command_byte,i) &&
                       i != hel::CANMotorController::SendCommandByteMask::SET_POWER_PERCENT &&
                       i != hel::CANMotorController::SendCommandByteMask::SET_INVERTED
                    ){
                        std::cerr<<"Synthesis warning: Writing to CAN motor controller with device ID "<<can_device.getID()<<" using command data byte "<<command_byte<<"\n";
                    }
                }
            }
            instance.second.unlock();
            break;
        }
        case hel::CANDevice::Type::UNKNOWN:
        case hel::CANDevice::Type::PCM:
        case hel::CANDevice::Type::PDP:
            std::cerr<<"Synthesis warning: Attempting to write to unsupported CAN device (" + hel::to_string(target_type) + ") using message ID "<<messageID<<"\n";
            break;
        default:
            throw hel::UnhandledEnumConstantException("hel::CANDevice::Type");
        }
    }

    void FRC_NetworkCommunication_CANSessionMux_receiveMessage(uint32_t* messageID, uint32_t /*messageIDMask*/, uint8_t* /*data*/, uint8_t* /*dataSize*/, uint32_t* /*timeStamp*/, int32_t* /*status*/){
        if(*messageID == SILENT_UNKNOWN_DEVICE_ID){
            return;
        }

        hel::CANDevice::Type target_type = hel::CANDevice::pullDeviceType(*messageID);

        switch(target_type){
        case hel::CANDevice::Type::TALON_SRX:
        case hel::CANDevice::Type::VICTOR_SPX:
        {
            uint8_t device_id = hel::CANDevice::pullDeviceID(*messageID);
            auto instance = hel::RoboRIOManager::getInstance();
            if(instance.first->can_motor_controllers.find(device_id) == instance.first->can_motor_controllers.end()){
                std::cerr<<"Synthesis warning: Attempting to read from missing CAN motor controller (" + hel::to_string(target_type) + " with ID "<<((unsigned)device_id)<<")\n";
            } else{
                std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Attempting to read from CAN motor controller (" + hel::to_string(target_type) + " with ID "<<((unsigned)device_id)<<") using message ID "<<*messageID<<"\n";
                /*
                if(hel::compareBits(*messageID, hel::CANMotorController::ReceiveCommandIDMask::GET_POWER_PERCENT, hel::CANMotorController::ReceiveCommandIDMask::GET_POWER_PERCENT)){
                    hel::BoundsCheckedArray<uint8_t, hel::CANMotorController::MessageData::SIZE> data_array = instance.first->can_motor_controllers[device_id].getSpeedData();
                    std::copy(data_array.begin(), data_array.end(), data);
                    *dataSize = hel::CANMotorController::MessageData::SIZE;
                    *timeStamp = hel::Global::getCurrentTime() / 1000;
                }
                */
            }
            instance.second.unlock();
            break;
        }
        case hel::CANDevice::Type::UNKNOWN:
        case hel::CANDevice::Type::PCM:
        case hel::CANDevice::Type::PDP:
            std::cerr<<"Synthesis warning: Attempting to read from CAN device (" + hel::to_string(target_type) + ") using message ID "<<*messageID<<"\n";
            break;
        default:
            throw hel::UnhandledEnumConstantException("hel::CANDevice::Type");
        }
    }

    void FRC_NetworkCommunication_CANSessionMux_openStreamSession(uint32_t* /*sessionHandle*/, uint32_t /*messageID*/, uint32_t /*messageIDMask*/, uint32_t /*maxMessages*/, int32_t* /*status*/){
        std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Function call FRC_NetworkCommunication_CANSessionMux_openStreamSession\n";
    }

    void FRC_NetworkCommunication_CANSessionMux_closeStreamSession(uint32_t /*sessionHandle*/){
        std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Function call FRC_NetworkCommunication_CANSessionMux_closeStreamSession\n";
    }

    void FRC_NetworkCommunication_CANSessionMux_readStreamSession(uint32_t /*sessionHandle*/, struct tCANStreamMessage* /*messages*/, uint32_t /*messagesToRead*/, uint32_t* /*messagesRead*/, int32_t* /*status*/){
        std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Function call FRC_NetworkCommunication_CANSessionMux_readStreamSession\n";
    }

    void FRC_NetworkCommunication_CANSessionMux_getCANStatus(float* /*percentBusUtilization*/, uint32_t* /*busOffCount*/, uint32_t* /*txFullCount*/, uint32_t* /*receiveErrorCount*/, uint32_t* /*transmitErrorCount*/, int32_t* /*status*/){
        std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Function call FRC_NetworkCommunication_CANSessionMux_getCANStatus\n";
    }

}
