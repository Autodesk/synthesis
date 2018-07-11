#include "roborio.h"

namespace hel{
    struct SPIManager: public tSPI{
        tSystemInterface* getSystemInterface(){
            //TODO
        }

        uint32_t readDebugIntStatReadCount(tRioStatusCode* /*status*/){
             //TODO
        }

        uint16_t readDebugState(tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoTriggerConfig(tAutoTriggerConfig value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoTriggerConfig_ExternalClockSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoTriggerConfig_ExternalClockSource_Module(uint8_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoTriggerConfig_ExternalClockSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoTriggerConfig_RisingEdge(bool value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoTriggerConfig_FallingEdge(bool value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoTriggerConfig_ExternalClock(bool value, tRioStatusCode* /*status*/){
             //TODO
        }

        tAutoTriggerConfig readAutoTriggerConfig(tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readAutoTriggerConfig_ExternalClockSource_Channel(tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readAutoTriggerConfig_ExternalClockSource_Module(tRioStatusCode* /*status*/){
             //TODO
        }

        bool readAutoTriggerConfig_ExternalClockSource_AnalogTrigger(tRioStatusCode* /*status*/){
             //TODO
        }

        bool readAutoTriggerConfig_RisingEdge(tRioStatusCode* /*status*/){
             //TODO
        }

        bool readAutoTriggerConfig_FallingEdge(tRioStatusCode* /*status*/){
             //TODO
        }

        bool readAutoTriggerConfig_ExternalClock(tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoChipSelect(uint8_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readAutoChipSelect(tRioStatusCode* /*status*/){
             //TODO
        }

        uint32_t readDebugRevision(tRioStatusCode* /*status*/){
             //TODO
        }

        uint32_t readTransferSkippedFullCount(tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoByteCount(tAutoByteCount value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoByteCount_TxByteCount(uint8_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoByteCount_ZeroByteCount(uint8_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        tAutoByteCount readAutoByteCount(tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readAutoByteCount_TxByteCount(tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readAutoByteCount_ZeroByteCount(tRioStatusCode* /*status*/){
             //TODO
        }

        uint32_t readDebugIntStat(tRioStatusCode* /*status*/){
             //TODO
        }

        uint32_t readDebugEnabled(tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoSPI1Select(bool value, tRioStatusCode* /*status*/){
             //TODO
        }

        bool readAutoSPI1Select(tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readDebugSubstate(tRioStatusCode* /*status*/){
             //TODO
        }

        void writeAutoRate(uint32_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        uint32_t readAutoRate(tRioStatusCode* /*status*/){
             //TODO
        }

        void writeEnableDIO(uint8_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readEnableDIO(tRioStatusCode* /*status*/){
             //TODO
        }

        void writeChipSelectActiveHigh(tChipSelectActiveHigh value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeChipSelectActiveHigh_Hdr(uint8_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        void writeChipSelectActiveHigh_MXP(uint8_t value, tRioStatusCode* /*status*/){
             //TODO
        }

        tChipSelectActiveHigh readChipSelectActiveHigh(tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readChipSelectActiveHigh_Hdr(tRioStatusCode* /*status*/){
             //TODO
        }

        uint8_t readChipSelectActiveHigh_MXP(tRioStatusCode* /*status*/){
             //TODO
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
