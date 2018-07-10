#include <thread>
#include <mutex>
#include <cmath>

#include <athena/DigitalInternal.h>

#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel {


    void RoboRIO::AnalogInputs::setConfig(tAI::tConfig value) {config = value;}
    tAI::tConfig RoboRIO::AnalogInputs::getConfig() {return config;}

    void RoboRIO::AnalogInputs::setReadSelect(tAI::tReadSelect value) {read_select = value;}
    tAI::tReadSelect RoboRIO::AnalogInputs::getReadSelect() {return read_select;}

    void RoboRIO::AnalogInputs::setOversampleBits(uint8_t channel, uint8_t value) {
        analog_inputs[channel].oversample_bits = value;
    }
    uint8_t RoboRIO::AnalogInputs::getOversampleBits(uint8_t channel) {
        return analog_inputs[channel].oversample_bits;
    }
    void RoboRIO::AnalogInputs::setAverageBits(uint8_t channel, uint8_t value) {
            analog_inputs[channel].average_bits = value;
    }
    uint8_t RoboRIO::AnalogInputs::getAverageBits(uint8_t channel) {
        return analog_inputs[channel].average_bits;
    }
    void RoboRIO::AnalogInputs::setScanList(uint8_t channel, uint8_t value) {
        analog_inputs[channel].scan_list = value;
    }
    uint8_t RoboRIO::AnalogInputs::getScanList(uint8_t channel) {
        return analog_inputs[channel].scan_list;
    }
    std::vector<int32_t> RoboRIO::AnalogInputs::getValues(uint8_t channel){
        return analog_inputs[channel].values;
    }
    void RoboRIO::AnalogInputs::setValues(uint8_t channel, std::vector<int32_t> values){
        analog_inputs[channel].values = values;
    }

    struct AnalogInputManager: public tAI{
        tSystemInterface* getSystemInterface(){
            return nullptr;
        }
        
        int32_t readOutput(tRioStatusCode* /*status*/){
            hel::RoboRIO::AnalogInputs analog_inputs = RoboRIOManager::getInstance()->analog_inputs;
            uint8_t channel = analog_inputs.getReadSelect().Channel;

            if(analog_inputs.getReadSelect().Averaged){
                float average = 0;
                int8_t start_index = std::max(
                    std::min(
                        (int)(analog_inputs.getValues(channel).size() - std::pow(2, analog_inputs.getAverageBits(channel) + analog_inputs.getOversampleBits(channel))),
                        (int)analog_inputs.getValues(channel).size()
                    ),
                    0
                );
                
                for(unsigned i = start_index; i < analog_inputs.getValues(channel).size(); i++){
                    average += analog_inputs.getValues(channel)[i];
                }

                average /= (analog_inputs.getValues(channel).size() - start_index);
                
                return (int32_t)average;
            }
            return analog_inputs.getValues(channel).back();
        }

        void writeConfig(hal::tAI::tConfig value, tRioStatusCode*) {
            RoboRIOManager::getInstance()->analog_inputs.setConfig(value);
        }

        void writeConfig_ScanSize(uint8_t value, tRioStatusCode*) {
            auto current_config = RoboRIOManager::getInstance()->analog_inputs.getConfig();
            current_config.ScanSize = value;
            RoboRIOManager::getInstance()->analog_inputs.setConfig(current_config);
        }

        void writeConfig_ConvertRate(uint32_t value, tRioStatusCode*) {
            auto current_config = RoboRIOManager::getInstance()->analog_inputs.getConfig();
            current_config.ConvertRate = value;
            RoboRIOManager::getInstance()->analog_inputs.setConfig(current_config);
        }

        hal::tAI::tConfig readConfig(tRioStatusCode*) {
            return RoboRIOManager::getInstance()->analog_inputs.getConfig();
        }

        uint8_t readConfig_ScanSize(tRioStatusCode*) {
            return RoboRIOManager::getInstance()->analog_inputs.getConfig().ScanSize;
        }

        uint32_t readConfig_ConvertRate(tRioStatusCode*) {
            return RoboRIOManager::getInstance()->analog_inputs.getConfig().ConvertRate;
        }

        void writeOversampleBits(uint8_t channel, uint8_t value, tRioStatusCode*) {
            RoboRIOManager::getInstance()->analog_inputs.setOversampleBits(channel, value);
        }
        void writeAverageBits(uint8_t channel, uint8_t value, tRioStatusCode*) {
            RoboRIOManager::getInstance()->analog_inputs.setAverageBits(channel, value);
        }
        void writeScanList(uint8_t channel, uint8_t value, tRioStatusCode*) {
            RoboRIOManager::getInstance()->analog_inputs.setScanList(channel, value);
        }

        uint8_t readOversampleBits(uint8_t channel, tRioStatusCode*) {
            return RoboRIOManager::getInstance()->analog_inputs.getOversampleBits(channel);
        }

        uint8_t readAverageBits(uint8_t channel, tRioStatusCode*) {
            return RoboRIOManager::getInstance()->analog_inputs.getAverageBits(channel);
        }

        uint8_t readScanList(uint8_t channel, tRioStatusCode*) {
            return RoboRIOManager::getInstance()->analog_inputs.getAverageBits(channel);
        }

        void writeReadSelect(hal::tAI::tReadSelect value, tRioStatusCode*) {
            RoboRIOManager::getInstance()->analog_inputs.setReadSelect(value);
        }

        void writeReadSelect_Channel(uint8_t value, tRioStatusCode*) {
            auto current_read_select = RoboRIOManager::getInstance()->analog_inputs.getReadSelect();
            current_read_select.Channel = value;
            RoboRIOManager::getInstance()->analog_inputs.setReadSelect(current_read_select);
        }

        void writeReadSelect_Averaged(bool value, tRioStatusCode*) {
            auto current_read_select = RoboRIOManager::getInstance()->analog_inputs.getReadSelect();
            current_read_select.Channel = value;
            RoboRIOManager::getInstance()->analog_inputs.setReadSelect(current_read_select);
        }

        hal::tAI::tReadSelect readReadSelect(tRioStatusCode*) {
            return RoboRIOManager::getInstance()->analog_inputs.getReadSelect();
        }

        uint8_t readReadSelect_Channel(tRioStatusCode*) {
            return RoboRIOManager::getInstance()->analog_inputs.getReadSelect().Channel;
        }
        bool readReadSelect_Averaged(tRioStatusCode*) {
            return RoboRIOManager::getInstance()->analog_inputs.getReadSelect().Averaged;
        }

        uint32_t readLoopTiming(tRioStatusCode*) {
            return hal::kExpectedLoopTiming;
        }

        void strobeLatchOutput(tRioStatusCode*) {}
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
    	tAI* tAI::create(tRioStatusCode* /*status*/){
    		return new hel::AnalogInputManager();
    	}
    }
}
