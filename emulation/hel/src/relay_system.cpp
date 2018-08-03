#include "roborio_manager.hpp"

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
