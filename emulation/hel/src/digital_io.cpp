#include "roborio.h"

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

	void writeDO_Headers(uint16_t value, tRioStatusCode* /*status*/){
		for(uint32_t i = 1; i <= value; i <<= 1){ 
			if(value & i){ // attempt output if bit in value is high
				if(hel::roborio_state.digital_system.getEnabledOutputs().Headers & i){ //allow write if enabled_outputs bit is also high 
					tDIO::tDO outputs = hel::roborio_state.digital_system.getOutputs();
					outputs.Headers = value;
					hel::roborio_state.digital_system.setOutputs(outputs);
				} else {
					//TODO error handling
			 	}
			}
		}
	}
	
	void writeDO_SPIPort(uint8_t value, tRioStatusCode* /*status*/){
		for(uint32_t i = 1; i <= value; i <<= 1){ 
			if(value & i){ // attempt output if bit in value is high
				if(hel::roborio_state.digital_system.getEnabledOutputs().SPIPort & i){ //allow write if enabled_outputs bit is also high
					tDIO::tDO outputs = hel::roborio_state.digital_system.getOutputs();
					outputs.SPIPort = value;
					hel::roborio_state.digital_system.setOutputs(outputs);
				} else {
					//TODO error handling
			 	}
			}
		}
	}
	
	void writeDO_Reserved(uint8_t value, tRioStatusCode* /*status*/){
		for(uint32_t i = 1; i <= value; i <<= 1){ 
			if(value & i){ // attempt output if bit in value is high
				if(hel::roborio_state.digital_system.getEnabledOutputs().Reserved & i){ //allow write if enabled_outputs bit is also high
					tDIO::tDO outputs = hel::roborio_state.digital_system.getOutputs();
					outputs.Reserved = value;
					hel::roborio_state.digital_system.setOutputs(outputs);
				} else {
					//TODO error handling
			 	}
			}
		}
	}
	
	void writeDO_MXP(uint16_t value, tRioStatusCode* /*status*/){
		for(uint32_t i = 1; i <= value; i <<= 1){ 
			if(value & i){ // attempt output if bit in value is high
				if(hel::roborio_state.digital_system.getEnabledOutputs().MXP & i){ //allow write if enabled_outputs bit is also high
					tDIO::tDO outputs = hel::roborio_state.digital_system.getOutputs();
					outputs.MXP = value;
					hel::roborio_state.digital_system.setOutputs(outputs);
				} else {
					//TODO error handling
			 	}
			}
		}
	}

	tDO readDO(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getOutputs();
	}

	uint16_t readDO_Headers(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getOutputs().Headers;
	}

	uint8_t readDO_SPIPort(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getOutputs().SPIPort;
	}

	uint8_t readDO_Reserved(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getOutputs().Reserved;
	}
	
	uint16_t readDO_MXP(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getOutputs().MXP;
	}

	void writePWMDutyCycleA(uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
		uint32_t i = 1u << bitfield_index;
		if(hel::roborio_state.digital_system.getEnabledOutputs().MXP & i){
			hel::roborio_state.digital_system.setPWMDutyCycle(bitfield_index, value);
		} else {
			//TODO error handling
		}
	}

	uint8_t readPWMDutyCycleA(uint8_t bitfield_index, tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getPWMDutyCycle(bitfield_index);
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
		hel::roborio_state.digital_system.setEnabledOutputs(value);
	}

	void writeOutputEnable_Headers(uint16_t value, tRioStatusCode* /*status*/){
		tDIO::tOutputEnable enabled_outputs = hel::roborio_state.digital_system.getEnabledOutputs();
		enabled_outputs.Headers = value;
		hel::roborio_state.digital_system.setEnabledOutputs(enabled_outputs);
	}
	
	void writeOutputEnable_SPIPort(uint8_t value, tRioStatusCode* /*status*/){
		tDIO::tOutputEnable enabled_outputs = hel::roborio_state.digital_system.getEnabledOutputs();
		enabled_outputs.SPIPort = value;
		hel::roborio_state.digital_system.setEnabledOutputs(enabled_outputs);
	}
	
	void writeOutputEnable_Reserved(uint8_t value, tRioStatusCode* /*status*/){
		tDIO::tOutputEnable enabled_outputs = hel::roborio_state.digital_system.getEnabledOutputs();
		enabled_outputs.Reserved = value;
		hel::roborio_state.digital_system.setEnabledOutputs(enabled_outputs);
	}
	
	void writeOutputEnable_MXP(uint16_t value, tRioStatusCode* /*status*/){
		tDIO::tOutputEnable enabled_outputs = hel::roborio_state.digital_system.getEnabledOutputs();
		enabled_outputs.MXP = value;
		hel::roborio_state.digital_system.setEnabledOutputs(enabled_outputs);
	}

	tOutputEnable readOutputEnable(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getEnabledOutputs();
	}

	uint16_t readOutputEnable_Headers(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getEnabledOutputs().Headers;
	}
	
	uint8_t readOutputEnable_SPIPort(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getEnabledOutputs().SPIPort;
	}
	
	uint8_t readOutputEnable_Reserved(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getEnabledOutputs().Reserved;
	}

	uint16_t readOutputEnable_MXP(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getEnabledOutputs().MXP;
	}

	void writePWMOutputSelect(uint8_t bitfield_index, uint8_t /*value*/, tRioStatusCode* /*status*/){
		//note: bitfield_index is mxp remapped dio address corresponding to the mxp pwm output
		tDIO::tOutputEnable enabled_outputs = hel::roborio_state.digital_system.getEnabledOutputs();
		enabled_outputs.MXP &= 1u << bitfield_index;
		hel::roborio_state.digital_system.setEnabledOutputs(enabled_outputs);
	}

	uint8_t readPWMOutputSelect(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
		return 0;//unnecessary for emulation
	}

	void writePulse(tDIO::tPulse value, tRioStatusCode* /*status*/){
		hel::roborio_state.digital_system.setPulses(value);
		//TODO this should only last for pulse_length seconds, and only one pulse should be active at a time?
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
		return hel::roborio_state.digital_system.getPulses();
	}

	uint16_t readPulse_Headers(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getPulses().Headers;
	}
	
	uint8_t readPulse_SPIPort(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getPulses().SPIPort;
	}
	
	uint8_t readPulse_Reserved(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getPulses().Reserved;
	}
	
	uint16_t readPulse_MXP(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getPulses().MXP;
	}

	tDI readDI(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getInputs();
	}

	uint16_t readDI_Headers(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getInputs().Headers;
	}
	
	uint8_t readDI_SPIPort(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getInputs().SPIPort;
	}
	
	uint8_t readDI_Reserved(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getInputs().Reserved;
	}
	
	uint16_t readDI_MXP(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getInputs().MXP;
	}		

	void writeEnableMXPSpecialFunction(uint16_t value, tRioStatusCode* /*status*/){
		hel::roborio_state.digital_system.setMXPSpecialFunctionsEnabled(value);
	}

	uint16_t readEnableMXPSpecialFunction(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getMXPSpecialFunctionsEnabled();
	}

	void writeFilterSelectMXP(uint8_t /*bitfield_index*/, uint8_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation

	uint8_t readFilterSelectMXP(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
		return 0;//unnecessary for emulation
	}

	void writePulseLength(uint8_t value, tRioStatusCode* /*status*/){
		hel::roborio_state.digital_system.setPulseLength(value);
	}

	uint8_t readPulseLength(tRioStatusCode* /*status*/){
		return hel::roborio_state.digital_system.getPulseLength();
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

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		tDIO* tDIO::create(tRioStatusCode* /*status*/){
			return new DIOManager();
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

