#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tCounter::tOutput RoboRIO::Counter::getOutput()const{
        return output;
    }

    void RoboRIO::Counter::setOutput(tCounter::tOutput out){
        output = out;
    }

    tCounter::tConfig RoboRIO::Counter::getConfig()const{
        return config;
    }

    void RoboRIO::Counter::setConfig(tCounter::tConfig c){
        config = c;
    }

    tCounter::tTimerOutput RoboRIO::Counter::getTimerOutput()const{
        return timer_output;
    }

    void RoboRIO::Counter::setTimerOutput(tCounter::tTimerOutput out){
        timer_output = out;
    }

    tCounter::tTimerConfig RoboRIO::Counter::getTimerConfig()const{
        return timer_config;
    }

    void RoboRIO::Counter::setTimerConfig(tCounter::tTimerConfig c){
        timer_config = c;
    }

    struct CounterManager: public tCounter{
    private:
        uint8_t index;

    public:
        CounterManager(uint8_t i):index(i){}

        tSystemInterface* getSystemInterface(){
            return nullptr;
        }

    	uint8_t getSystemIndex(){
            return index;
        }

        tOutput readOutput(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getOutput();
        }

        bool readOutput_Direction(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getOutput().Direction;
        }

        int32_t readOutput_Value(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getOutput().Value;
        }

    	void writeConfig(tConfig value, tRioStatusCode* /*status*/){
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(value);
        }

    	void writeConfig_UpSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.UpSource_Channel = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_UpSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.UpSource_Module = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_UpSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.UpSource_AnalogTrigger = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_DownSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.DownSource_Channel = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_DownSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.DownSource_Module = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

        void writeConfig_DownSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.DownSource_AnalogTrigger = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_IndexSource_Channel(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.IndexSource_Channel = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_IndexSource_Module(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.IndexSource_Module = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.IndexSource_AnalogTrigger = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_IndexActiveHigh(bool value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.IndexActiveHigh = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_IndexEdgeSensitive(bool value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.IndexEdgeSensitive = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_UpRisingEdge(bool value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.UpRisingEdge = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_UpFallingEdge(bool value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.UpFallingEdge = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_DownRisingEdge(bool value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.DownRisingEdge = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_DownFallingEdge(bool value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.DownFallingEdge = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_Mode(uint8_t value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.Mode = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	void writeConfig_PulseLengthThreshold(uint16_t value, tRioStatusCode* /*status*/){
            tConfig config = hel::RoboRIOManager::getInstance()->counters[index].getConfig();
            config.PulseLengthThreshold = value;
            hel::RoboRIOManager::getInstance()->counters[index].setConfig(config);
        }

    	tConfig readConfig(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig();
        }

    	uint8_t readConfig_UpSource_Channel(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().UpSource_Channel;
        }

    	uint8_t readConfig_UpSource_Module(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().UpSource_Module;
        }

    	bool readConfig_UpSource_AnalogTrigger(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().UpSource_AnalogTrigger;
        }

    	uint8_t readConfig_DownSource_Channel(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().DownSource_Channel;
        }

    	uint8_t readConfig_DownSource_Module(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().DownSource_Module;
        }

    	bool readConfig_DownSource_AnalogTrigger(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().DownSource_AnalogTrigger;
        }

    	uint8_t readConfig_IndexSource_Channel(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().IndexSource_Channel;
        }

    	uint8_t readConfig_IndexSource_Module(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().IndexSource_Module;
        }

    	bool readConfig_IndexSource_AnalogTrigger(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().IndexSource_AnalogTrigger;
        }

    	bool readConfig_IndexActiveHigh(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().IndexActiveHigh;
        }

    	bool readConfig_IndexEdgeSensitive(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().IndexEdgeSensitive;
        }

    	bool readConfig_UpRisingEdge(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().UpRisingEdge;
        }

    	bool readConfig_UpFallingEdge(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().UpFallingEdge;
        }

    	bool readConfig_DownRisingEdge(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().DownRisingEdge;
        }

    	bool readConfig_DownFallingEdge(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().DownFallingEdge;
        }

    	uint8_t readConfig_Mode(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().Mode;
        }

    	uint16_t readConfig_PulseLengthThreshold(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getConfig().PulseLengthThreshold;
        }

    	tTimerOutput readTimerOutput(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getTimerOutput();
        }

    	uint32_t readTimerOutput_Period(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getTimerOutput().Period;
        }

    	int8_t readTimerOutput_Count(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getTimerOutput().Count;
        }

    	bool readTimerOutput_Stalled(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getTimerOutput().Stalled;
        }

    	void strobeReset(tRioStatusCode* /*status*/){
            //resets counter
            tOutput output = hel::RoboRIOManager::getInstance()->counters[index].getOutput();
            output.Value = 0;
            hel::RoboRIOManager::getInstance()->counters[index].setOutput(output);
        }

    	void writeTimerConfig(tTimerConfig value, tRioStatusCode* /*status*/){
            hel::RoboRIOManager::getInstance()->counters[index].setTimerConfig(value);
        }

    	void writeTimerConfig_StallPeriod(uint32_t value, tRioStatusCode* /*status*/){
            tTimerConfig timer_config = hel::RoboRIOManager::getInstance()->counters[index].getTimerConfig();
            timer_config.StallPeriod = value;
            hel::RoboRIOManager::getInstance()->counters[index].setTimerConfig(timer_config);
        }

    	void writeTimerConfig_AverageSize(uint8_t value, tRioStatusCode* /*status*/){
            tTimerConfig timer_config = hel::RoboRIOManager::getInstance()->counters[index].getTimerConfig();
            timer_config.AverageSize = value;
            hel::RoboRIOManager::getInstance()->counters[index].setTimerConfig(timer_config);
        }

    	void writeTimerConfig_UpdateWhenEmpty(bool value, tRioStatusCode* /*status*/){
            tTimerConfig timer_config = hel::RoboRIOManager::getInstance()->counters[index].getTimerConfig();
            timer_config.UpdateWhenEmpty = value;
            hel::RoboRIOManager::getInstance()->counters[index].setTimerConfig(timer_config);
        }

    	tTimerConfig readTimerConfig(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getTimerConfig();
        }

    	uint32_t readTimerConfig_StallPeriod(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getTimerConfig().StallPeriod;
        }

    	uint8_t readTimerConfig_AverageSize(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getTimerConfig().AverageSize;
        }

    	bool readTimerConfig_UpdateWhenEmpty(tRioStatusCode* /*status*/){
            return hel::RoboRIOManager::getInstance()->counters[index].getTimerConfig().UpdateWhenEmpty;
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
