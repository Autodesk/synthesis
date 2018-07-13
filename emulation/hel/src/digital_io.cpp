#include "roborio.h"
#include "util.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tDIO::tDO RoboRIO::DIOSystem::getOutputs()const{
    	return outputs;
    }
    
    void RoboRIO::DIOSystem::setOutputs(tDIO::tDO value){
    	outputs = value;
    }

    tDIO::tOutputEnable RoboRIO::DIOSystem::getEnabledOutputs()const{
    	return enabled_outputs;
    }
    
    void RoboRIO::DIOSystem::setEnabledOutputs(tDIO::tOutputEnable value){
    	enabled_outputs = value;
    }

    tDIO::tPulse RoboRIO::DIOSystem::getPulses()const{
    	return pulses;
    }
    
    void RoboRIO::DIOSystem::setPulses(tDIO::tPulse value){
    	pulses = value;
    }

    tDIO::tDI RoboRIO::DIOSystem::getInputs()const{
    	return inputs;
    }

    void RoboRIO::DIOSystem::setInputs(tDIO::tDI value){
    	inputs = value;
    }

    uint16_t RoboRIO::DIOSystem::getMXPSpecialFunctionsEnabled()const{
    		return mxp_special_functions_enabled;
    }

    void RoboRIO::DIOSystem::setMXPSpecialFunctionsEnabled(uint16_t value){
    	mxp_special_functions_enabled = value;
    }

    uint8_t RoboRIO::DIOSystem::getPulseLength()const{
    	return pulse_length;
    }

    void RoboRIO::DIOSystem::setPulseLength(uint8_t value){
    	pulse_length = value;
    }

    uint8_t RoboRIO::DIOSystem::getPWMDutyCycle(uint8_t index)const{
    	return pwm[index];
    }

    void RoboRIO::DIOSystem::setPWMDutyCycle(uint8_t index, uint8_t value){
    	pwm[index] = value;
    }

    struct DIOManager: public tDIO{
        tSystemInterface* getSystemInterface() override{
            return nullptr;
        }
    
        void writeDO(tDIO::tDO value, tRioStatusCode* status){
            writeDO_Headers(value.Headers, status);
            writeDO_SPIPort(value.SPIPort, status);
            writeDO_Reserved(value.Reserved, status);
            writeDO_MXP(value.MXP, status);
        }

    private:
        template<typename T, typename S> //note: this is not an Ni FPGA function
        bool allowOutput(T output,S enabled, bool requires_special_function){
            auto instance = hel::RoboRIOManager::getInstance();
            for(unsigned i = 1; i < findMostSignificantBit(output); i++){
                if(checkBitHigh(output, i) && hel::checkBitHigh(enabled, i)){ //Attempt output if bit in value is high, allow write if enabled_outputs bit is also high 
                    bool special_enabled = checkBitHigh(instance.first->digital_system.getMXPSpecialFunctionsEnabled(), i);
                    instance.second.unlock();

                    return requires_special_function ? special_enabled : !special_enabled; //If it's DO, special function should be disabled. Otherwise, it should be enabled
                }
            }
            instance.second.unlock();
            return false;
        }

    public:

        void writeDO_Headers(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            if(allowOutput(value, instance.first->digital_system.getEnabledOutputs().Headers, false)){
                tDIO::tDO outputs = instance.first->digital_system.getOutputs();
                outputs.Headers = value;
                instance.first->digital_system.setOutputs(outputs);
            }
            instance.second.unlock();
            //TODO error handling
        }
    
        void writeDO_SPIPort(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            if(allowOutput(value, instance.first->digital_system.getEnabledOutputs().SPIPort, false)){
                auto instance = hel::RoboRIOManager::getInstance();
                tDIO::tDO outputs = instance.first->digital_system.getOutputs();
                outputs.SPIPort = value;
                instance.first->digital_system.setOutputs(outputs);
            }
            instance.second.unlock();
            //TODO error handling
        }
    
        void writeDO_Reserved(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            if(allowOutput(value, instance.first->digital_system.getEnabledOutputs().Reserved, false)){
                tDIO::tDO outputs = instance.first->digital_system.getOutputs();
                outputs.Reserved = value;
                instance.first->digital_system.setOutputs(outputs);
            }
            instance.second.unlock();
            //TODO error handling
        }
    
        void writeDO_MXP(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            if(allowOutput(value, instance.first->digital_system.getEnabledOutputs().MXP, false)){
                tDIO::tDO outputs = instance.first->digital_system.getOutputs();
                outputs.MXP = value;
                instance.first->digital_system.setOutputs(outputs);
            }
            instance.second.unlock();
            //TODO error handling
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
            if(allowOutput(instance.first->digital_system.getEnabledOutputs().MXP, bitfield_index, true)){
                instance.first->digital_system.setPWMDutyCycle(bitfield_index, value);
            } else {
                //TODO error handling
            }
            instance.second.unlock();
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
            enabled_outputs.MXP &= 1u << bitfield_index;
            instance.first->digital_system.setEnabledOutputs(enabled_outputs);
            instance.second.unlock();
        }

        uint8_t readPWMOutputSelect(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation
        }

        void writePulse(tDIO::tPulse value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.first->digital_system.setPulses(value);
            //TODO this should only last for pulse_length seconds, and only one pulse should be active at a time? Also need to use allowOutput() too
            instance.second.unlock();
        }

        void writePulse_Headers(uint16_t /*value*/, tRioStatusCode* /*status*/){
            //TODO
        }

        void writePulse_SPIPort(uint8_t /*value*/, tRioStatusCode* /*status*/){
            //TODO
        }

        void writePulse_Reserved(uint8_t /*value*/, tRioStatusCode* /*status*/){
            //TODO
        }

        void writePulse_MXP(uint16_t /*value*/, tRioStatusCode* /*status*/){
            //TODO
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

