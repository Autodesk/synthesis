#include "roborio_manager.hpp"
#include "system_interface.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{

    tRelay::tValue RelaySystem::getValue()const noexcept{
        return value;
    }

    void RelaySystem::setValue(tRelay::tValue v)noexcept{
        value = v;
    }

    RelaySystem::RelaySystem()noexcept:value(){}
    RelaySystem::RelaySystem(const RelaySystem& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(value);
#undef COPY
    }

    RelaySystem::State RelaySystem::getState(uint8_t index)noexcept{
        bool forward = checkBitHigh(value.Forward, index);
        bool reverse  = checkBitHigh(value.Reverse, index);
        if(forward){
            if(reverse){
                return State::ERROR;
            }
            return State::FORWARD;
        }
        if(reverse){
            return State::REVERSE;
        }
        return State::OFF;
    }

    std::string asString(RelaySystem::State state){
        switch(state){
        case RelaySystem::State::OFF:
            return "OFF";
        case RelaySystem::State::REVERSE:
            return "REVERSE";
        case RelaySystem::State::FORWARD:
            return "FORWARD";
        case RelaySystem::State::ERROR:
            return "ERROR";
        default:
            throw UnhandledEnumConstantException("hel::RelaySystem::State");
        }
    }


    struct RelayManager: public tRelay{
        tSystemInterface* getSystemInterface(){
            return new SystemInterface();
        }

        void writeValue(tValue value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->relay_system.setValue(value);
            instance.second.unlock();
        }

        void writeValue_Forward(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tRelay::tValue v = instance.first->relay_system.getValue();
            v.Forward = value;
            instance.first->relay_system.setValue(v);
            instance.second.unlock();
        }

        void writeValue_Reverse(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tRelay::tValue v = instance.first->relay_system.getValue();
            v.Reverse = value;
            instance.first->relay_system.setValue(v);
            instance.second.unlock();
        }

        tValue readValue(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->relay_system.getValue();
        }

        uint8_t readValue_Forward(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->relay_system.getValue().Forward;
        }

        uint8_t readValue_Reverse(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->relay_system.getValue().Reverse;
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tRelay* tRelay::create(tRioStatusCode* /*status*/){
            return new hel::RelayManager();
        }
    }
}
