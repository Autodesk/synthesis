#include "roborio_manager.hpp"
#include "robot_outputs.hpp"
#include "system_interface.hpp"
#include "util.hpp"

#include <unistd.h>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tDIO::tDO DigitalSystem::getOutputs()const noexcept{
        return outputs;
    }

    void DigitalSystem::setOutputs(tDIO::tDO out)noexcept{
        outputs = out;
        auto instance = RobotOutputsManager::getInstance();
        instance.first->updateDeep();
        instance.second.unlock();
    }

    tDIO::tOutputEnable DigitalSystem::getEnabledOutputs()const noexcept{
        return enabled_outputs;
    }

    void DigitalSystem::setEnabledOutputs(tDIO::tOutputEnable enabled_out)noexcept{
        enabled_outputs = enabled_out;
        auto instance = RobotOutputsManager::getInstance();
        instance.first->updateDeep();
        instance.second.unlock();
    }

    tDIO::tPulse DigitalSystem::getPulses()const noexcept{
        return pulses;
    }

    void DigitalSystem::setPulses(tDIO::tPulse value)noexcept{
        pulses = value;
        auto instance = RobotOutputsManager::getInstance();
        instance.first->updateDeep();
        instance.second.unlock();
    }

    tDIO::tDI DigitalSystem::getInputs()const noexcept{
        return inputs;
    }

    void DigitalSystem::setInputs(tDIO::tDI in)noexcept{
        inputs = in;
    }

    uint16_t DigitalSystem::getMXPSpecialFunctionsEnabled()const noexcept{
        return mxp_special_functions_enabled;
    }

    void DigitalSystem::setMXPSpecialFunctionsEnabled(uint16_t enabled_mxp_special_functions)noexcept{
        mxp_special_functions_enabled = enabled_mxp_special_functions;
        auto instance = RobotOutputsManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

	void DigitalSystem::setMXPConfig(const unsigned index){
		mxp_configurations[index] = [&](){
			if(checkBitHigh(getMXPSpecialFunctionsEnabled(), index)){
				if(
				   index == 4 || index == 5 ||
				   index == 6 || index == 7
				   ){
					return MXPData::Config::SPI;
				}
				if(index == 14 || index == 15){
					return MXPData::Config::I2C;
				}
				/* One of these must be true if the above are false
				   index == 0  || index == 1  ||
				   index == 2  || index == 3  ||
				   index == 8  || index == 9  ||
				   index == 10 || index == 11 ||
				   index == 12 || index == 13
				*/
				return MXPData::Config::PWM;
			}
			if(checkBitHigh(getEnabledOutputs().MXP,index)){
				return MXPData::Config::DO;
			}
			return MXPData::Config::DI;
		}();
	}

	MXPData::Config DigitalSystem::getMXPConfig(const unsigned i)const{
		return mxp_configurations[i];
	}

    uint8_t DigitalSystem::getPulseLength()const noexcept{
        return pulse_length;
    }

    void DigitalSystem::setPulseLength(uint8_t length)noexcept{
        pulse_length = length;
    }

    uint8_t DigitalSystem::getPWMPulseWidth(uint8_t index)const{
        return pwm[index];
    }

    void DigitalSystem::setPWMPulseWidth(uint8_t index, uint8_t pulse_width){
        pwm[index] = pulse_width;
    }

    DigitalSystem::DigitalSystem()noexcept:
        outputs(),
        enabled_outputs(),
        pulses(),
        inputs(),
        mxp_special_functions_enabled(0),
        pulse_length(0),
        pwm(0),
		mxp_configurations(MXPData::Config::DI)
    {}

    DigitalSystem::DigitalSystem(const DigitalSystem& source)noexcept:DigitalSystem(){
#define COPY(NAME) NAME = source.NAME
        COPY(outputs);
        COPY(enabled_outputs);
        COPY(pulses);
        COPY(inputs);
        COPY(mxp_special_functions_enabled);
        COPY(pulse_length);
        COPY(pwm);
#undef COPY
    }

    std::string asString(DigitalSystem::DIOConfigurationException::Config config){
        switch(config){
        case DigitalSystem::DIOConfigurationException::Config::DI:
            return "digital input";
        case DigitalSystem::DIOConfigurationException::Config::DO:
            return "digital output";
        case DigitalSystem::DIOConfigurationException::Config::MXP_SPECIAL_FUNCTION:
            return "mxp special function";
        default:
            throw UnhandledEnumConstantException("hel::DIOManager::DIOConfigurationException::Config");
        }
    }

	std::string asString(DigitalSystem::HeaderConfig config){
        switch(config){
        case DigitalSystem::HeaderConfig::DI:
            return "DI";
        case DigitalSystem::HeaderConfig::DO:
            return "DO";
        default:
            throw UnhandledEnumConstantException("hel::DIOManager::HeaderConfig");
        }
    }

    const char* DigitalSystem::DIOConfigurationException::what()const throw(){
        return makeExceptionMessage("Digital IO failed attempting " + asString(expected_configuration) + " but configured for " + asString(actual_configuration) + " on digital port " + std::to_string(port)).c_str();
    }

    DigitalSystem::DIOConfigurationException::DIOConfigurationException(Config expected, Config actual, uint8_t index)noexcept: actual_configuration(actual), expected_configuration(expected), port(index){}

    struct DIOManager: public tDIO{
        tSystemInterface* getSystemInterface() override{
            return new SystemInterface();
        }

    private:
        template<typename T>
        void ensureDigitalOutputAllowed(T output){
            auto instance = RoboRIOManager::getInstance();
			for(int i = 0; i <= findMostSignificantBit(output.MXP); i++){
				if(checkBitHigh(output.MXP, i) && instance.first->digital_system.getMXPConfig(i) != MXPData::Config::DO){
					throw DigitalSystem::DIOConfigurationException(DigitalSystem::DIOConfigurationException::Config::DO, DigitalSystem::DIOConfigurationException::Config::MXP_SPECIAL_FUNCTION, i + DigitalSystem::NUM_DIGITAL_HEADERS);
				}
			}
			for(int i = 0; i <= findMostSignificantBit(output.Headers); i++){
				if(checkBitHigh(output.Headers, i) && checkBitLow(instance.first->digital_system.getEnabledOutputs().Headers, i)){
					throw DigitalSystem::DIOConfigurationException(DigitalSystem::DIOConfigurationException::Config::DO, DigitalSystem::DIOConfigurationException::Config::DI, i);
				}
			}
		}

    public:

        void writeDO(tDIO::tDO value, tRioStatusCode* /*status*/){
			ensureDigitalOutputAllowed(value);
			auto instance = RoboRIOManager::getInstance();
			instance.first->digital_system.setOutputs(value);
			instance.second.unlock();
        }

        void writeDO_Headers(uint16_t value, tRioStatusCode* status){
            auto instance = RoboRIOManager::getInstance();
            tDIO::tDO outputs = instance.first->digital_system.getOutputs();
            outputs.Headers = value;
            writeDO(outputs, status);
            instance.second.unlock();
        }

        void writeDO_SPIPort(uint8_t value, tRioStatusCode* status){
            auto instance = RoboRIOManager::getInstance();
            tDIO::tDO outputs = instance.first->digital_system.getOutputs();
            outputs.SPIPort = value;
            writeDO(outputs, status);
            instance.second.unlock();
        }

        void writeDO_Reserved(uint8_t value, tRioStatusCode* status){
            auto instance = RoboRIOManager::getInstance();
            tDIO::tDO outputs = instance.first->digital_system.getOutputs();
            outputs.Reserved = value;
            writeDO(outputs, status);
            instance.second.unlock();
        }

        void writeDO_MXP(uint16_t value, tRioStatusCode* status){
            auto instance = RoboRIOManager::getInstance();
            tDIO::tDO outputs = instance.first->digital_system.getOutputs();
            outputs.MXP = value;
            writeDO(outputs, status);
            instance.second.unlock();
        }

        tDO readDO(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs();
        }

        uint16_t readDO_Headers(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs().Headers;
        }

        uint8_t readDO_SPIPort(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs().SPIPort;
        }

        uint8_t readDO_Reserved(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs().Reserved;
        }

        uint16_t readDO_MXP(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs().MXP;
        }

        void writePWMDutyCycleA(uint8_t /*bitfield_index*/, uint8_t /*value*/, tRioStatusCode* /*status*/){
            warnUnsupportedFeature("Function call tDIO::writePWMDutyCycleA");
        }

        uint8_t readPWMDutyCycleA(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            warnUnsupportedFeature("Function call tDIO::readPWMDutyCycleA");
            return 0;
        }

        void writePWMDutyCycleB(uint8_t /*bitfield_index*/, uint8_t /*value*/, tRioStatusCode* /*status*/){
            warnUnsupportedFeature("Function call tDIO::writePWMDutyCycleB");
            //no need to reimplement writePWMDutyCycleA, they do the same thing
        }

        uint8_t readPWMDutyCycleB(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            warnUnsupportedFeature("Function call tDIO::readPWMDutyCycleB");
            //no need to reimplement readPWMDutyCycleA, they do the same thing
            return 0;
        }

        void writeFilterSelectHdr(uint8_t /*bitfield_index*/, uint8_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation


        uint8_t readFilterSelectHdr(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation (0 implies no active filter)
        }

        void writeOutputEnable(tDIO::tOutputEnable value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->digital_system.setEnabledOutputs(value);
            for(int i = 0; i <= findMostSignificantBit(value.MXP); i++){
				instance.first->digital_system.setMXPConfig(i);
            }
            instance.second.unlock();
        }

        void writeOutputEnable_Headers(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.Headers = value;
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        void writeOutputEnable_SPIPort(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.SPIPort = value;
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        void writeOutputEnable_Reserved(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.Reserved = value;
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        void writeOutputEnable_MXP(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.MXP = value;
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        tOutputEnable readOutputEnable(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs();
        }

        uint16_t readOutputEnable_Headers(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs().Headers;
        }

        uint8_t readOutputEnable_SPIPort(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs().SPIPort;
        }

        uint8_t readOutputEnable_Reserved(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs().Reserved;
        }

        uint16_t readOutputEnable_MXP(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs().MXP;
        }

        void writePWMOutputSelect(uint8_t bitfield_index, uint8_t /*value*/, tRioStatusCode* /*status*/){
            //note: bitfield_index is mxp remapped dio address corresponding to the mxp pwm output
            auto instance = RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.MXP = setBit(enabled_outputs.MXP, true, bitfield_index);
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        uint8_t readPWMOutputSelect(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation
        }

    private:
        void pulse(tPulse value, uint8_t length){
            auto instance = RoboRIOManager::getInstance();
            instance.first->digital_system.setPulses(value);
            instance.second.unlock();

            usleep(length);

			tPulse new_pulse;
			new_pulse.value = 0;

			instance.second.lock();
            instance.first->digital_system.setPulses(new_pulse);
            instance.second.unlock();
        }

    public:

        void writePulse(tPulse value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            if(instance.first->digital_system.getPulses().value != 0){
                warn("Multiple digital output pulses are not allowed at once");
                return;
            }
			ensureDigitalOutputAllowed(value);
			instance.second.unlock();
			std::thread(&DIOManager::pulse, this, value, instance.first->digital_system.getPulseLength()).detach();
        }

        void writePulse_Headers(uint16_t value, tRioStatusCode* status){
            tPulse pulse;
            pulse.Headers = value;
            writePulse(pulse, status);
        }

        void writePulse_SPIPort(uint8_t value, tRioStatusCode* status){
            tPulse pulse;
            pulse.SPIPort = value;
            writePulse(pulse, status);
        }

        void writePulse_Reserved(uint8_t value, tRioStatusCode* status){
            tPulse pulse;
            pulse.Reserved = value;
            writePulse(pulse, status);
        }

        void writePulse_MXP(uint16_t value, tRioStatusCode* status){
            tPulse pulse;
            pulse.MXP = value;
            writePulse(pulse, status);
        }

        tPulse readPulse(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses();
        }

        uint16_t readPulse_Headers(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses().Headers;
        }

        uint8_t readPulse_SPIPort(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses().SPIPort;
        }

        uint8_t readPulse_Reserved(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses().Reserved;
        }

        uint16_t readPulse_MXP(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses().MXP;
        }

        tDI readDI(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
			return instance.first->digital_system.getInputs();
        }

        uint16_t readDI_Headers(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getInputs().Headers;
        }

        uint8_t readDI_SPIPort(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getInputs().SPIPort;
        }

        uint8_t readDI_Reserved(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getInputs().Reserved;
        }

        uint16_t readDI_MXP(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getInputs().MXP;
        }

        void writeEnableMXPSpecialFunction(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->digital_system.setMXPSpecialFunctionsEnabled(value);

			writeDO_MXP(this->readDO_MXP(nullptr) & ~value, nullptr); // HAL enables MXP special functions for DIOs when they're freed, so here we'll just reset the digital output for them

			for(int i = 0; i <= findMostSignificantBit(value); i++){
				instance.first->digital_system.setMXPConfig(i);
				const MXPData::Config& config = instance.first->digital_system.getMXPConfig(i);
                if(
				   config == MXPData::Config::I2C ||
				   config == MXPData::Config::SPI
				 ){
					warnUnsupportedFeature("Configuring digital MXP DIO " + std::to_string(i) + " for " + asString(config));
                }
            }

            instance.second.unlock();
        }

        uint16_t readEnableMXPSpecialFunction(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
			return instance.first->digital_system.getMXPSpecialFunctionsEnabled();
        }

        void writeFilterSelectMXP(uint8_t /*bitfield_index*/, uint8_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation

        uint8_t readFilterSelectMXP(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            return 0;//unnecessary for emulation
        }

        void writePulseLength(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->digital_system.setPulseLength(value);
            instance.second.unlock();
        }

        uint8_t readPulseLength(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulseLength();
        }

        void writePWMPeriodPower(uint16_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation


        uint16_t readPWMPeriodPower(tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation
        }

        void writeFilterPeriodMXP(uint8_t /*reg_index*/, uint32_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation


        uint32_t readFilterPeriodMXP(uint8_t /*reg_index*/, tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation
        }

        void writeFilterPeriodHdr(uint8_t /*reg_index*/, uint32_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation

        uint32_t readFilterPeriodHdr(uint8_t /*reg_index*/, tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation
        }

    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tDIO* tDIO::create(tRioStatusCode* /*status*/){
            return new hel::DIOManager();
        }
    }
}
