#include "roborio.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tEncoder::tOutput Encoder::getOutput()const{
        return output;
    }

    void Encoder::setOutput(tEncoder::tOutput out){
        output = out;
    }

    tEncoder::tConfig Encoder::getConfig()const{
        return config;
    }

    void Encoder::setConfig(tEncoder::tConfig c){
        config = c;
    }

    tEncoder::tTimerOutput Encoder::getTimerOutput()const{
        return timer_output;
    }

    void Encoder::setTimerOutput(tEncoder::tTimerOutput output){
        timer_output = output;
    }

    tEncoder::tTimerConfig Encoder::getTimerConfig()const{
        return timer_config;
    }

    void Encoder::setTimerConfig(tEncoder::tTimerConfig c){
        timer_config = c;
    }

    Encoder::Encoder():output(),config(),timer_output(),timer_config(){}

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
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getOutput();
        }

        bool readOutput_Direction(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getOutput().Direction;
        }

        int32_t readOutput_Value(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getOutput().Value;
        }

        void writeConfig(tConfig value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->encoders[index].setConfig(value);
            instance.second.unlock();
        }

        void writeConfig_ASource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.ASource_Channel = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_ASource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.ASource_Module = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_ASource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.ASource_AnalogTrigger = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_BSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.BSource_Channel = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_BSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.BSource_Module = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_BSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.BSource_AnalogTrigger = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.IndexSource_Channel = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.IndexSource_Module = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.IndexSource_AnalogTrigger = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexActiveHigh(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.IndexActiveHigh = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexEdgeSensitive(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.IndexEdgeSensitive = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_Reverse(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->encoders[index].getConfig();
            config.Reverse = value;
            instance.first->encoders[index].setConfig(config);
            instance.second.unlock();
        }

        tConfig readConfig(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig();
        }

        uint8_t readConfig_ASource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().ASource_Channel;
        }

        uint8_t readConfig_ASource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().ASource_Module;
        }

        bool readConfig_ASource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().ASource_AnalogTrigger;
        }

        uint8_t readConfig_BSource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().BSource_Channel;
        }

        uint8_t readConfig_BSource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().BSource_Module;
        }

        bool readConfig_BSource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().BSource_AnalogTrigger;
        }

        uint8_t readConfig_IndexSource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().IndexSource_Channel;
        }

        uint8_t readConfig_IndexSource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().IndexSource_Module;
        }

        bool readConfig_IndexSource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().IndexSource_AnalogTrigger;
        }

        bool readConfig_IndexActiveHigh(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().IndexActiveHigh;
        }

        bool readConfig_IndexEdgeSensitive(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().IndexEdgeSensitive;
        }

        bool readConfig_Reverse(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getConfig().Reverse;
        }

        tTimerOutput readTimerOutput(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getTimerOutput();
        }

        uint32_t readTimerOutput_Period(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getTimerOutput().Period;
        }

        int8_t readTimerOutput_Count(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getTimerOutput().Count;
        }

        bool readTimerOutput_Stalled(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getTimerOutput().Stalled;
        }

        void strobeReset(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->encoders[index].setOutput(*(new tOutput()));
            instance.first->encoders[index].setTimerOutput(*(new tTimerOutput()));
            instance.second.unlock();
        }

        void writeTimerConfig(tTimerConfig value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->encoders[index].setTimerConfig(value);
            instance.second.unlock();
        }

        void writeTimerConfig_StallPeriod(uint32_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tTimerConfig config = instance.first->encoders[index].getTimerConfig();
            config.StallPeriod = value;
            instance.first->encoders[index].setTimerConfig(config);
            instance.second.unlock();
        }

        void writeTimerConfig_AverageSize(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tTimerConfig config = instance.first->encoders[index].getTimerConfig();
            config.AverageSize = value;
            instance.first->encoders[index].setTimerConfig(config);
            instance.second.unlock();
        }

        void writeTimerConfig_UpdateWhenEmpty(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tTimerConfig config = instance.first->encoders[index].getTimerConfig();
            config.UpdateWhenEmpty = value;
            instance.first->encoders[index].setTimerConfig(config);
            instance.second.unlock();
        }

        tTimerConfig readTimerConfig(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getTimerConfig();
        }

        uint32_t readTimerConfig_StallPeriod(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getTimerConfig().StallPeriod;
        }

        uint8_t readTimerConfig_AverageSize(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getTimerConfig().AverageSize;
        }

        bool readTimerConfig_UpdateWhenEmpty(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->encoders[index].getTimerConfig().UpdateWhenEmpty;
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
