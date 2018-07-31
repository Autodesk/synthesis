#include "roborio_manager.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    bool Alarm::getEnabled()const{
        return enabled;
    }

    void Alarm::setEnabled(bool a){
        enabled = a;
    }

    uint32_t Alarm::getTriggerTime()const{
        return trigger_time;
    }

    void Alarm::setTriggerTime(uint32_t time){
        trigger_time = time;
    }

    struct AlarmManager: public tAlarm{ //TODO implement full logic
        tSystemInterface* getSystemInterface(){
            return nullptr;
        }

        void writeEnable(bool value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.first->alarm.setEnabled(value);
            instance.second.unlock();
        }

        bool readEnable(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->alarm.getEnabled();
        }

        void writeTriggerTime(uint32_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.first->alarm.setTriggerTime(value);
            instance.second.unlock();
        }

        uint32_t readTriggerTime(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->alarm.getTriggerTime();
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tAlarm* tAlarm::create(tRioStatusCode* /*status*/){
            return new hel::AlarmManager();
        }
    }
}
