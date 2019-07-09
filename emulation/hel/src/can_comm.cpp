#include "roborio_manager.hpp"
#include "util.hpp"
#include "FRC_NetworkCommunication/CANSessionMux.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{

    void warnUnsupportedDevice(const CANMessageID& message_id, const std::string& action){
        warnUnsupportedFeature("Attempting to " + action + " CAN device: " + message_id.toString());
    }

    void warnUnconnectedDevice(const CANMessageID& message_id, const std::string& action){
        warn("Attempting to " + action + " unconnected CAN device: " + message_id.toString());
    }

    namespace ctre{
        void parseMessage(const CANMessageID& message_id, const std::vector<uint8_t>& data){
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
                {
                    if(message_id.getID() != ctre::CANMotorController::HEARTBEAT_ID || message_id.getAPIID() != ctre::CANMotorController::HEARTBEAT_API_ID){ // Ignore heartbeat
                        warnUnsupportedDevice(message_id, "write to");
                    }
                    break;
                }
            case CANMessageID::Type::PDP:
                warnUnsupportedDevice(message_id, "write to");
                break;
            default:
                throw UnhandledEnumConstantException("hel::CANMessageID::Type");
            }

            instance.second.unlock();
        }

      std::vector<uint8_t> generateMessage(const CANMessageID& message_id){
            auto instance = hel::RoboRIOManager::getInstance();

            switch(message_id.getType()){
            case hel::CANMessageID::Type::TALON_SRX:
            case hel::CANMessageID::Type::VICTOR_SPX:
                {
                    if(instance.first->can_motor_controllers.find(message_id.getID()) == instance.first->can_motor_controllers.end()){
                        warnUnconnectedDevice(message_id, "read from");
                    } else{
                        instance.second.unlock();
                        return instance.first->can_motor_controllers[message_id.getID()]->generateCANPacket(message_id.getAPIID());
                    }
                    break;
                }
            default:
                warnUnsupportedDevice(message_id, "read from");
                break;
            }
            instance.second.unlock();
            return {};
        }
    }

    namespace rev{
        void parseMessage(const CANMessageID& message_id, const std::vector<uint8_t>& data){
            auto instance = hel::RoboRIOManager::getInstance();

            switch(message_id.getType()){
            case CANMessageID::Type::SPARK_MAX:
                {
                    if(message_id.getID() != rev::CANMotorController::HEARTBEAT_ID || message_id.getAPIID() != rev::CANMotorController::CommandAPIID::HEARTBEAT){ // Ignore heartbeat
                        if(instance.first->can_motor_controllers.find(message_id.getID()) == instance.first->can_motor_controllers.end()){
                            if(message_id.getAPIID() == rev::CANMotorController::CommandAPIID::FIRMWARE){
                                // On startup, REV tries to write parameters assuming all CAN IDs are associated with a REV controller. 
                                // Instead of adding controllers it tries to talk to, instead, add controllers it tries to read firmware versions from.
                                instance.first->can_motor_controllers[message_id.getID()] = std::make_shared<rev::CANMotorController>(message_id);
                            } else {
                                warnUnconnectedDevice(message_id, "write to");
                            }
                        } else {
                            instance.first->can_motor_controllers[message_id.getID()]->parseCANPacket(message_id.getAPIID(), data);
                        }
                    }
                    break;
                }
            default:
                warnUnsupportedDevice(message_id, "write to");
                break;
            }

            instance.second.unlock();
        }

      std::vector<uint8_t> generateMessage(const CANMessageID& message_id){
          auto instance = hel::RoboRIOManager::getInstance();

            switch(message_id.getType()){
            case hel::CANMessageID::Type::SPARK_MAX:
                if(message_id.getAPIID() == rev::CANMotorController::CommandAPIID::FIRMWARE && instance.first->can_motor_controllers.find(message_id.getID()) == instance.first->can_motor_controllers.end()){
                    // On startup, REV tries to write parameters assuming all CAN IDs are associated with a REV controller.
                    // Instead of adding controllers it tries to talk to, instead, add controllers it tries to read firmware versions from.
                    instance.first->can_motor_controllers[message_id.getID()] = std::make_shared<rev::CANMotorController>(message_id);
                }
                instance.second.unlock();
                return instance.first->can_motor_controllers[message_id.getID()]->generateCANPacket(message_id.getAPIID());
            default:
                warnUnsupportedDevice(message_id, "read from");
                break;
            }

            instance.second.unlock();
            return {};
        }
    }
}

extern "C"{
    void FRC_NetworkCommunication_CANSessionMux_sendMessage(uint32_t messageID, const uint8_t* data, uint8_t dataSize, int32_t periodMs, int32_t* /*status*/){
        hel::CANMessageID message_id = hel::CANMessageID::parse(messageID);

        if(periodMs != CAN_SEND_PERIOD_NO_REPEAT && periodMs != CAN_SEND_PERIOD_STOP_REPEATING){
            hel::warnUnsupportedFeature("Sending repeating CAN message (" + message_id.toString() + " periodMs " + std::to_string(periodMs) + ") -- sending once");
        }

        std::vector<uint8_t> data_v;

        if(data != nullptr && dataSize != 0){
            data_v.reserve(dataSize);
            std::copy(data, data + dataSize, std::back_inserter(data_v));
        }

        switch(message_id.getManufacturer()){
        case hel::CANMessageID::Manufacturer::CTRE:
            hel::ctre::parseMessage(message_id, data_v);
            break;
        case hel::CANMessageID::Manufacturer::REV:
            hel::rev::parseMessage(message_id, data_v);
            break;
        default:
            hel::warnUnsupportedDevice(message_id, "write to");
            break;
        }
    }

    void FRC_NetworkCommunication_CANSessionMux_receiveMessage(uint32_t* messageID, uint32_t messageIDMask, uint8_t* data, uint8_t* dataSize, uint32_t* timeStamp, int32_t* /*status*/){
        hel::CANMessageID message_id = hel::CANMessageID::parse(*messageID & messageIDMask);

        std::vector<uint8_t> data_v;

        switch(message_id.getManufacturer()){
        case hel::CANMessageID::Manufacturer::CTRE:
            data_v = hel::ctre::generateMessage(message_id);
            break;
        case hel::CANMessageID::Manufacturer::REV:
            data_v = hel::rev::generateMessage(message_id);
            break;
        default:
            hel::warnUnsupportedDevice(message_id, "read from");
            break;
        }

        std::copy(data_v.begin(), data_v.end(), data);
        *dataSize = data_v.size();
        *timeStamp = hel::Global::getCurrentTime() / 1000; // Milliseconds
    }

    void FRC_NetworkCommunication_CANSessionMux_openStreamSession(uint32_t* /*sessionHandle*/, uint32_t /*messageID*/, uint32_t /*messageIDMask*/, uint32_t /*maxMessages*/, int32_t* /*status*/){
        hel::warnUnsupportedFeature("Function call FRC_NetworkCommunication_CANSessionMux_openStreamSession");
    }

    void FRC_NetworkCommunication_CANSessionMux_closeStreamSession(uint32_t /*sessionHandle*/){
        hel::warnUnsupportedFeature("Function call FRC_NetworkCommunication_CANSessionMux_closeStreamSession");
    }

    void FRC_NetworkCommunication_CANSessionMux_readStreamSession(uint32_t /*sessionHandle*/, struct tCANStreamMessage* /*messages*/, uint32_t /*messagesToRead*/, uint32_t* /*messagesRead*/, int32_t* /*status*/){
        hel::warnUnsupportedFeature("Function call FRC_NetworkCommunication_CANSessionMux_readStreamSession");
    }

    void FRC_NetworkCommunication_CANSessionMux_getCANStatus(float* /*percentBusUtilization*/, uint32_t* /*busOffCount*/, uint32_t* /*txFullCount*/, uint32_t* /*receiveErrorCount*/, uint32_t* /*transmitErrorCount*/, int32_t* /*status*/){
        hel::warnUnsupportedFeature("Function call FRC_NetworkCommunication_CANSessionMux_getCANStatus");
    }
}
