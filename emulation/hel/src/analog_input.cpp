#include <thread>
#include <mutex>

#include <athena/DigitalInternal.h>

#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

struct AnalogInputManager: public tAI{
	tSystemInterface* getSystemInterface(){
		return nullptr;
	}
	int32_t readOutput(tRioStatusCode* /*status*/){
		//TODO
	}

    void writeConfig(hal::tAI::tConfig value, tRioStatusCode*) {
        hel::roborio_state.analog_inputs.setConfig(value);
    }

    void writeConfig_ScanSize(uint8_t value, tRioStatusCode*) {
        auto current_config = hel::roborio_state.analog_inputs.getConfig();
        current_config.ScanSize = value;
        hel::roborio_state.analog_inputs.setConfig(current_config);
    }

    void writeConfig_ConvertRate(uint32_t value, tRioStatusCode*) {
        auto current_config = hel::roborio_state.analog_inputs.getConfig();
        current_config.ConvertRate = value;
        hel::roborio_state.analog_inputs.setConfig(current_config);
    }

    hal::tAI::tConfig readConfig(tRioStatusCode*) {
        return hel::roborio_state.analog_inputs.getConfig();
    }

    uint8_t readConfig_ScanSize(tRioStatusCode*) {
        return hel::roborio_state.analog_inputs.getConfig().ScanSize;
    }

    uint32_t readConfig_ConvertRate(tRioStatusCode*) {
        return hel::roborio_state.analog_inputs.getConfig().ConvertRate;
    }

    void writeOversampleBits(uint8_t channel, uint8_t value, tRioStatusCode*) {
        hel::roborio_state.analog_inputs.setOversampleBits(channel, value);
    }
    void writeAverageBits(uint8_t channel, uint8_t value, tRioStatusCode*) {
        hel::roborio_state.analog_inputs.setAverageBits(channel, value);
    }
    void writeScanList(uint8_t channel, uint8_t value, tRioStatusCode*) {
        hel::roborio_state.analog_inputs.setScanList(channel, value);
    }

    uint8_t readOversampleBits(uint8_t channel, tRioStatusCode*) {
        return hel::roborio_state.analog_inputs.getOversampleBits(channel);
    }

    uint8_t readAverageBits(uint8_t channel, tRioStatusCode*) {
        return hel::roborio_state.analog_inputs.getAverageBits(channel);
    }

    uint8_t readScanList(uint8_t channel, tRioStatusCode*) {
        return hel::roborio_state.analog_inputs.getAverageBits(channel);
    }

    void writeReadSelect(hal::tAI::tReadSelect value, tRioStatusCode*) {
        hel::roborio_state.analog_inputs.setReadSelect(value);
    }

    void writeReadSelect_Channel(uint8_t value, tRioStatusCode*) {
        auto current_read_select = hel::roborio_state.analog_inputs.getReadSelect();
        current_read_select.Channel = value;
        hel::roborio_state.analog_inputs.setReadSelect(current_read_select);
    }

    void writeReadSelect_Averaged(bool value, tRioStatusCode*) {
        auto current_read_select = hel::roborio_state.analog_inputs.getReadSelect();
        current_read_select.Channel = value;
        hel::roborio_state.analog_inputs.setReadSelect(current_read_select);
    }

    hal::tAI::tReadSelect readReadSelect(tRioStatusCode*) {
        return hel::roborio_state.analog_inputs.getReadSelect();
    }

    uint8_t readReadSelect_Channel(tRioStatusCode*) {
        return hel::roborio_state.analog_inputs.getReadSelect().Channel;
    }
    bool readReadSelect_Averaged(tRioStatusCode*) {
            return hel::roborio_state.analog_inputs.getReadSelect().Averaged;
    }

    uint32_t readLoopTiming(tRioStatusCode*) {
        return hal::kExpectedLoopTiming;
    }

    void strobeLatchOutput(tRioStatusCode*) {}
};

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		tAI* tAI::create(tRioStatusCode* /*status*/){
			return new AnalogInputManager();
		}
	}
}

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

}

#ifdef ANALOG_INPUT_TEST

int main(){
	//tAI* a = tAI::create(nullptr);
}

#endif
