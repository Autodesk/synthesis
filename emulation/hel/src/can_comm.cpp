#include "roborio_manager.hpp"
#include "util.hpp"
#include <iomanip>
using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{

    namespace ctre{
        void sendMessage(const CANMessageID& message_id, const std::vector<uint8_t>& data){
            auto instance = hel::RoboRIOManager::getInstance();

            switch(message_id.getType()){
            case CANMessageID::Type::TALON_SRX:
            case CANMessageID::Type::VICTOR_SPX:
                {
                    if(instance.first->can_motor_controllers.find(message_id.getID()) == instance.first->can_motor_controllers.end()){
                        // Add motor controller to map if one with controller ID is not found
                        instance.first->can_motor_controllers[message_id.getID()] = std::make_shared<ctre::CANMotorController>(message_id);
                    }
                    instance.first->can_motor_controllers[message_id.getID()]->parseCANPacket(message_id.getAPIID(), data);
                    break;
                }
            case CANMessageID::Type::PCM:
                {
                    instance.first->pcm.parseCANPacket(message_id.getAPIID(), data);
                    break;
                }
            case CANMessageID::Type::UNKNOWN:
            case CANMessageID::Type::PDP:
                std::cerr<<"Synthesis warning: Attempting to write to unsupported CAN device (" << asString(message_id.getType()) << " from " << asString(message_id.getManufacturer()) << ")\n";
                break;
            default:
                throw UnhandledEnumConstantException("hel::CANMessageID::Type");
            }

            instance.second.unlock();
        }

        void receiveMessage(const CANMessageID& message_id){
            auto instance = hel::RoboRIOManager::getInstance();

            switch(message_id.getType()){
            case hel::CANMessageID::Type::TALON_SRX:
            case hel::CANMessageID::Type::VICTOR_SPX:
                {
                    if(instance.first->can_motor_controllers.find(message_id.getID()) == instance.first->can_motor_controllers.end()){
                        std::cerr<<"Synthesis warning: Attempting to read from missing CAN motor controller (" + asString(message_id.getType()) + " with ID "<<((unsigned)message_id.getID())<<")\n";
                    } else{
                        std::cerr<<"Synthesis warning: Unsupported feature: Attempting to read from CAN motor controller (" + asString(message_id.getType()) + " with ID "<<((unsigned)message_id.getID())<<")\n";
                        /*
                        if(hel::compareBits(*messageID, hel::CANMotorController::ReceiveCommandIDMask::GET_POWER_PERCENT, hel::CANMotorController::ReceiveCommandIDMask::GET_POWER_PERCENT)){
                            std:vector<uint8_t> data_v = instance.first->can_motor_controllers[message_id.getID()]->generateCANPacket(message_id.getAPIID(), data);
                            std::copy(data_v.begin(), data_v.end(), data);
                            *dataSize = hel::CANMotorController::MessageData::SIZE;
                            *timeStamp = hel::Global::getCurrentTime() / 1000;
                        }
                        */
                    }
                    break;
                }
            default:
                std::cerr<<"Synthesis warning: Attempting to read from unsupported CAN device (" << asString(message_id.getType()) << " from " << asString(message_id.getManufacturer()) << ")\n";
                break;
            }
            instance.second.unlock();
        }
    }

    namespace rev{
        void sendMessage(const CANMessageID& message_id, const std::vector<uint8_t>& data){
            auto instance = hel::RoboRIOManager::getInstance();

            switch(message_id.getType()){
            case CANMessageID::Type::SPARK_MAX:
                {
                    if(instance.first->can_motor_controllers.find(message_id.getID()) == instance.first->can_motor_controllers.end()){
                        // Add motor controller to map if one with controller ID is not found
                        instance.first->can_motor_controllers[message_id.getID()] = std::make_shared<rev::CANMotorController>(message_id);
                    }
                    /*std::cout << std::hex << "api_id:" << message_id.getAPIID() << " data:[";
                    for(unsigned i = 0; i < data.size(); i++){
                        std::cout << std::hex << (int)data[i];
                        if(i < (data.size() - 1)){
                            std::cout << ", ";
                        }
                    }
                    std::cout << "]\n";*/
                    // TODO
                    break;
                }
            default:
                std::cerr<<"Synthesis warning: Attempting to write to unsupported CAN device (" << asString(message_id.getType()) << " from " << asString(message_id.getManufacturer()) << ")\n";
                break;
            }

            instance.second.unlock();
        }

        void receiveMessage(const CANMessageID& message_id){
            switch(message_id.getType()){
            case hel::CANMessageID::Type::SPARK_MAX:
                {
                    std::cout << std::hex << "Receiving - target_type:" << asString(message_id.getType()) << "\n";
                    break;
                }
                break;
            default:
                std::cerr<<"Synthesis warning: Attempting to read from unsupported CAN device (" << asString(message_id.getType()) << " from " << asString(message_id.getManufacturer()) << ")\n";
                break;
            }
        }
    }

    constexpr uint32_t SILENT_UNKNOWN_DEVICE_ID = 262271; //Unclear what this CAN address is attempting to communicate. This is used to silence Synthesis warnings about this CAN address not being found since user code tries to communicate to it very frequently, which causes lag with all the warnings
}

extern "C"{

    void FRC_NetworkCommunication_CANSessionMux_sendMessage(uint32_t messageID, const uint8_t* data, uint8_t dataSize, int32_t /*periodMs*/, int32_t* /*status*/){
        if(messageID == hel::SILENT_UNKNOWN_DEVICE_ID){
            return;
        }

        hel::CANMessageID message_id = hel::CANMessageID::parse(messageID);
        // std::cout << "Sending " << asString(message_id.getType()) << " " << asString(message_id.getManufacturer()) << " " << message_id.getAPIID() << " " << (int)message_id.getID() << "\n"; // TODO delete

        std::vector<uint8_t> data_v;
        data_v.reserve(dataSize);

        if(data != nullptr){
            std::copy(data, data + dataSize, std::back_inserter(data_v));
        }

        switch(message_id.getManufacturer()){
        case hel::CANMessageID::Manufacturer::CTRE:
            hel::ctre::sendMessage(message_id, data_v);
            break;
        case hel::CANMessageID::Manufacturer::REV:
            hel::rev::sendMessage(message_id, data_v);
            break;
        default:
            std::cerr << "Synthesis warning: Attempting to write to unsupported CAN device (" + asString(message_id.getType()) + ") using message ID " << messageID << "\n";
            break;
        }
    }

    void FRC_NetworkCommunication_CANSessionMux_receiveMessage(uint32_t* messageID, uint32_t /*messageIDMask*/, uint8_t* /*data*/, uint8_t* dataSize, uint32_t* /*timeStamp*/, int32_t* /*status*/){
        if(messageID != nullptr && *messageID == hel::SILENT_UNKNOWN_DEVICE_ID){
            return;
        }

        *dataSize = 0;

        hel::CANMessageID message_id = hel::CANMessageID::parse(*messageID);

        // std::cout << "Receiving " << asString(message_id.getType()) << " " << asString(message_id.getManufacturer()) << " " << message_id.getAPIID() << " " << (int)message_id.getID() << "\n"; // TODO delete

        switch(message_id.getManufacturer()){
        case hel::CANMessageID::Manufacturer::CTRE:
            hel::ctre::receiveMessage(message_id);
            break;
        case hel::CANMessageID::Manufacturer::REV:
            hel::rev::receiveMessage(message_id);
            break;
        default:
            std::cerr<<"Synthesis warning: Attempting to read from unsupported CAN device (" << asString(message_id.getType()) << " from " << asString(message_id.getManufacturer()) << ")\n";
            break;
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
