#include "roborio_manager.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tSPI::tAutoTriggerConfig SPISystem::getAutoTriggerConfig()const{
        return auto_trigger_config;
    }

    void SPISystem::setAutoTriggerConfig(tSPI::tAutoTriggerConfig config){
        auto_trigger_config = config;
    }

    tSPI::tAutoByteCount SPISystem::getAutoByteCount()const{
        return auto_byte_count;
    }

    void SPISystem::setAutoByteCount(tSPI::tAutoByteCount count){
        auto_byte_count = count;
    }

    tSPI::tChipSelectActiveHigh SPISystem::getChipSelectActiveHigh()const{
        return chip_select_active_high;
    }

    void SPISystem::setChipSelectActiveHigh(tSPI::tChipSelectActiveHigh select){
        chip_select_active_high = select;
    }

    uint8_t SPISystem::getAutoChipSelect()const{
        return auto_chip_select;
    }

    void SPISystem::setAutoChipSelect(uint8_t select){
        auto_chip_select = select;
    }

    bool SPISystem::getAutoSPI1Select()const{
        return auto_spi_1_select;
    }

    void SPISystem::setAutoSPI1Select(bool select){
        auto_spi_1_select = select;
    }

    uint32_t SPISystem::getAutoRate()const{
        return auto_rate;
    }

    void SPISystem::setAutoRate(uint32_t rate){
        auto_rate = rate;
    }

    uint8_t SPISystem::getEnabledDIO()const{
        return enabled_dio;
    }

    void SPISystem::setEnabledDIO(uint8_t enabled){
        enabled_dio = enabled;
    }

    SPISystem::SPISystem()noexcept:auto_trigger_config(),auto_byte_count(),chip_select_active_high(),auto_chip_select(0),auto_spi_1_select(0),auto_rate(0),enabled_dio(0){}
    SPISystem::SPISystem(const SPISystem& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(auto_trigger_config);
        COPY(auto_byte_count);
        COPY(chip_select_active_high);
        COPY(auto_chip_select);
        COPY(auto_spi_1_select);
        COPY(auto_rate);
        COPY(enabled_dio);
#undef COPY
    }

    struct SPIManager: public tSPI{
        tSystemInterface* getSystemInterface(){
            return new SystemInterface();
        }

        uint32_t readDebugIntStatReadCount(tRioStatusCode* /*status*/){ //unnecessary for emulation
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readDebugIntStatReadCount\n";
            return 0;
        }

        uint16_t readDebugState(tRioStatusCode* /*status*/){ //unnecessary for emulation
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readDebugState\n";
            return 0;
        }

        void writeAutoTriggerConfig(tAutoTriggerConfig value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoTriggerConfig\n";
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoTriggerConfig(value);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_ExternalClockSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoTriggerConfig_ExternalClockSource_Channel\n";
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.ExternalClockSource_Channel = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_ExternalClockSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoTriggerConfig_ExternalClockSource_Module\n";
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.ExternalClockSource_Module = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_ExternalClockSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoTriggerConfig_ExternalClockSource_AnalogTrigger\n";
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.ExternalClockSource_AnalogTrigger = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_RisingEdge(bool value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoTriggerConfig_RisingEdge\n";
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.RisingEdge = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_FallingEdge(bool value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoTriggerConfig_FallingEdge\n";
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.FallingEdge = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        void writeAutoTriggerConfig_ExternalClock(bool value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoTriggerConfig_ExternalClock\n";
            auto instance = RoboRIOManager::getInstance();
            tAutoTriggerConfig config = instance.first->spi_system.getAutoTriggerConfig();
            config.ExternalClock = value;
            instance.first->spi_system.setAutoTriggerConfig(config);
            instance.second.unlock();
        }

        tAutoTriggerConfig readAutoTriggerConfig(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoTriggerConfig\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig();
        }

        uint8_t readAutoTriggerConfig_ExternalClockSource_Channel(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoTriggerConfig_ExternalClockSource_Channel\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().ExternalClockSource_Channel;
        }

        uint8_t readAutoTriggerConfig_ExternalClockSource_Module(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoTriggerConfig_ExternalClockSource_Module\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().ExternalClockSource_Module;
        }

        bool readAutoTriggerConfig_ExternalClockSource_AnalogTrigger(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoTriggerConfig_ExternalClockSource_AnalogTrigger\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().ExternalClockSource_AnalogTrigger;
        }

        bool readAutoTriggerConfig_RisingEdge(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoTriggerConfig_RisingEdge\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().RisingEdge;
        }

        bool readAutoTriggerConfig_FallingEdge(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoTriggerConfig_FallingEdge\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().FallingEdge;
        }

        bool readAutoTriggerConfig_ExternalClock(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoTriggerConfig_ExternalClock\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoTriggerConfig().ExternalClock;
        }

        void writeAutoChipSelect(uint8_t value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoChipSelect\n";
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoChipSelect(value);
            instance.second.unlock();
        }

        uint8_t readAutoChipSelect(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoChipSelect\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoChipSelect();
        }

        uint32_t readDebugRevision(tRioStatusCode* /*status*/){ //unnecessary for emulation
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readDebugRevision\n";
            return 0;
        }

        uint32_t readTransferSkippedFullCount(tRioStatusCode* /*status*/){ //unnecessary for emulation
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readTransferSkippedFullCount\n";
            return 0;
        }

        void writeAutoByteCount(tAutoByteCount value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoByteCount\n";
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoByteCount(value);
            instance.second.unlock();
        }

        void writeAutoByteCount_TxByteCount(uint8_t value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoByteCount_TxByteCount\n";
            auto instance = RoboRIOManager::getInstance();
            tAutoByteCount count = instance.first->spi_system.getAutoByteCount();
            count.TxByteCount = value;
            instance.first->spi_system.setAutoByteCount(count);
            instance.second.unlock();
        }

        void writeAutoByteCount_ZeroByteCount(uint8_t value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoByteCount_ZeroByteCount\n";
            auto instance = RoboRIOManager::getInstance();
            tAutoByteCount count = instance.first->spi_system.getAutoByteCount();
            count.ZeroByteCount = value;
            instance.first->spi_system.setAutoByteCount(count);
            instance.second.unlock();
        }

        tAutoByteCount readAutoByteCount(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoByteCount\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoByteCount();
        }

        uint8_t readAutoByteCount_TxByteCount(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoByteCount_TxByteCount\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoByteCount().TxByteCount;
        }

        uint8_t readAutoByteCount_ZeroByteCount(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoByteCount_ZeroByteCount\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoByteCount().ZeroByteCount;
        }

        uint32_t readDebugIntStat(tRioStatusCode* /*status*/){ //unnecessary for emulation
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readDebugIntStat\n";
            return 0;
        }

        uint32_t readDebugEnabled(tRioStatusCode* /*status*/){ //unnecessary for emulation
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readDebugEnabled\n";
            return 0;
        }

        void writeAutoSPI1Select(bool value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoSPI1Select\n";
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoSPI1Select(value);
            instance.second.unlock();
        }

        bool readAutoSPI1Select(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoSPI1Select\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoSPI1Select();
        }

        uint8_t readDebugSubstate(tRioStatusCode* /*status*/){ //unnecessary for emulation
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readDebugSubstate\n";
            return 0;
        }

        void writeAutoRate(uint32_t value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoRate\n";
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setAutoRate(value);
            instance.second.unlock();
        }

        uint32_t readAutoRate(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoRate\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getAutoRate();
        }

        void writeEnableDIO(uint8_t value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeEnableDIO\n";
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setEnabledDIO(value);
            instance.second.unlock();
        }

        uint8_t readEnableDIO(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readEnableDIO\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getEnabledDIO();
        }

        void writeChipSelectActiveHigh(tChipSelectActiveHigh value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeChipSelectActiveHigh\n";
            auto instance = RoboRIOManager::getInstance();
            instance.first->spi_system.setChipSelectActiveHigh(value);
            instance.second.unlock();
        }

        void writeChipSelectActiveHigh_Hdr(uint8_t value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeChipSelectActiveHigh_Hdr\n";
            auto instance = RoboRIOManager::getInstance();
            tChipSelectActiveHigh select = instance.first->spi_system.getChipSelectActiveHigh();
            select.Hdr = value;
            instance.first->spi_system.setChipSelectActiveHigh(select);
            instance.second.unlock();
        }

        void writeChipSelectActiveHigh_MXP(uint8_t value, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeChipSelectActiveHigh_MXP\n";
            auto instance = RoboRIOManager::getInstance();
            tChipSelectActiveHigh select = instance.first->spi_system.getChipSelectActiveHigh();
            select.MXP = value;
            instance.first->spi_system.setChipSelectActiveHigh(select);
            instance.second.unlock();
        }

        tChipSelectActiveHigh readChipSelectActiveHigh(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readChipSelectActiveHigh\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getChipSelectActiveHigh();
        }

        uint8_t readChipSelectActiveHigh_Hdr(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readChipSelectActiveHigh_Hdr\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getChipSelectActiveHigh().Hdr;
        }

        uint8_t readChipSelectActiveHigh_MXP(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readChipSelectActiveHigh_MXP\n";
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->spi_system.getChipSelectActiveHigh().MXP;
        }

        void strobeAutoForceOne(tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::strobeAutoForceOne\n";
            //TODO
        }

        void writeAutoTx(uint8_t /*reg_index*/, uint8_t /*bitfield_index*/, uint8_t /*value*/, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::writeAutoTx\n";
             //TODO
        }

        uint8_t readAutoTx(uint8_t /*reg_index*/, uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            std::cerr<<"Synthesis warning: Unsupported feature: Function call tSPI::readAutoTx\n";
            //TODO
            return 0;
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
