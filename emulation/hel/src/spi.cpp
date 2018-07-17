#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tSPI::tAutoTriggerConfig RoboRIO::SPISystem::getAutoTriggerConfig()const{
        return auto_trigger_config;
    }

    void RoboRIO::SPISystem::setAutoTriggerConfig(tSPI::tAutoTriggerConfig config){
        auto_trigger_config = config;
    }

    tSPI::tAutoByteCount RoboRIO::SPISystem::getAutoByteCount()const{
        return auto_byte_count;
    }

    void RoboRIO::SPISystem::setAutoByteCount(tSPI::tAutoByteCount count){
        auto_byte_count = count;
    }

    tSPI::tChipSelectActiveHigh RoboRIO::SPISystem::getChipSelectActiveHigh()const{
        return chip_select_active_high;
    }

    void RoboRIO::SPISystem::setChipSelectActiveHigh(tSPI::tChipSelectActiveHigh select){
        chip_select_active_high = select;
    }

    uint8_t RoboRIO::SPISystem::getAutoChipSelect()const{
        return auto_chip_select;
    }

    void RoboRIO::SPISystem::setAutoChipSelect(uint8_t select){
        auto_chip_select = select;
    }

    bool RoboRIO::SPISystem::getAutoSPI1Select()const{
        return auto_spi_1_select;
    }

    void RoboRIO::SPISystem::setAutoSPI1Select(bool select){
        auto_spi_1_select = select;
    }

    uint32_t RoboRIO::SPISystem::getAutoRate()const{
        return auto_rate;
    }

    void RoboRIO::SPISystem::setAutoRate(uint32_t rate){
        auto_rate = rate;
    }

    uint8_t RoboRIO::SPISystem::getEnabledDIO()const{
        return enabled_dio;
    }

    void RoboRIO::SPISystem::setEnabledDIO(uint8_t enabled){
        enabled_dio = enabled;
    }

    RoboRIO::SPISystem::SPISystem():auto_trigger_config(),auto_byte_count(),chip_select_active_high(),auto_chip_select(),auto_spi_1_select(),auto_rate(),enabled_dio(){}

    struct SPIManager: public tSPI{
        tSystemInterface* getSystemInterface(){
            return nullptr;
        }

        uint32_t readDebugIntStatReadCount(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint16_t readDebugState(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        void writeAutoTriggerConfig(tAutoTriggerConfig value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoTriggerConfig(value);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_ExternalClockSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.ExternalClockSource_Channel = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_ExternalClockSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.ExternalClockSource_Module = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_ExternalClockSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.ExternalClockSource_AnalogTrigger = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_RisingEdge(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.RisingEdge = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_FallingEdge(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.FallingEdge = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_ExternalClock(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.ExternalClock = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        tAutoTriggerConfig readAutoTriggerConfig(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig();
        }

        uint8_t readAutoTriggerConfig_ExternalClockSource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().ExternalClockSource_Channel;
        }

        uint8_t readAutoTriggerConfig_ExternalClockSource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().ExternalClockSource_Module;
        }

        bool readAutoTriggerConfig_ExternalClockSource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().ExternalClockSource_AnalogTrigger;
        }

        bool readAutoTriggerConfig_RisingEdge(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().RisingEdge;
        }

        bool readAutoTriggerConfig_FallingEdge(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().FallingEdge;
        }

        bool readAutoTriggerConfig_ExternalClock(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().ExternalClock;
        }

        void writeAutoChipSelect(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoChipSelect(value);
            instance.second.unlock();
        }

        uint8_t readAutoChipSelect(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoChipSelect();
        }

        uint32_t readDebugRevision(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint32_t readTransferSkippedFullCount(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        void writeAutoByteCount(tAutoByteCount value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoByteCount(value);
            instance.second.unlock();
        }

        void writeAutoByteCount_TxByteCount(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tAutoByteCount count = instance.first->spi_system.getAutoByteCount();
            count.TxByteCount = value;
            instance.first->spi_system.setAutoByteCount(count);
            instance.second.unlock();
        }

        void writeAutoByteCount_ZeroByteCount(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tAutoByteCount count = instance.first->spi_system.getAutoByteCount();
            count.ZeroByteCount = value;
            instance.first->spi_system.setAutoByteCount(count);
            instance.second.unlock();
        }

        tAutoByteCount readAutoByteCount(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoByteCount();
        }

        uint8_t readAutoByteCount_TxByteCount(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoByteCount().TxByteCount;
        }

        uint8_t readAutoByteCount_ZeroByteCount(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoByteCount().ZeroByteCount;
        }

        uint32_t readDebugIntStat(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint32_t readDebugEnabled(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        void writeAutoSPI1Select(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoSPI1Select(value);
            instance.second.unlock();
        }

        bool readAutoSPI1Select(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoSPI1Select();
        }

        uint8_t readDebugSubstate(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        void writeAutoRate(uint32_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoRate(value);
            instance.second.unlock();
        }

        uint32_t readAutoRate(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoRate();
        }

        void writeEnableDIO(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setEnabledDIO(value);
            instance.second.unlock();
        }

        uint8_t readEnableDIO(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getEnabledDIO();
        }

        void writeChipSelectActiveHigh(tChipSelectActiveHigh value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setChipSelectActiveHigh(value);
            instance.second.unlock();
        }

        void writeChipSelectActiveHigh_Hdr(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tChipSelectActiveHigh select = instance.first->spi_system.getChipSelectActiveHigh();
            select.Hdr = value;
            instance.first->spi_system.setChipSelectActiveHigh(select);
            instance.second.unlock();
        }

        void writeChipSelectActiveHigh_MXP(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tChipSelectActiveHigh select = instance.first->spi_system.getChipSelectActiveHigh();
            select.MXP = value;
            instance.first->spi_system.setChipSelectActiveHigh(select);
            instance.second.unlock();
        }

        tChipSelectActiveHigh readChipSelectActiveHigh(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getChipSelectActiveHigh();
        }

        uint8_t readChipSelectActiveHigh_Hdr(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getChipSelectActiveHigh().Hdr;
        }

        uint8_t readChipSelectActiveHigh_MXP(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getChipSelectActiveHigh().MXP;
        }

        void strobeAutoForceOne(tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoTx(uint8_t reg_index, uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readAutoTx(uint8_t reg_index, uint8_t bitfield_index, tRioStatusCode* /*status*/){
            //TODO
        }
    };
}
namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tSPI* tSPI::create(tRioStatusCode* /*status*/){
            return new hel::SPIManager();
        }
    }
}
