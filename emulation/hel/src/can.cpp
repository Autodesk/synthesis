#include "roborio.hpp"

#include <algorithm>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{

    void CANBus::enqueueMessage(CANBus::Message m){
    	out_message_queue.push(m);
    }

    CANBus::Message CANBus::getNextMessage()const{
    	return in_message_queue.front();
    }

    void CANBus::popNextMessage(){
    	in_message_queue.pop();
    }

    CANBus::Message::Message():id(),data(),data_size(),time_stamp(){}

    CANBus::CANBus():in_message_queue(),out_message_queue(){}
}

void FRC_NetworkCommunication_CANSessionMux_sendMessage(uint32_t messageID, const uint8_t* data, uint8_t dataSize, int32_t /*periodMs*/, int32_t* /*status*/){
    auto instance = hel::RoboRIOManager::getInstance();
    hel::CANBus::Message m;
    m.id = messageID;
    m.data_size = dataSize;
    std::copy(data, data + dataSize, std::begin(m.data));
    instance.first->can_bus.enqueueMessage(m);
    instance.second.unlock();
    //TODO handle repeating messages - currently unsupported
}

void FRC_NetworkCommunication_CANSessionMux_receiveMessage(uint32_t* messageID, uint32_t /*messageIDMask*/, uint8_t* data, uint8_t* dataSize, uint32_t* /*timeStamp*/, int32_t* /*status*/){
    auto instance = hel::RoboRIOManager::getInstance();
    hel::CANBus::Message m = instance.first->can_bus.getNextMessage();
    instance.first->can_bus.popNextMessage();
    *messageID = m.id; //TODO use message mask?
    *dataSize = m.data_size;
    std::copy(std::begin(m.data), std::end(m.data), data);
    instance.second.unlock();
    //TODO figure out what time stamp is marking and add it
}

void FRC_NetworkCommunication_CANSessionMux_openStreamSession(uint32_t* /*sessionHandle*/, uint32_t /*messageID*/, uint32_t /*messageIDMask*/, uint32_t /*maxMessages*/, int32_t* /*status*/){} //TODO

void FRC_NetworkCommunication_CANSessionMux_closeStreamSession(uint32_t /*sessionHandle*/){} //TODO

void FRC_NetworkCommunication_CANSessionMux_readStreamSession(uint32_t /*sessionHandle*/, struct tCANStreamMessage* /*messages*/, uint32_t /*messagesToRead*/, uint32_t* /*messagesRead*/, int32_t* /*status*/){} //TODO

void FRC_NetworkCommunication_CANSessionMux_getCANStatus(float* /*percentBusUtilization*/, uint32_t* /*busOffCount*/, uint32_t* /*txFullCount*/, uint32_t* /*receiveErrorCount*/, uint32_t* /*transmitErrorCount*/, int32_t* /*status*/){} //unnecessary for emulation

