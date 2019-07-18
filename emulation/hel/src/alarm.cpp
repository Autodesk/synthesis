#include "roborio_manager.hpp"
#include "system_interface.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    bool Alarm::getEnabled()const noexcept{
        return enabled;
    }

    void Alarm::setEnabled(bool a)noexcept{
        enabled = a;
    }

    uint32_t Alarm::getTriggerTime()const noexcept{
        return trigger_time;
    }

    void Alarm::setTriggerTime(uint32_t time)noexcept{
        trigger_time = time;
    }

    Alarm::Alarm()noexcept:enabled(false),trigger_time(0){}

    Alarm::Alarm(const Alarm& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(enabled);
        COPY(trigger_time);
#undef COPY
    }

    struct AlarmManager: public tAlarm{ //TODO implement full logic
        tSystemInterface* getSystemInterface(){
            return new SystemInterface();
        }

        void writeEnable(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->alarm.setEnabled(value);
            instance.second.unlock();
        }

        bool readEnable(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->alarm.getEnabled();
        }

        void writeTriggerTime(uint32_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->alarm.setTriggerTime(value);
            instance.second.unlock();
        }

        uint32_t readTriggerTime(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
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
