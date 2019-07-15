#include <mutex>
#include <cmath>

#include "roborio_manager.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel {
    void AnalogInputs::setConfig(tAI::tConfig value)noexcept{
        config = value;
    }

    tAI::tConfig AnalogInputs::getConfig()noexcept{
        return config;
    }

    void AnalogInputs::setReadSelect(tAI::tReadSelect value)noexcept{
        read_select = value;
    }

    tAI::tReadSelect AnalogInputs::getReadSelect()noexcept{
        return read_select;
    }

    void AnalogInputs::setOversampleBits(uint8_t channel, uint8_t value){
        analog_inputs[channel].oversample_bits = value;
    }

    uint8_t AnalogInputs::getOversampleBits(uint8_t channel){
        return analog_inputs[channel].oversample_bits;
    }

    void AnalogInputs::setAverageBits(uint8_t channel, uint8_t value){
        analog_inputs[channel].average_bits = value;
    }

    uint8_t AnalogInputs::getAverageBits(uint8_t channel){
        return analog_inputs[channel].average_bits;
    }

    void AnalogInputs::setScanList(uint8_t channel, uint8_t value){
        analog_inputs[channel].scan_list = value;
    }

    uint8_t AnalogInputs::getScanList(uint8_t channel){
        return analog_inputs[channel].scan_list;
    }

    std::vector<int32_t> AnalogInputs::getValues(uint8_t channel){
        return analog_inputs[channel].values;
    }

    void AnalogInputs::setValues(uint8_t channel, std::vector<int32_t> values){
        analog_inputs[channel].values = values;
    }

    AnalogInputs::AnalogInput::AnalogInput()noexcept:oversample_bits(0),average_bits(0),scan_list(0),values({}){}

    AnalogInputs::AnalogInput::AnalogInput(const AnalogInput& source)noexcept:AnalogInput(){
#define COPY(NAME) NAME = source.NAME
        COPY(oversample_bits);
        COPY(average_bits);
        COPY(values);
#undef COPY
    }

    AnalogInputs::AnalogInputs()noexcept:analog_inputs({}),config(),read_select(){}

    AnalogInputs::AnalogInputs(const AnalogInputs& source)noexcept:AnalogInputs(){
#define COPY(NAME) NAME = source.NAME
        COPY(analog_inputs);
        COPY(config);
        COPY(read_select);
#undef COPY
    }

    struct AnalogInputManager: public tAI{
        tSystemInterface* getSystemInterface(){
            return nullptr;
        }

        int32_t readOutput(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            AnalogInputs analog_inputs = instance.first->analog_inputs;
            uint8_t channel = analog_inputs.getReadSelect().Channel;

            if(analog_inputs.getValues(channel).empty()){
                return 0;
            }

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
            instance.second.unlock();
            return analog_inputs.getValues(channel).back();
        }

        void writeConfig(tAI::tConfig value, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.first->analog_inputs.setConfig(value);
            instance.second.unlock();
        }

        void writeConfig_ScanSize(uint8_t value, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            auto current_config = instance.first->analog_inputs.getConfig();
            current_config.ScanSize = value;
            instance.first->analog_inputs.setConfig(current_config);
            instance.second.unlock();
        }

        void writeConfig_ConvertRate(uint32_t value, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            auto current_config = instance.first->analog_inputs.getConfig();
            current_config.ConvertRate = value;
            instance.first->analog_inputs.setConfig(current_config);
            instance.second.unlock();
        }

        tAI::tConfig readConfig(tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_inputs.getConfig();
        }

        uint8_t readConfig_ScanSize(tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_inputs.getConfig().ScanSize;
        }

        uint32_t readConfig_ConvertRate(tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_inputs.getConfig().ConvertRate;
        }

        void writeOversampleBits(uint8_t channel, uint8_t value, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.first->analog_inputs.setOversampleBits(channel, value);
            instance.second.unlock();
        }
        void writeAverageBits(uint8_t channel, uint8_t value, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.first->analog_inputs.setAverageBits(channel, value);
            instance.second.unlock();
        }
        void writeScanList(uint8_t channel, uint8_t value, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.first->analog_inputs.setScanList(channel, value);
            instance.second.unlock();
        }

        uint8_t readOversampleBits(uint8_t channel, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_inputs.getOversampleBits(channel);
        }

        uint8_t readAverageBits(uint8_t channel, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_inputs.getAverageBits(channel);
        }

        uint8_t readScanList(uint8_t channel, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_inputs.getScanList(channel);
        }

        void writeReadSelect(tAI::tReadSelect value, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.first->analog_inputs.setReadSelect(value);
            instance.second.unlock();
        }

        void writeReadSelect_Channel(uint8_t value, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            auto current_read_select = instance.first->analog_inputs.getReadSelect();
            current_read_select.Channel = value;
            instance.first->analog_inputs.setReadSelect(current_read_select);
            instance.second.unlock();
        }

        void writeReadSelect_Averaged(bool value, tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            auto current_read_select = instance.first->analog_inputs.getReadSelect();
            current_read_select.Channel = value;
            instance.first->analog_inputs.setReadSelect(current_read_select);
            instance.second.unlock();
        }

        tAI::tReadSelect readReadSelect(tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_inputs.getReadSelect();
        }

        uint8_t readReadSelect_Channel(tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_inputs.getReadSelect().Channel;
        }
        bool readReadSelect_Averaged(tRioStatusCode*) {
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_inputs.getReadSelect().Averaged;
        }

        uint32_t readLoopTiming(tRioStatusCode*) {
            return PWMSystem::EXPECTED_LOOP_TIMING; //this is probably fine
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
