#include "roborio.hpp"
#include "json_util.hpp"

#include <algorithm>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{

    std::string to_string(CANMotorController::Type type){
        switch(type){
        case CANMotorController::Type::TALON_SRX:
            return "TALON_SRX";
        case CANMotorController::Type::VICTOR_SPX:
            return "VICTOR_SPX";
        case CANMotorController::Type::UNKNOWN:
            return "UNKNOWN";
        default:
            throw UnhandledEnumConstantException("hel::MXPData::Config");
        }
    }

    CANMotorController::Type s_to_can_device_type(std::string s){
        switch(hasher(s.c_str())){
        case hasher("TALON_SRX"):
            return CANMotorController::Type::TALON_SRX;
        case hasher("VICTOR_SPX"):
            return CANMotorController::Type::VICTOR_SPX;
        case hasher("UNKNOWN"):
            return CANMotorController::Type::UNKNOWN;
        default:
            throw UnhandledCase();
        }
    }

    std::string CANMotorController::toString()const{
        std::string s = "(";
        s += "type:" + hel::to_string(type) + ", ";
        s += "id:" + std::to_string(id);
        if(type != Type::UNKNOWN){
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

    CANMotorController::Type CANMotorController::getType()const{
        return type;
    }

    uint8_t CANMotorController::getID()const{
        return id;
    }

    void CANMotorController::setSpeed(BoundsCheckedArray<uint8_t, CANMotorController::MessageData::SIZE> data){
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

    double CANMotorController::getSpeed()const{
        return speed;
    }

    void CANMotorController::setInverted(bool i){
        inverted = i;
    }

    uint8_t CANMotorController::pullDeviceID(uint32_t message_id){
        return message_id & IDMask::DEVICE_ID;
    }

    CANMotorController::Type CANMotorController::pullDeviceType(uint32_t message_id){
        if(compareBits(message_id, IDMask::VICTOR_SPX_TYPE, IDMask::DEVICE_TYPE)){
            return Type::VICTOR_SPX;
        } else if(compareBits(message_id, IDMask::TALON_SRX_TYPE, IDMask::DEVICE_TYPE)){
            return Type::TALON_SRX;
        }
        return Type::UNKNOWN;
    }

    CANMotorController::CANMotorController():type(Type::UNKNOWN),id(0),speed(0.0),inverted(false){}

    CANMotorController::CANMotorController(uint32_t message_id):CANMotorController(){
        type = pullDeviceType(message_id);
        if(type != Type::UNKNOWN){
            id = pullDeviceID(message_id);
        }
    }
}

extern "C"{

    void FRC_NetworkCommunication_CANSessionMux_sendMessage(uint32_t messageID, const uint8_t* data, uint8_t dataSize, int32_t periodMs, int32_t* /*status*/){
        printf("FRC_NetworkCommunication_CANSessionMux_sendMessage(");
        printf("messageID:%d,", messageID);
        printf("data:");
        for(int i = 0; i < dataSize; i++){
            printf("%d,",data[i]);
        }
        printf("dataSize:%d,periodMs:%d)\n",dataSize,periodMs);

        auto instance = hel::RoboRIOManager::getInstance();

        hel::CANMotorController can_device = {messageID};
        if(can_device.getType() == hel::CANMotorController::Type::UNKNOWN){
            std::cerr<<"Synthesis warning: Attempting to write message to unknown CAN device using message ID "<<messageID<<"\n";
        } else{
            hel::BoundsCheckedArray<uint8_t, hel::CANMotorController::MessageData::SIZE> data_array;
            std::copy(data, data + dataSize, data_array.begin());
            uint8_t command_byte = data[hel::CANMotorController::MessageData::COMMAND_BYTE];
            if(hel::checkBitHigh(command_byte,hel::CANMotorController::CommandByteMask::SET_POWER_PERCENT)){
                can_device.setSpeed(data_array);
            }
            if(hel::checkBitHigh(command_byte,hel::CANMotorController::CommandByteMask::SET_INVERTED)){
                can_device.setInverted(true);
            }
            instance.first->can_motor_controllers[can_device.getID()] = can_device;
            //throw hel::UnsupportedFeature("Writing to CAN motor controller with device ID " + std::to_string(device_id) + " using command data byte " + std::to_string(command_byte));//TODO
            std::cout<<"CANMotorController:"<<instance.first->can_motor_controllers[can_device.getID()].toString()<<"\n";
        }
        instance.second.unlock();
    }

    void FRC_NetworkCommunication_CANSessionMux_receiveMessage(uint32_t* messageID, uint32_t messageIDMask, uint8_t* data, uint8_t* dataSize, uint32_t* timeStamp, int32_t* /*status*/){
        printf("FRC_NetworkCommunication_CANSessionMux_receiveMessage(messageID:%d messageIDMask:%d)\n", *messageID, messageIDMask);
        auto instance = hel::RoboRIOManager::getInstance();
        uint8_t device_id = hel::CANMotorController::pullDeviceID(*messageID);
        if(instance.first->can_motor_controllers.find(device_id) != instance.first->can_motor_controllers.end()){
            std::cerr<<"Request for data from CAN device with id "<<((unsigned)device_id)<<"\n";
        }
        instance.second.unlock();
    }

    void FRC_NetworkCommunication_CANSessionMux_openStreamSession(uint32_t* /*sessionHandle*/, uint32_t messageID, uint32_t messageIDMask, uint32_t maxMessages, int32_t* /*status*/){//TODO
        printf("FRC_NetworkCommunication_CANSessionMux_openStreamSession(");
        printf("messageID:%d, ",messageID);
        printf("messageIDMask:%d, ",messageIDMask);
        printf("maxMessages:%d)\n",maxMessages);
    }

    void FRC_NetworkCommunication_CANSessionMux_closeStreamSession(uint32_t sessionHandle){ //TODO
        printf("FRC_NetworkCommunication_CANSessionMux_closeStreamSession(sessionHandle:%d)\n",sessionHandle);
    }

    void FRC_NetworkCommunication_CANSessionMux_readStreamSession(uint32_t sessionHandle, struct tCANStreamMessage* /*messages*/, uint32_t messagesToRead, uint32_t* /*messagesRead*/, int32_t* /*status*/){//TODO
        printf("FRC_NetworkCommunication_CANSessionMux_readStreamSession(sessionHandle:%d,messagesRead:%d)\n",sessionHandle,messagesToRead);
    }

    void FRC_NetworkCommunication_CANSessionMux_getCANStatus(float* /*percentBusUtilization*/, uint32_t* /*busOffCount*/, uint32_t* /*txFullCount*/, uint32_t* /*receiveErrorCount*/, uint32_t* /*transmitErrorCount*/, int32_t* /*status*/){ //unnecessary for emulation
        throw hel::UnsupportedFeature("Function call FRC_NetworkCommunication_CANSessionMux_getCANStatus");
    }

}
