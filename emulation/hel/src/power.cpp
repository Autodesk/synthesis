#include "roborio_manager.hpp"
#include "system_interface.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tPower::tStatus Power::getStatus()const noexcept{
        return status;
    }

    void Power::setStatus(tPower::tStatus s)noexcept{
        status = s;
    }

    tPower::tFaultCounts Power::getFaultCounts()const noexcept{
        return fault_counts;
    }

    void Power::setFaultCounts(tPower::tFaultCounts counts)noexcept{
        fault_counts = counts;
    }

    tPower::tDisable Power::getDisabled()const noexcept{
        return disabled;
    }

    void Power::setDisabled(tPower::tDisable d)noexcept{
        disabled = d;
    }

    Power::Power()noexcept:status(),fault_counts(),disabled(){}
    Power::Power(const Power& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(status);
        COPY(fault_counts);
        COPY(disabled);
#undef COPY
    }

    struct PowerManager: public tPower{
        tSystemInterface* getSystemInterface(){ //unnecessary for emulation
            return new SystemInterface();
        }

        uint16_t readUserVoltage3V3(tRioStatusCode* /*status*/){
            return (3.3 + 0.01) * 4.096 / 0.004902; //reverse HAL math to return expected value
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
            return (6.0 + 0.014) * 4.096 / 0.007019; //reverse HAL math to return expected value
        }

        uint16_t readOnChipTemperature(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0; 
        }

        uint16_t readUserVoltage5V(tRioStatusCode* /*status*/){
            return (5.0 + 0.013) * 4.096 / 0.005962; //reverse HAL math to return expected value
        }

        void strobeResetFaultCounts(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->power.setFaultCounts(*(new tFaultCounts));
            instance.second.unlock();
        }

        uint16_t readIntegratedIO(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint16_t readMXP_DIOVoltage(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint16_t readUserCurrent3V3(tRioStatusCode* /*status*/){
            return 0;
        }

        uint16_t readVinVoltage(tRioStatusCode* /*status*/){
            return 0;
        }

        uint16_t readUserCurrent6V(tRioStatusCode* /*status*/){
            return 0;
        }

        uint16_t readUserCurrent5V(tRioStatusCode* /*status*/){
            return 0;
        }

        uint16_t readAOVoltage(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
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
            return 0;
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
