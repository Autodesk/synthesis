#include "roborio.hpp"
#include "util.hpp"
#include "error.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tDIO::tDO DigitalSystem::getOutputs()const{
        return outputs;
    }

    void DigitalSystem::setOutputs(tDIO::tDO value){
        outputs = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    tDIO::tOutputEnable DigitalSystem::getEnabledOutputs()const{
        return enabled_outputs;
    }

    void DigitalSystem::setEnabledOutputs(tDIO::tOutputEnable value){
        enabled_outputs = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    tDIO::tPulse DigitalSystem::getPulses()const{
        return pulses;
    }

    void DigitalSystem::setPulses(tDIO::tPulse value){
        pulses = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    tDIO::tDI DigitalSystem::getInputs()const{
        return inputs;
    }

    void DigitalSystem::setInputs(tDIO::tDI value){
        inputs = value;
    }

    uint16_t DigitalSystem::getMXPSpecialFunctionsEnabled()const{
    		return mxp_special_functions_enabled;
    }

    void DigitalSystem::setMXPSpecialFunctionsEnabled(uint16_t value){
        mxp_special_functions_enabled = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    uint8_t DigitalSystem::getPulseLength()const{
        return pulse_length;
    }

    void DigitalSystem::setPulseLength(uint8_t value){
        pulse_length = value;
    }

    uint8_t DigitalSystem::getPWMDutyCycle(uint8_t index)const{
        return pwm[index];
    }

    void DigitalSystem::setPWMDutyCycle(uint8_t index, uint8_t value){
        pwm[index] = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    DigitalSystem::DigitalSystem():
        outputs(),
        enabled_outputs(),
        pulses(),
        inputs(),
        mxp_special_functions_enabled(),
        pulse_length(),
        pwm()
    {}

    std::string to_string(DigitalSystem::DIOConfigurationException::Config c){
        switch(c){
        case DigitalSystem::DIOConfigurationException::Config::DI:
            return "digital input";
        case DigitalSystem::DIOConfigurationException::Config::DO:
            return "digital ouput";
        case DigitalSystem::DIOConfigurationException::Config::MXP_SPECIAL_FUNCTION:
            return "mxp special function";
        default:
            throw UnhandledEnumConstantException("hel::DIOManager::DIOConfigurationException::Config");
        }
    }

    const char* DigitalSystem::DIOConfigurationException::what()const throw(){
        std::string s = "Exception: digital IO failed attempting " + to_string(expected_configuration) + " but configured for " + to_string(configuration) + " on digital port " + std::to_string(port);
        return s.c_str();
    }

    DigitalSystem::DIOConfigurationException::DIOConfigurationException(Config config, Config expected, uint8_t p):configuration(config), expected_configuration(expected), port(p){}

    struct DIOManager: public tDIO{
        tSystemInterface* getSystemInterface() override{
            return nullptr;
        }

    private:
        template<typename T, typename S> //note: this is not an Ni FPGA function
        bool allowOutput(T output,S enabled, bool requires_special_function){
            auto instance = hel::RoboRIOManager::getInstance();
            for(unsigned i = 1; i < findMostSignificantBit(output); i++){
                if(!checkBitHigh(output, i)){
                    continue;
                }
                if(requires_special_function && !checkBitHigh(instance.first->digital_system.getMXPSpecialFunctionsEnabled(), i)){ //If it reqiores MXP special function, and it's not, don't allow output
                    instance.second.unlock();
                    throw DigitalSystem::DIOConfigurationException(DigitalSystem::DIOConfigurationException::Config::DO, DigitalSystem::DIOConfigurationException::Config::MXP_SPECIAL_FUNCTION, i);
                    return false;
                }
                if(!requires_special_function && !hel::checkBitHigh(enabled, i)){ //If output is set but output is not enabled, don't allow output (don't check if special function is enabled)
                    instance.second.unlock();
                    throw DigitalSystem::DIOConfigurationException(DigitalSystem::DIOConfigurationException::Config::DI, DigitalSystem::DIOConfigurationException::Config::DO, i);
                    return false;
                }
            }
            instance.second.unlock();
            return true;
        }

    public:

        void writeDO(tDIO::tDO value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            try{
                if(allowOutput(value.value, instance.first->digital_system.getEnabledOutputs().value, false)){
                    instance.first->digital_system.setOutputs(value);
                }
                instance.second.unlock();
            } catch(std::exception& e){
                instance.second.unlock();
                throw;
            }
        }

        void writeDO_Headers(uint16_t value, tRioStatusCode* status){
            auto instance = hel::RoboRIOManager::getInstance();
            tDIO::tDO outputs = instance.first->digital_system.getOutputs();
            outputs.Headers = value;
            writeDO(outputs, status);
            instance.second.unlock();
        }

        void writeDO_SPIPort(uint8_t value, tRioStatusCode* status){
            auto instance = hel::RoboRIOManager::getInstance();
            tDIO::tDO outputs = instance.first->digital_system.getOutputs();
            outputs.SPIPort = value;
            writeDO(outputs, status);
            instance.second.unlock();
        }

        void writeDO_Reserved(uint8_t value, tRioStatusCode* status){
            auto instance = hel::RoboRIOManager::getInstance();
            tDIO::tDO outputs = instance.first->digital_system.getOutputs();
            outputs.Reserved = value;
            writeDO(outputs, status);
            instance.second.unlock();
        }

        void writeDO_MXP(uint16_t value, tRioStatusCode* status){
            auto instance = hel::RoboRIOManager::getInstance();
            tDIO::tDO outputs = instance.first->digital_system.getOutputs();
            outputs.MXP = value;
            writeDO(outputs, status);
            instance.second.unlock();
        }

        tDO readDO(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs();
        }

        uint16_t readDO_Headers(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs().Headers;
        }

        uint8_t readDO_SPIPort(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs().SPIPort;
        }

        uint8_t readDO_Reserved(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs().Reserved;
        }

        uint16_t readDO_MXP(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getOutputs().MXP;
        }

        void writePWMDutyCycleA(uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            try{
                if(allowOutput(instance.first->digital_system.getEnabledOutputs().MXP, bitfield_index, true)){
                    instance.first->digital_system.setPWMDutyCycle(bitfield_index, value);
                }
                instance.second.unlock();
            } catch(std::exception& e){
                instance.second.unlock();
                throw;
            }
        }

        uint8_t readPWMDutyCycleA(uint8_t bitfield_index, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPWMDutyCycle(bitfield_index);
        }

        void writePWMDutyCycleB(uint8_t bitfield_index, uint8_t value, tRioStatusCode* status){
            writePWMDutyCycleA(bitfield_index, value, status); //no need to reimplement writePWMDutyCycleA, they do the same thing
        }

        uint8_t readPWMDutyCycleB(uint8_t bitfield_index, tRioStatusCode* status){
            return readPWMDutyCycleA(bitfield_index, status);
        }

        void writeFilterSelectHdr(uint8_t /*bitfield_index*/, uint8_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation


        uint8_t readFilterSelectHdr(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation (0 implies no active filter)
        }

        void writeOutputEnable(tDIO::tOutputEnable value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.first->digital_system.setEnabledOutputs(value);
            instance.second.unlock();
        }

        void writeOutputEnable_Headers(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.Headers = value;
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        void writeOutputEnable_SPIPort(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.SPIPort = value;
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        void writeOutputEnable_Reserved(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.Reserved = value;
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        void writeOutputEnable_MXP(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.MXP = value;
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        tOutputEnable readOutputEnable(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs();
        }

        uint16_t readOutputEnable_Headers(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs().Headers;
        }

        uint8_t readOutputEnable_SPIPort(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs().SPIPort;
        }

        uint8_t readOutputEnable_Reserved(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs().Reserved;
        }

        uint16_t readOutputEnable_MXP(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getEnabledOutputs().MXP;
        }

        void writePWMOutputSelect(uint8_t bitfield_index, uint8_t /*value*/, tRioStatusCode* /*status*/){
            //note: bitfield_index is mxp remapped dio address corresponding to the mxp pwm output
            auto instance = hel::RoboRIOManager::getInstance();
            tDIO::tOutputEnable enabled_outputs = instance.first->digital_system.getEnabledOutputs();
            enabled_outputs.MXP = hel::setBit(enabled_outputs.MXP, true, bitfield_index);
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        uint8_t readPWMOutputSelect(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation
        }

    private:
        void pulse(tPulse value){
            auto instance = hel::RoboRIOManager::getInstance();

            instance.first->digital_system.setPulses(value);
            uint8_t length = instance.first->digital_system.getPulseLength();

            instance.second.unlock();
            std::this_thread::sleep_for(std::chrono::milliseconds(length * 1000));
            instance.second.lock();

            instance.first->digital_system.setPulses(*(new tPulse));
            instance.second.unlock();
        }

    public:

        void writePulse(tPulse value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            if(instance.first->digital_system.getPulses().value != (new tPulse)->value){
                //TODO error handling multiple pulses active at once
            }
            try{
                if(allowOutput(value.value, instance.first->digital_system.getEnabledOutputs().value, false)){
                    instance.second.unlock();
                    std::thread(&DIOManager::pulse, this, value).detach();
                }
                instance.second.unlock();
            } catch(std::exception& e){
                instance.second.unlock();
                throw;
            }
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
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses();
        }

        uint16_t readPulse_Headers(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses().Headers;
        }
    
        uint8_t readPulse_SPIPort(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses().SPIPort;
        }
    
        uint8_t readPulse_Reserved(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses().Reserved;
        }
    
        uint16_t readPulse_MXP(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getPulses().MXP;
        }

        tDI readDI(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getInputs();
        }

        uint16_t readDI_Headers(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getInputs().Headers;
        }

        uint8_t readDI_SPIPort(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getInputs().SPIPort;
        }

        uint8_t readDI_Reserved(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getInputs().Reserved;
        }

        uint16_t readDI_MXP(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getInputs().MXP;
        }

        void writeEnableMXPSpecialFunction(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.first->digital_system.setMXPSpecialFunctionsEnabled(value);
            instance.second.unlock();
        }

        uint16_t readEnableMXPSpecialFunction(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->digital_system.getMXPSpecialFunctionsEnabled();
        }

        void writeFilterSelectMXP(uint8_t /*bitfield_index*/, uint8_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation

        uint8_t readFilterSelectMXP(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            return 0;//unnecessary for emulation
        }

        void writePulseLength(uint8_t value, tRioStatusCode* /*status*/){
            if(value > static_cast<uint8_t>(DigitalSystem::MAX_PULSE_LENGTH)){
                //TODO handle pulse request longer than max pulse length
            }
            auto instance = hel::RoboRIOManager::getInstance();
            instance.first->digital_system.setPulseLength(value);
            instance.second.unlock();
        }

        uint8_t readPulseLength(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
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

#ifdef DIGITAL_IO_TEST

int main(){
    uint16_t value = 1u << 3;
    tDIO* a = tDIO::create(nullptr);
    a->writeOutputEnable_Headers(1u << 3, nullptr);
    a->writeDO_Headers(value, nullptr);
}

#endif

