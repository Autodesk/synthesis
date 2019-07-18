#include "roborio_manager.hpp"
#include "system_interface.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    void Counter::reset()noexcept{
        zeroed_output = output;
    }

    tCounter::tOutput Counter::getCurrentOutput()const noexcept{
        tCounter::tOutput out = output;
        out.Value -= zeroed_output.Value;
        return out;
    }

    tCounter::tOutput Counter::getRawOutput()const noexcept{
        return output;
    }

    void Counter::setRawOutput(tCounter::tOutput out)noexcept{
        output = out;
    }

    tCounter::tConfig Counter::getConfig()const noexcept{
        return config;
    }

    void Counter::setConfig(tCounter::tConfig c)noexcept{
        config = c;
    }

    tCounter::tTimerOutput Counter::getTimerOutput()const noexcept{
        return timer_output;
    }

    void Counter::setTimerOutput(tCounter::tTimerOutput timer_out)noexcept{
        timer_output = timer_out;
    }

    tCounter::tTimerConfig Counter::getTimerConfig()const noexcept{
        return timer_config;
    }

    void Counter::setTimerConfig(tCounter::tTimerConfig timer_c)noexcept{
        timer_config = timer_c;
    }

    Counter::Counter()noexcept:output(),config(),timer_output(),timer_config(){}
    Counter::Counter(const Counter& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(output);
        COPY(config);
        COPY(timer_output);
        COPY(timer_config);
#undef COPY
    }

    struct CounterManager: public tCounter{
    private:
        uint8_t index;

    public:
        CounterManager(uint8_t i):index(0){
            assert(i < Counter::MAX_COUNTER_COUNT);
            index = i;
        }

        tSystemInterface* getSystemInterface(){
            return new SystemInterface();
        }

        uint8_t getSystemIndex(){
            return index;
        }

        tOutput readOutput(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getCurrentOutput();
        }

        bool readOutput_Direction(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getCurrentOutput().Direction;
        }

        int32_t readOutput_Value(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getCurrentOutput().Value;
        }

        void writeConfig(tConfig value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->counters[index].setConfig(value);
            instance.second.unlock();
        }

        void writeConfig_UpSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.UpSource_Channel = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_UpSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.UpSource_Module = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_UpSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.UpSource_AnalogTrigger = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_DownSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.DownSource_Channel = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_DownSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.DownSource_Module = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_DownSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.DownSource_AnalogTrigger = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.IndexSource_Channel = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.IndexSource_Module = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.IndexSource_AnalogTrigger = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexActiveHigh(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.IndexActiveHigh = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_IndexEdgeSensitive(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.IndexEdgeSensitive = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_UpRisingEdge(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.UpRisingEdge = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_UpFallingEdge(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.UpFallingEdge = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_DownRisingEdge(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.DownRisingEdge = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_DownFallingEdge(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.DownFallingEdge = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_Mode(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.Mode = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_PulseLengthThreshold(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tConfig config = instance.first->counters[index].getConfig();
            config.PulseLengthThreshold = value;
            instance.first->counters[index].setConfig(config);
            instance.second.unlock();
        }

        tConfig readConfig(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig();
        }

        uint8_t readConfig_UpSource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().UpSource_Channel;
        }

        uint8_t readConfig_UpSource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().UpSource_Module;
        }

        bool readConfig_UpSource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().UpSource_AnalogTrigger;
        }

        uint8_t readConfig_DownSource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().DownSource_Channel;
        }

        uint8_t readConfig_DownSource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().DownSource_Module;
        }

        bool readConfig_DownSource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().DownSource_AnalogTrigger;
        }

        uint8_t readConfig_IndexSource_Channel(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().IndexSource_Channel;
        }

        uint8_t readConfig_IndexSource_Module(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().IndexSource_Module;
        }

        bool readConfig_IndexSource_AnalogTrigger(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().IndexSource_AnalogTrigger;
        }

        bool readConfig_IndexActiveHigh(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().IndexActiveHigh;
        }

        bool readConfig_IndexEdgeSensitive(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().IndexEdgeSensitive;
        }

        bool readConfig_UpRisingEdge(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().UpRisingEdge;
        }

        bool readConfig_UpFallingEdge(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().UpFallingEdge;
        }

        bool readConfig_DownRisingEdge(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().DownRisingEdge;
        }

        bool readConfig_DownFallingEdge(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().DownFallingEdge;
        }

        uint8_t readConfig_Mode(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().Mode;
        }

        uint16_t readConfig_PulseLengthThreshold(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getConfig().PulseLengthThreshold;
        }

        tTimerOutput readTimerOutput(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getTimerOutput();
        }

        uint32_t readTimerOutput_Period(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getTimerOutput().Period;
        }

        int8_t readTimerOutput_Count(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getTimerOutput().Count;
        }

        bool readTimerOutput_Stalled(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getTimerOutput().Stalled;
        }

        void strobeReset(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->counters[index].reset();
            instance.second.unlock();
        }

        void writeTimerConfig(tTimerConfig value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->counters[index].setTimerConfig(value);
            instance.second.unlock();
        }

        void writeTimerConfig_StallPeriod(uint32_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tTimerConfig timer_config = instance.first->counters[index].getTimerConfig();
            timer_config.StallPeriod = value;
            instance.first->counters[index].setTimerConfig(timer_config);
            instance.second.unlock();
        }

        void writeTimerConfig_AverageSize(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tTimerConfig timer_config = instance.first->counters[index].getTimerConfig();
            timer_config.AverageSize = value;
            instance.first->counters[index].setTimerConfig(timer_config);
            instance.second.unlock();
        }

        void writeTimerConfig_UpdateWhenEmpty(bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tTimerConfig timer_config = instance.first->counters[index].getTimerConfig();
            timer_config.UpdateWhenEmpty = value;
            instance.first->counters[index].setTimerConfig(timer_config);
            instance.second.unlock();
        }

        tTimerConfig readTimerConfig(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getTimerConfig();
        }

        uint32_t readTimerConfig_StallPeriod(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getTimerConfig().StallPeriod;
        }

        uint8_t readTimerConfig_AverageSize(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getTimerConfig().AverageSize;
        }

        bool readTimerConfig_UpdateWhenEmpty(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->counters[index].getTimerConfig().UpdateWhenEmpty;
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tCounter* tCounter::create(uint8_t sys_index, tRioStatusCode* /*status*/){
            return new hel::CounterManager(sys_index);
        }
    }
}
