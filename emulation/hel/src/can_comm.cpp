#include "roborio_manager.hpp"
#include "util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

extern "C"{
    //Unclear what this CAN address is attempting to communicate. This is used to silence Synthesis warnings about this CAN address not being found since user code tries to communicate to it very frequently, which causes lag with all the warnings
    static const uint32_t SILENT_UNKNOWN_DEVICE_ID = 262271;

    void FRC_NetworkCommunication_CANSessionMux_sendMessage(uint32_t messageID, const uint8_t* data, uint8_t dataSize, int32_t /*periodMs*/, int32_t* /*status*/){
        if(messageID == SILENT_UNKNOWN_DEVICE_ID){
            return;
        }
        hel::BoundsCheckedArray<uint8_t, hel::CANMotorController::MessageData::SIZE> data_array{0};
        if(data != nullptr){
            std::copy(data, data + dataSize, data_array.begin());
        }

        hel::CANDevice::Type target_type = hel::CANDevice::pullDeviceType(messageID);
        switch(target_type){
        case hel::CANDevice::Type::TALON_SRX:
        case hel::CANDevice::Type::VICTOR_SPX:
        {
            uint8_t controller_id = hel::CANDevice::pullDeviceID(messageID);
            uint8_t command_byte = data[hel::CANMotorController::MessageData::COMMAND_BYTE];

            auto instance = hel::RoboRIOManager::getInstance();
            if(instance.first->can_motor_controllers.find(controller_id) == instance.first->can_motor_controllers.end()){ //add motor controller to map if one with controller ID is not found
                instance.first->can_motor_controllers[controller_id] = {controller_id,target_type};
            }
            if(hel::checkBitHigh(command_byte,hel::CANMotorController::SendCommandByteMask::SET_POWER_PERCENT)){
                instance.first->can_motor_controllers[controller_id].setPercentOutputData(data_array);
            }
            if(hel::checkBitHigh(command_byte,hel::CANMotorController::SendCommandByteMask::SET_INVERTED)){
                instance.first->can_motor_controllers[controller_id].setInverted(true);
            }
            instance.second.unlock();

            for(unsigned i = 0; i < 8; i++){ //check for unrecognized command bits
                if(
                    i != hel::CANMotorController::SendCommandByteMask::SET_POWER_PERCENT &&
                    i != hel::CANMotorController::SendCommandByteMask::SET_INVERTED &&
                    hel::checkBitHigh(command_byte,i)
                    ){
                    std::cerr<<"Synthesis warning: Unsupported feature: Writing to CAN motor controller ("<<asString(target_type)<<" with ID "<<((unsigned)controller_id)<<") using unknown command data byte "<<((unsigned)command_byte)<<" (bit "<<i<<")\n";
                }
            }
            break;
        }
        case hel::CANDevice::Type::PCM:
        {
            auto instance = hel::RoboRIOManager::getInstance();
            instance.first->pcm.setSolenoids(data_array[hel::PCM::MessageData::SOLENOIDS]);
            instance.second.unlock();
            break;
        }
        case hel::CANDevice::Type::UNKNOWN:
        case hel::CANDevice::Type::PDP:
            std::cerr<<"Synthesis warning: Attempting to write to unsupported CAN device (" + asString(target_type) + ") using message ID "<<messageID<<"\n";
            break;
        default:
            throw hel::UnhandledEnumConstantException("hel::CANDevice::Type");
        }
    }

    void FRC_NetworkCommunication_CANSessionMux_receiveMessage(uint32_t* messageID, uint32_t /*messageIDMask*/, uint8_t* /*data*/, uint8_t* /*dataSize*/, uint32_t* /*timeStamp*/, int32_t* /*status*/){
        if(messageID != nullptr && *messageID == SILENT_UNKNOWN_DEVICE_ID){
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
                std::cerr<<"Synthesis warning: Attempting to read from missing CAN motor controller (" + asString(target_type) + " with ID "<<((unsigned)device_id)<<") using message ID "<<*messageID<<"\n";
            } else{
                std::cerr<<"Synthesis warning: Unsupported feature: Attempting to read from CAN motor controller (" + asString(target_type) + " with ID "<<((unsigned)device_id)<<") using message ID "<<*messageID<<"\n";
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
        case hel::CANDevice::Type::PCM:
        case hel::CANDevice::Type::UNKNOWN:
        case hel::CANDevice::Type::PDP:
            std::cerr<<"Synthesis warning: Unsupported feature: Attempting to read from CAN device (" + asString(target_type) + ") using message ID "<<*messageID<<"\n";
            break;
        default:
            throw hel::UnhandledEnumConstantException("hel::CANDevice::Type");
        }
    }

    void FRC_NetworkCommunication_CANSessionMux_openStreamSession(uint32_t* /*sessionHandle*/, uint32_t /*messageID*/, uint32_t /*messageIDMask*/, uint32_t /*maxMessages*/, int32_t* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call FRC_NetworkCommunication_CANSessionMux_openStreamSession\n";
    }

    void FRC_NetworkCommunication_CANSessionMux_closeStreamSession(uint32_t /*sessionHandle*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call FRC_NetworkCommunication_CANSessionMux_closeStreamSession\n";
    }

    void FRC_NetworkCommunication_CANSessionMux_readStreamSession(uint32_t /*sessionHandle*/, struct tCANStreamMessage* /*messages*/, uint32_t /*messagesToRead*/, uint32_t* /*messagesRead*/, int32_t* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call FRC_NetworkCommunication_CANSessionMux_readStreamSession\n";
    }

    void FRC_NetworkCommunication_CANSessionMux_getCANStatus(float* /*percentBusUtilization*/, uint32_t* /*busOffCount*/, uint32_t* /*txFullCount*/, uint32_t* /*receiveErrorCount*/, uint32_t* /*transmitErrorCount*/, int32_t* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call FRC_NetworkCommunication_CANSessionMux_getCANStatus\n";
    }
}
