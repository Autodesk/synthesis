#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tEncoder::tOutput RoboRIO::Encoder::getOutput()const{
        return output;
    }

    void RoboRIO::Encoder::setOutput(tEncoder::tOutput out){
        output = out;
    }

    tEncoder::tConfig RoboRIO::Encoder::getConfig()const{
        return config;
    }

    void RoboRIO::Encoder::setConfig(tEncoder::tConfig c){
        config = c;
    }

    tEncoder::tTimerOutput RoboRIO::Encoder::getTimerOutput()const{
        return timer_output;
    }

    void RoboRIO::Encoder::setTimerOutput(tEncoder::tTimerOutput output){
        timer_output = output;
    }

    tEncoder::tTimerConfig RoboRIO::Encoder::getTimerConfig()const{
        return timer_config;
    }

    void RoboRIO::Encoder::setTimerConfig(tEncoder::tTimerConfig c){
        timer_config = c;
    }

    struct EncoderManager: public tEncoder{
    private:
        uint8_t index;

    public:
        tSystemInterface* getSystemInterface(){ //unnecessary for emulation
            return nullptr;
        }

        uint8_t getSystemIndex(){
            return index;
        }

        tOutput readOutput(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getOutput();
        }

        bool readOutput_Direction(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getOutput().Direction;
        }

        int32_t readOutput_Value(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getOutput().Value;
        }

        void writeConfig(tConfig value, tRioStatusCode* /*status*/){
            RoboRIOManager::getInstance()->encoders[index].setConfig(value);
        }

        void writeConfig_ASource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.ASource_Channel = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_ASource_Module(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.ASource_Module = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_ASource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.ASource_AnalogTrigger = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_BSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.BSource_Channel = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_BSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.BSource_Module = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_BSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.BSource_AnalogTrigger = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_IndexSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.IndexSource_Channel = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_IndexSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.IndexSource_Module = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.IndexSource_AnalogTrigger = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_IndexActiveHigh(bool value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.IndexActiveHigh = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_IndexEdgeSensitive(bool value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.IndexEdgeSensitive = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        void writeConfig_Reverse(bool value, tRioStatusCode* /*status*/){
            tConfig config = RoboRIOManager::getInstance()->encoders[index].getConfig();
            config.Reverse = value;
            RoboRIOManager::getInstance()->encoders[index].setConfig(config);
        }

        tConfig readConfig(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig();
        }

        uint8_t readConfig_ASource_Channel(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().ASource_Channel;
        }

        uint8_t readConfig_ASource_Module(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().ASource_Module;
        }

        bool readConfig_ASource_AnalogTrigger(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().ASource_AnalogTrigger;
        }

        uint8_t readConfig_BSource_Channel(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().BSource_Channel;
        }

        uint8_t readConfig_BSource_Module(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().BSource_Module;
        }

        bool readConfig_BSource_AnalogTrigger(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().BSource_AnalogTrigger;
        }

        uint8_t readConfig_IndexSource_Channel(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().IndexSource_Channel;
        }

        uint8_t readConfig_IndexSource_Module(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().IndexSource_Module;
        }

        bool readConfig_IndexSource_AnalogTrigger(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().IndexSource_AnalogTrigger;
        }

        bool readConfig_IndexActiveHigh(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().IndexActiveHigh;
        }

        bool readConfig_IndexEdgeSensitive(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().IndexEdgeSensitive;
        }

        bool readConfig_Reverse(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getConfig().Reverse;
        }

        tTimerOutput readTimerOutput(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getTimerOutput();
        }

        uint32_t readTimerOutput_Period(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getTimerOutput().Period;
        }

        int8_t readTimerOutput_Count(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getTimerOutput().Count;
        }

        bool readTimerOutput_Stalled(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getTimerOutput().Stalled;
        }

        void strobeReset(tRioStatusCode* /*status*/){
            RoboRIOManager::getInstance()->encoders[index].setOutput(*(new tOutput()));
            RoboRIOManager::getInstance()->encoders[index].setTimerOutput(*(new tTimerOutput()));
        }

        void writeTimerConfig(tTimerConfig value, tRioStatusCode* /*status*/){
            RoboRIOManager::getInstance()->encoders[index].setTimerConfig(value);
        }

        void writeTimerConfig_StallPeriod(uint32_t value, tRioStatusCode* /*status*/){
            tTimerConfig config = RoboRIOManager::getInstance()->encoders[index].getTimerConfig();
            config.StallPeriod = value;
            RoboRIOManager::getInstance()->encoders[index].setTimerConfig(config);
        }

        void writeTimerConfig_AverageSize(uint8_t value, tRioStatusCode* /*status*/){
            tTimerConfig config = RoboRIOManager::getInstance()->encoders[index].getTimerConfig();
            config.AverageSize = value;
            RoboRIOManager::getInstance()->encoders[index].setTimerConfig(config);
        }

        void writeTimerConfig_UpdateWhenEmpty(bool value, tRioStatusCode* /*status*/){
            tTimerConfig config = RoboRIOManager::getInstance()->encoders[index].getTimerConfig();
            config.UpdateWhenEmpty = value;
            RoboRIOManager::getInstance()->encoders[index].setTimerConfig(config);
        }

        tTimerConfig readTimerConfig(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getTimerConfig();
        }

        uint32_t readTimerConfig_StallPeriod(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getTimerConfig().StallPeriod;
        }

        uint8_t readTimerConfig_AverageSize(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getTimerConfig().AverageSize;
        }

        bool readTimerConfig_UpdateWhenEmpty(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->encoders[index].getTimerConfig().UpdateWhenEmpty;
        }

        EncoderManager(uint8_t i):index(i){}
    };
}
   
namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tEncoder* tEncoder::create(uint8_t sys_index, tRioStatusCode* /*status*/){
            return new hel::EncoderManager(sys_index);
        }
    }
}
