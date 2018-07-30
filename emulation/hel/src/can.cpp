#include "roborio.hpp"
#include "json_util.hpp"

#include <algorithm>

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
        default:
            throw UnhandledEnumConstantException("hel::MXPData::Config");
        }
    }

    CANDevice::Type s_to_can_device_type(std::string s){
        switch(hasher(s.c_str())){
        case hasher("TALON_SRX"):
            return CANDevice::Type::TALON_SRX;
        case hasher("VICTOR_SPX"):
            return CANDevice::Type::VICTOR_SPX;
        case hasher("UNKNOWN"):
            return CANDevice::Type::UNKNOWN;
        default:
            throw UnhandledCase();
        }
    }

    std::string CANDevice::toString()const{
        std::string s = "(";
        s += "type:" + to_string(type) + ", ";
        s += "id:" + to_string(id);
        if(type != Type::UNKNOWN){
            s += ", speed:" + std::to_string(speed);
        }
        s += ")";
        return s;
    }

    std::string CANDevice::serialize()const{
        std::string s = "{";
        s += "\"type\":" + hel::quote(hel::to_string(type)) + ", ";
        s += "\"id\":" + std::to_string(id);
        s += "\"speed\":" + std::to_string(speed);
        s += "}";
        return s;
    }

    CANDevice CANDevice::deserialize(std::string s){
        CANDevice a;
        a.type = s_to_can_device_type(hel::unquote(hel::pullValue("\"type\"",s)));
        a.id = std::stod(hel::pullValue("\"id\"",s));
        a.speed = std::stod(hel::pullValue("\"speed\"",s));
        return a;
    }

    CANDevice::Type CANDevice::getType()const{
        return type;
    }

    void CANDevice::setSpeed(BoundsCheckedArray<uint8_t, CANDevice::MAX_MESSAGE_DATA_SIZE> data){
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

    double CANDevice::getSpeed()const{
        return speed;
    }

    CANDevice::CANDevice():type(Type::UNKNOWN),id(0),speed(0.0){}

    CANDevice::CANDevice(uint32_t message_id):CANDevice(){
        if(message_id >= VICTOR_SPX_ZERO_ADDRESS && message_id <= (VICTOR_SPX_ZERO_ADDRESS + MAX_CAN_BUS_ADDRESS)){
            id = message_id - VICTOR_SPX_ZERO_ADDRESS;
            type = Type::VICTOR_SPX;
        } else if(message_id >= TALON_SRX_ZERO_ADDRESS && message_id <= (TALON_SRX_ZERO_ADDRESS + MAX_CAN_BUS_ADDRESS)){
            id = message_id - VICTOR_SPX_ZERO_ADDRESS;
            type = Type::TALON_SRX;
        }
    }

    bool checkCANID(uint32_t id_a, uint32_t id_b, uint32_t id_mask){
        unsigned msb = std::max(findMostSignificantBit(id_a), findMostSignificantBit(id_b));
        for(unsigned i = 0 ; i < msb; i++){
            if(checkBitHigh(id_mask,i)){
                if(checkBitHigh(id_a,i) != checkBitHigh(id_b,i)){
                    return false;
                }
            }
        }
        return true;
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

        if(instance.first->can_devices.find(messageID) == instance.first->can_devices.end()){
            instance.first->can_devices[messageID] = hel::CANDevice(messageID);
        }
        if(instance.first->can_devices[messageID].getType() == hel::CANDevice::Type::UNKNOWN){
            std::cerr<<"Synthesis warning: Attempting to write message to unknown CAN device using message ID "<<messageID<<"\n";
        } else{
            hel::BoundsCheckedArray<uint8_t, hel::CANDevice::MAX_MESSAGE_DATA_SIZE> data_array;
            std::copy(data, data + dataSize, data_array.begin());
            instance.first->can_devices[messageID].setSpeed(data_array);
        }
        instance.second.unlock();
    }

    void FRC_NetworkCommunication_CANSessionMux_receiveMessage(uint32_t* messageID, uint32_t messageIDMask, uint8_t* data, uint8_t* dataSize, uint32_t* timeStamp, int32_t* /*status*/){
        printf("FRC_NetworkCommunication_CANSessionMux_receiveMessage(messageID:%d messageIDMask:%d)\n", *messageID, messageIDMask);
        auto instance = hel::RoboRIOManager::getInstance();

        for(auto i = instance.first->can_devices.begin(); i != instance.first->can_devices.end(); ++i){
            if(hel::checkCANID(*messageID,i->first,messageIDMask)){
                //TODO
            }
        }

        instance.second.unlock();
        //TODO figure out what time stamp is marking and add it
    }

    void FRC_NetworkCommunication_CANSessionMux_openStreamSession(uint32_t* /*sessionHandle*/, uint32_t messageID, uint32_t messageIDMask, uint32_t maxMessages, int32_t* /*status*/){//TODO
        printf("FRC_NetworkCommunication_CANSessionMux_openStreamSession(\n");
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
