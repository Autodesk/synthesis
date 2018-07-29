#include "roborio.hpp"

#include <algorithm>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{

    void CANBus::enqueueMessage(CANBus::Message m){
    	out_message_queue.push(m);
      auto instance = SendDataManager::getInstance();
      instance.first->update();
      instance.second.unlock();
    }

    CANBus::Message CANBus::getNextMessage()const{
    	return in_message_queue.front();
    }

    void CANBus::popNextMessage(){
    	in_message_queue.pop();
      auto instance = SendDataManager::getInstance();
      instance.first->update();
      instance.second.unlock();
    }

    double CANBus::getSpeed(std::array<uint8_t, CANBus::Message::MAX_DATA_SIZE> data){
        double speed = 0;

        /*
          For CAN motor controllers:
          data[x] - data[0] results in the number with the correct sign
          data[1] - data[0] is the number of 256*256's
          data[2] - data[0] is the number of 256's
          data[3] - data[0] is the number of 1's
          divide by (256*256*4) to scale from -256*256*4 to 256*256*4 to -1.0 to 1.0
        */
        speed = (double)((data[1] - data[0])*256*256 + (data[2] - data[0])*256 + (data[3] - data[0]))/(256*256*4);

        return speed;
    }

    CANBus::Message::Message():id(),data(),data_size(),time_stamp(){}

    CANBus::CANBus():in_message_queue(),out_message_queue(){}
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
    hel::CANBus::Message m;
    m.id = messageID;
    m.data_size = dataSize;
    std::copy(data, data + dataSize, std::begin(m.data));
    instance.first->can_bus.enqueueMessage(m);
    instance.second.unlock();
    //TODO handle repeating messages - currently unsupported
}

void FRC_NetworkCommunication_CANSessionMux_receiveMessage(uint32_t* messageID, uint32_t messageIDMask, uint8_t* data, uint8_t* dataSize, uint32_t* /*timeStamp*/, int32_t* /*status*/){
    printf("FRC_NetworkCommunication_CANSessionMux_receiveMessage(");
    printf("messageIDMask:%d)\n", messageIDMask);
    auto instance = hel::RoboRIOManager::getInstance();
    hel::CANBus::Message m = instance.first->can_bus.getNextMessage();
    instance.first->can_bus.popNextMessage();
    *messageID = m.id; //TODO use message mask?
    *dataSize = m.data_size;
    std::copy(std::begin(m.data), std::end(m.data), data);
    instance.second.unlock();
    //TODO figure out what time stamp is marking and add it
}

void FRC_NetworkCommunication_CANSessionMux_openStreamSession(uint32_t* /*sessionHandle*/, uint32_t messageID, uint32_t /*messageIDMask*/, uint32_t /*maxMessages*/, int32_t* /*status*/){//TODO
    printf("C\n");
}

void FRC_NetworkCommunication_CANSessionMux_closeStreamSession(uint32_t /*sessionHandle*/){ //TODO
    printf("D\n");
}

void FRC_NetworkCommunication_CANSessionMux_readStreamSession(uint32_t /*sessionHandle*/, struct tCANStreamMessage* /*messages*/, uint32_t /*messagesToRead*/, uint32_t* /*messagesRead*/, int32_t* /*status*/){//TODO
    printf("E\n");
}

void FRC_NetworkCommunication_CANSessionMux_getCANStatus(float* /*percentBusUtilization*/, uint32_t* /*busOffCount*/, uint32_t* /*txFullCount*/, uint32_t* /*receiveErrorCount*/, uint32_t* /*transmitErrorCount*/, int32_t* /*status*/){ //unnecessary for emulation
    printf("F\n");
}

}
