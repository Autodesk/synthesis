#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

struct DIOManager: public tDIO{
	tSystemInterface* getSystemInterface() override{
		return nullptr;
	}
	
	void writeDO(tDIO::tDO value, tRioStatusCode* status){
		cerebrum::roborio_state.digital_system.outputs = value;
		writeDO_Headers(value.Headers, status);
		writeDO_SPIPort(value.SPIPort, status);
		writeDO_Reserved(value.Reserved, status);
		writeDO_MXP(value.MXP, status);
	}

	void writeDO_Headers(uint16_t value, tRioStatusCode* /*status*/){
		for(uint32_t i = 1; i <= value; i <<= 1){ 
			if(value & i){ // attempt output if bit in value is high
				if(cerebrum::roborio_state.digital_system.enabled_outputs.Headers & i){ //allow write if enabled_outputs bit is also high
					cerebrum::roborio_state.digital_system.outputs.Headers = value;
				} else {
					//TODO error handling
			 	}
			}
		}
	}
	
	void writeDO_SPIPort(uint8_t value, tRioStatusCode* /*status*/){
		for(uint32_t i = 1; i <= value; i <<= 1){ 
			if(value & i){ // attempt output if bit in value is high
				if(cerebrum::roborio_state.digital_system.enabled_outputs.SPIPort & i){ //allow write if enabled_outputs bit is also high
					cerebrum::roborio_state.digital_system.outputs.SPIPort = value;
				} else {
					//TODO error handling
			 	}
			}
		}
	}
	
	void writeDO_Reserved(uint8_t value, tRioStatusCode* /*status*/){
		for(uint32_t i = 1; i <= value; i <<= 1){ 
			if(value & i){ // attempt output if bit in value is high
				if(cerebrum::roborio_state.digital_system.enabled_outputs.Reserved & i){ //allow write if enabled_outputs bit is also high
					cerebrum::roborio_state.digital_system.outputs.Reserved = value;
				} else {
					//TODO error handling
			 	}
			}
		}
	}
	
	void writeDO_MXP(uint16_t value, tRioStatusCode* /*status*/){
		for(uint32_t i = 1; i <= value; i <<= 1){ 
			if(value & i){ // attempt output if bit in value is high
				if(cerebrum::roborio_state.digital_system.enabled_outputs.MXP & i){ //allow write if enabled_outputs bit is also high
					cerebrum::roborio_state.digital_system.outputs.MXP = value;
				} else {
					//TODO error handling
			 	}
			}
		}
	}

	tDO readDO(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.outputs;
	}

	uint16_t readDO_Headers(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.outputs.Headers;
	}

	uint8_t readDO_SPIPort(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.outputs.SPIPort;
	}

	uint8_t readDO_Reserved(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.outputs.Reserved;
	}
	
	uint16_t readDO_MXP(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.outputs.MXP;
	}

	void writePWMDutyCycleA(uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
		uint32_t i = 1u << bitfield_index;
		if(cerebrum::roborio_state.digital_system.enabled_outputs.Headers & i){
			cerebrum::roborio_state.digital_system.pwm[bitfield_index] = value;
		} else {
			//TODO error handling
		}
	}

	uint8_t readPWMDutyCycleA(uint8_t bitfield_index, tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.pwm[bitfield_index];
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
		cerebrum::roborio_state.digital_system.enabled_outputs = value;
	}

	void writeOutputEnable_Headers(uint16_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.digital_system.enabled_outputs.Headers = value;
	}
	
	void writeOutputEnable_SPIPort(uint8_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.digital_system.enabled_outputs.SPIPort = value;
	}
	
	void writeOutputEnable_Reserved(uint8_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.digital_system.enabled_outputs.Reserved = value;
	}
	
	void writeOutputEnable_MXP(uint16_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.digital_system.enabled_outputs.MXP = value;
	}

	tOutputEnable readOutputEnable(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.enabled_outputs;
	}

	uint16_t readOutputEnable_Headers(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.enabled_outputs.Headers;
	}
	
	uint8_t readOutputEnable_SPIPort(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.enabled_outputs.SPIPort;
	}
	
	uint8_t readOutputEnable_Reserved(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.enabled_outputs.Reserved;
	}

	uint16_t readOutputEnable_MXP(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.enabled_outputs.MXP;
	}

	void writePWMOutputSelect(uint8_t bitfield_index, uint8_t /*value*/, tRioStatusCode* /*status*/){
		//note: bitfield_index is mxp remapped dio address corresponding to the mxp pwm output
		cerebrum::roborio_state.digital_system.enabled_outputs.MXP &= 1u << bitfield_index;
	}

	uint8_t readPWMOutputSelect(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
		return 0;//unnecessary for emulation
	}

	void writePulse(tDIO::tPulse value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.digital_system.pulses = value;
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
		return cerebrum::roborio_state.digital_system.pulses;
	}

	uint16_t readPulse_Headers(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.pulses.Headers;
	}
	
	uint8_t readPulse_SPIPort(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.pulses.SPIPort;
	}
	
	uint8_t readPulse_Reserved(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.pulses.Reserved;
	}
	
	uint16_t readPulse_MXP(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.pulses.MXP;
	}

	tDI readDI(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.inputs;
	}

	uint16_t readDI_Headers(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.inputs.Headers;
	}
	
	uint8_t readDI_SPIPort(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.inputs.SPIPort;
	}
	
	uint8_t readDI_Reserved(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.inputs.Reserved;
	}
	
	uint16_t readDI_MXP(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.inputs.MXP;
	}		

	void writeEnableMXPSpecialFunction(uint16_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation


	uint16_t readEnableMXPSpecialFunction(tRioStatusCode* /*status*/){
		return 0;//unnecessary for emulation
	}

	void writeFilterSelectMXP(uint8_t /*bitfield_index*/, uint8_t /*value*/, tRioStatusCode* /*status*/){}//unnecessary for emulation

	uint8_t readFilterSelectMXP(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
		return 0;//unnecessary for emulation
	}

	void writePulseLength(uint8_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.digital_system.pulse_length = value;
	}

	uint8_t readPulseLength(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.digital_system.pulse_length;
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
	nFPGA::nRoboRIO_FPGANamespace::tDIO* a = nFPGA::nRoboRIO_FPGANamespace::tDIO::create(nullptr);
	a->writeOutputEnable_Headers(1u << 3, nullptr);
	a->writeDO_Headers(value, nullptr);
}

#endif

