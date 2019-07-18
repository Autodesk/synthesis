#include "roborio_manager.hpp"
#include "system_interface.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    void FPGAEncoder::reset()noexcept{
        zeroed_output = output;
    }

    tEncoder::tOutput FPGAEncoder::getRawOutput()const noexcept{
        return output;
    }

    tEncoder::tOutput FPGAEncoder::getCurrentOutput()const noexcept{
        tEncoder::tOutput out = output;
        out.Value -= zeroed_output.Value;
        return out;
    }

    void FPGAEncoder::setRawOutput(tEncoder::tOutput out)noexcept{
        output = out;
    }

    tEncoder::tConfig FPGAEncoder::getConfig()const noexcept{
        return config;
    }

    void FPGAEncoder::setConfig(tEncoder::tConfig c)noexcept{
        config = c;
    }

    tEncoder::tTimerOutput FPGAEncoder::getTimerOutput()const noexcept{
        return timer_output;
    }

    void FPGAEncoder::setTimerOutput(tEncoder::tTimerOutput timer_out)noexcept{
        timer_output = timer_out;
    }

    tEncoder::tTimerConfig FPGAEncoder::getTimerConfig()const noexcept{
        return timer_config;
    }

    void FPGAEncoder::setTimerConfig(tEncoder::tTimerConfig timer_c)noexcept{
        timer_config = timer_c;
    }

    FPGAEncoder::FPGAEncoder()noexcept:output(),config(),timer_output(),timer_config(){}
    FPGAEncoder::FPGAEncoder(const FPGAEncoder& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(output);
        COPY(config);
        COPY(timer_output);
        COPY(timer_config);
#undef COPY
    }

    struct FPGAEncoderManager: public tEncoder{
    private:
        uint8_t index;

    public:
        tSystemInterface* getSystemInterface(){ //unnecessary for emulation
            return new SystemInterface();
        }

        uint8_t getSystemIndex(){
            return index;
        }

        tOutput readOutput(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getCurrentOutput();
        }

        bool readOutput_Direction(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getCurrentOutput().Direction;
        }

        int32_t readOutput_Value(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getCurrentOutput().Value;
        }

        void writeConfig(tConfig value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->fpga_encoders[index].setConfig(value);
            instance.second.unlock();
        }

        void writeConfig_ASource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.ASource_Channel = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_ASource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.ASource_Module = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_ASource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.ASource_AnalogTrigger = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_BSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.BSource_Channel = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_BSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.BSource_Module = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_BSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.BSource_AnalogTrigger = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.IndexSource_Channel = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.IndexSource_Module = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.IndexSource_AnalogTrigger = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexActiveHigh(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.IndexActiveHigh = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexEdgeSensitive(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.IndexEdgeSensitive = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_Reverse(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->fpga_encoders[index].getConfig();
            config.Reverse = value;
            instance.first->fpga_encoders[index].setConfig(config);
            instance.second.unlock();
        }

        tConfig readConfig(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig();
        }

        uint8_t readConfig_ASource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().ASource_Channel;
        }

        uint8_t readConfig_ASource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().ASource_Module;
        }

        bool readConfig_ASource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().ASource_AnalogTrigger;
        }

        uint8_t readConfig_BSource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().BSource_Channel;
        }

        uint8_t readConfig_BSource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().BSource_Module;
        }

        bool readConfig_BSource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().BSource_AnalogTrigger;
        }

        uint8_t readConfig_IndexSource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().IndexSource_Channel;
        }

        uint8_t readConfig_IndexSource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().IndexSource_Module;
        }

        bool readConfig_IndexSource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().IndexSource_AnalogTrigger;
        }

        bool readConfig_IndexActiveHigh(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().IndexActiveHigh;
        }

        bool readConfig_IndexEdgeSensitive(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().IndexEdgeSensitive;
        }

        bool readConfig_Reverse(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getConfig().Reverse;
        }

        tTimerOutput readTimerOutput(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getTimerOutput();
        }

        uint32_t readTimerOutput_Period(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getTimerOutput().Period;
        }

        int8_t readTimerOutput_Count(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getTimerOutput().Count;
        }

        bool readTimerOutput_Stalled(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getTimerOutput().Stalled;
        }

        void strobeReset(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->fpga_encoders[index].reset();
            instance.second.unlock();
        }

        void writeTimerConfig(tTimerConfig value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->fpga_encoders[index].setTimerConfig(value);
            instance.second.unlock();
        }

        void writeTimerConfig_StallPeriod(uint32_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tTimerConfig config = instance.first->fpga_encoders[index].getTimerConfig();
            config.StallPeriod = value;
            instance.first->fpga_encoders[index].setTimerConfig(config);
            instance.second.unlock();
        }

        void writeTimerConfig_AverageSize(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tTimerConfig config = instance.first->fpga_encoders[index].getTimerConfig();
            config.AverageSize = value;
            instance.first->fpga_encoders[index].setTimerConfig(config);
            instance.second.unlock();
        }

        void writeTimerConfig_UpdateWhenEmpty(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tTimerConfig config = instance.first->fpga_encoders[index].getTimerConfig();
            config.UpdateWhenEmpty = value;
            instance.first->fpga_encoders[index].setTimerConfig(config);
            instance.second.unlock();
        }

        tTimerConfig readTimerConfig(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getTimerConfig();
        }

        uint32_t readTimerConfig_StallPeriod(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getTimerConfig().StallPeriod;
        }

        uint8_t readTimerConfig_AverageSize(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getTimerConfig().AverageSize;
        }

        bool readTimerConfig_UpdateWhenEmpty(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->fpga_encoders[index].getTimerConfig().UpdateWhenEmpty;
        }

        FPGAEncoderManager(uint8_t i):index(0){
            assert(i < FPGAEncoder::NUM_ENCODERS);
            index = i;
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tEncoder* tEncoder::create(uint8_t sys_index, tRioStatusCode* /*status*/){
            return new hel::FPGAEncoderManager(sys_index);
        }
    }
}
