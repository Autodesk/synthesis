#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tPower::tStatus RoboRIO::Power::getStatus()const{
        return status;
    }

    void RoboRIO::Power::setStatus(tPower::tStatus s){
        status = s;
    }

    tPower::tFaultCounts RoboRIO::Power::getFaultCounts()const{
        return fault_counts;
    }

    void RoboRIO::Power::setFaultCounts(tPower::tFaultCounts counts){
        fault_counts = counts;
    }

    tPower::tDisable RoboRIO::Power::getDisabled()const{
        return disabled;
    }

    void RoboRIO::Power::setDisabled(tPower::tDisable d){
        disabled = d;
    }

    struct PowerManager: public tPower{
        tSystemInterface* getSystemInterface(){ //unnecessary for emulation
            return nullptr;
        }

        uint16_t readUserVoltage3V3(tRioStatusCode* /*status*/){
            //TODO
        }

        tStatus readStatus(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getStatus();
        }

        uint8_t readStatus_User3V3(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getStatus().User3V3;
        }

        uint8_t readStatus_User5V(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getStatus().User5V;
        }

        uint8_t readStatus_User6V(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getStatus().User6V;
        }

        uint16_t readUserVoltage6V(tRioStatusCode* /*status*/){
            //TODO
        }

        uint16_t readOnChipTemperature(tRioStatusCode* /*status*/){
            //TODO
        }

        uint16_t readUserVoltage5V(tRioStatusCode* /*status*/){
            //TODO
        }

        void strobeResetFaultCounts(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->power.setFaultCounts(*(new tFaultCounts));
            instance.second.unlock();
        }

        uint16_t readIntegratedIO(tRioStatusCode* /*status*/){
            //TODO
        }

        uint16_t readMXP_DIOVoltage(tRioStatusCode* /*status*/){
            //TODO
        }

        uint16_t readUserCurrent3V3(tRioStatusCode* /*status*/){
            //TODO
        }

        uint16_t readVinVoltage(tRioStatusCode* /*status*/){
            //TODO
        }

        uint16_t readUserCurrent6V(tRioStatusCode* /*status*/){
            //TODO
        }

        uint16_t readUserCurrent5V(tRioStatusCode* /*status*/){
            //TODO
        }

        uint16_t readAOVoltage(tRioStatusCode* /*status*/){
            //TODO
        }

        tFaultCounts readFaultCounts(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getFaultCounts();
        }

        uint8_t readFaultCounts_OverCurrentFaultCount3V3(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getFaultCounts().OverCurrentFaultCount3V3;
        }

        uint8_t readFaultCounts_OverCurrentFaultCount5V(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getFaultCounts().OverCurrentFaultCount5V;
        }

        uint8_t readFaultCounts_OverCurrentFaultCount6V(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getFaultCounts().OverCurrentFaultCount6V;
        }

        uint8_t readFaultCounts_UnderVoltageFaultCount5V(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getFaultCounts().UnderVoltageFaultCount5V;
        }

        uint16_t readVinCurrent(tRioStatusCode* /*status*/){
            //TODO
        }

        void writeDisable(tDisable value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->power.setDisabled(value);
            instance.second.unlock();
        }

        void writeDisable_User3V3(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tDisable disabled = instance.first->power.getDisabled();
            disabled.User3V3 = value;
            instance.first->power.setDisabled(disabled);
            instance.second.unlock();
        }

        void writeDisable_User5V(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tDisable disabled = instance.first->power.getDisabled();
            disabled.User5V = value;
            instance.first->power.setDisabled(disabled);
            instance.second.unlock();
        }

        void writeDisable_User6V(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tDisable disabled = instance.first->power.getDisabled();
            disabled.User6V = value;
            instance.first->power.setDisabled(disabled);
            instance.second.unlock();
        }

        tDisable readDisable(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getDisabled();
        }

        bool readDisable_User3V3(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getDisabled().User3V3;
        }

        bool readDisable_User5V(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getDisabled().User5V;
        }

        bool readDisable_User6V(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->power.getDisabled().User6V;
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tPower* tPower::create(tRioStatusCode* /*status*/){
            return new hel::PowerManager();
        }
    }
}
