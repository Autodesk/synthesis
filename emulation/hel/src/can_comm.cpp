#include "roborio_manager.hpp"
#include "util.hpp"

#include "FRC_NetworkCommunication/CANSessionMux.h"

#include <iostream> // TODO delete

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
                warnUnsupportedDevice(message_id, "write to");
                break;
            default:
                throw UnhandledEnumConstantException("hel::CANMessageID::Type");
            }

            instance.second.unlock();
        }

      std::vector<uint8_t> receiveMessage(const CANMessageID& message_id){
            auto instance = hel::RoboRIOManager::getInstance();

            switch(message_id.getType()){
            case hel::CANMessageID::Type::TALON_SRX:
            case hel::CANMessageID::Type::VICTOR_SPX:
                {
                    if(instance.first->can_motor_controllers.find(message_id.getID()) == instance.first->can_motor_controllers.end()){
                        warnUnconnectedDevice(message_id, "read from");
                    } else{
                        warnUnsupportedDevice(message_id, "read from");
                        // return instance.first->can_motor_controllers[message_id.getID()]->generateCANPacket(message_id.getAPIID());
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
        void sendMessage(const CANMessageID& message_id, const std::vector<uint8_t>& data){
            auto instance = hel::RoboRIOManager::getInstance();

            switch(message_id.getType()){
            case CANMessageID::Type::SPARK_MAX:
                {
                    if(instance.first->can_motor_controllers.find(message_id.getID()) == instance.first->can_motor_controllers.end()){
                        if(message_id.getAPIID() == rev::CANMotorController::CommandAPIID::FIRMWARE){
                        // On startup, REV tries to write parameters assuming all CAN IDs are associated with a REV controller. Instead of adding controllers it tries to talk to, instead, add controllers it tries to read firmware versions from.
                        instance.first->can_motor_controllers[message_id.getID()] = std::make_shared<rev::CANMotorController>(message_id);
                        } else {
                        warnUnconnectedDevice(message_id, "write to");
                        }
                    } else {
                        instance.first->can_motor_controllers[message_id.getID()]->parseCANPacket(message_id.getAPIID(), data);
                    }
                    // TODO
                    break;
                }
            default:
                warnUnsupportedDevice(message_id, "write to");
                break;
            }

            instance.second.unlock();
        }

      std::vector<uint8_t> receiveMessage(const CANMessageID& message_id){
          auto instance = hel::RoboRIOManager::getInstance();

            switch(message_id.getType()){
            case hel::CANMessageID::Type::SPARK_MAX:
              if(message_id.getAPIID() == rev::CANMotorController::CommandAPIID::FIRMWARE && instance.first->can_motor_controllers.find(message_id.getID()) == instance.first->can_motor_controllers.end()){
                // On startup, REV tries to write parameters assuming all CAN IDs are associated with a REV controller. Instead of adding controllers it tries to talk to, instead, add controllers it tries to read firmware versions from.
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

    constexpr uint32_t SILENT_UNKNOWN_DEVICE_ID = 262271; //Unclear what this CAN address is attempting to communicate. This is used to silence Synthesis warnings about this CAN address not being found since user code tries to communicate to it very frequently, which causes lag with all the warnings
}

extern "C"{

    void FRC_NetworkCommunication_CANSessionMux_sendMessage(uint32_t messageID, const uint8_t* data, uint8_t dataSize, int32_t periodMs, int32_t* /*status*/){
        // if(messageID == hel::SILENT_UNKNOWN_DEVICE_ID){ // TODO figure out what this is
        //     return;
        // }

      if(periodMs != CAN_SEND_PERIOD_NO_REPEAT && periodMs != CAN_SEND_PERIOD_STOP_REPEATING){
        hel::warnUnsupportedFeature("CANSessionMux repeating message not yet repeated (periodMs: " + std::to_string(periodMs) + ")");
      }

        hel::CANMessageID message_id = hel::CANMessageID::parse(messageID);

        std::vector<uint8_t> data_v;
        data_v.reserve(dataSize);

        if(data != nullptr){
            std::copy(data, data + dataSize, std::back_inserter(data_v));
        }

        // std::cout << "Sending   " << message_id.toString() << " ["; // TODO delete
        // for(unsigned i = 0; i < data_v.size(); i++){
        //   std::cout << (int)data_v[i];
        //   if(i < (data_v.size() - 1)){
        //     std::cout << ", ";
        //   }
        // }
        // std::cout << "]\n";

        switch(message_id.getManufacturer()){
        case hel::CANMessageID::Manufacturer::CTRE:
            hel::ctre::sendMessage(message_id, data_v);
            break;
        case hel::CANMessageID::Manufacturer::REV:
            hel::rev::sendMessage(message_id, data_v);
            break;
        default:
            hel::warnUnsupportedDevice(message_id, "write to");
            break;
        }
    }

    void FRC_NetworkCommunication_CANSessionMux_receiveMessage(uint32_t* messageID, uint32_t messageIDMask, uint8_t* data, uint8_t* dataSize, uint32_t* timeStamp, int32_t* status){
        // if(messageID != nullptr && *messageID == hel::SILENT_UNKNOWN_DEVICE_ID){
        //     return;
        // }

        hel::CANMessageID message_id = hel::CANMessageID::parse(*messageID & messageIDMask);
        std::vector<uint8_t> data_v;

        // std::cout << "Receiving " << message_id.toString() << "\n"; // TODO delete

        switch(message_id.getManufacturer()){
        case hel::CANMessageID::Manufacturer::CTRE:
            data_v = hel::ctre::receiveMessage(message_id);
            break;
        case hel::CANMessageID::Manufacturer::REV:
            data_v = hel::rev::receiveMessage(message_id);
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
