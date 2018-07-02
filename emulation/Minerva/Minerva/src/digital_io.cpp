#include "roborio.h"

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		void writeDO(tDIO::tDO value, tRioStatusCode* status){
			minerva::roborio_state.digital_system.outputs = value;
		}

		void writeDO_Headers(uint16_t value, tRioStatusCode* status){}
		void writeDO_SPIPort(uint8_t value, tRioStatusCode* status){}
		void writeDO_Reserved(uint8_t value, tRioStatusCode* status){}
		void writeDO_MXP(uint16_t value, tRioStatusCode* status){}

		tDIO::tDO readDO(tRioStatusCode* status){
			return minerva::roborio_state.digital_system.outputs;
		}

		uint16_t readDO_Headers(tRioStatusCode* status){}
		uint8_t readDO_SPIPort(tRioStatusCode* status){}
		uint8_t readDO_Reserved(tRioStatusCode* status){}
		uint16_t readDO_MXP(tRioStatusCode* status){}

		void writePWMDutyCycleA(uint8_t bitfield_index, uint8_t value, tRioStatusCode* status){
			for(minerva::RoboRIO::DIOSystem::PWMData& pwm: minerva::roborio_state.digital_system.pwm_data){
				if(pwm.id == bitfield_index){
					pwm.duty_cycle = value;
					return;
				}
			}
			//TODO error handling 
		}

		uint8_t readPWMDutyCycleA(uint8_t bitfield_index, tRioStatusCode* status){
			for(minerva::RoboRIO::DIOSystem::PWMData& pwm: minerva::roborio_state.digital_system.pwm_data){
				if(pwm.id == bitfield_index){
					return pwm.duty_cycle;
				}
			}
			//TODO error handling	
		}

		void writePWMDutyCycleB(uint8_t bitfield_index, uint8_t value, tRioStatusCode* status){
			writePWMDutyCycleA(bitfield_index + 4, value, status);//Probably unnecessary difference between functions A and B, so to merge them, we call A from B adding 4 to the id since HAL subtracts 4 when it calls B
		}

		uint8_t readPWMDutyCycleB(uint8_t bitfield_index, tRioStatusCode* status){
			return readPWMDutyCycleA(bitfield_index + 4, status);
		}

		void writeFilterSelectHdr(uint8_t bitfield_index, uint8_t value, tRioStatusCode* status){
			//TODO
		}

		uint8_t readFilterSelectHdr(uint8_t bitfield_index, tRioStatusCode* status){
			//TODO
		}

		void writeOutputEnable(tDIO::tOutputEnable value, tRioStatusCode* status){
			minerva::roborio_state.digital_system.enabled_outputs = value;
		}

		void writeOutputEnable_Headers(uint16_t value, tRioStatusCode* status){}
		void writeOutputEnable_SPIPort(uint8_t value, tRioStatusCode* status){}
		void writeOutputEnable_Reserved(uint8_t value, tRioStatusCode* status){}
		void writeOutputEnable_MXP(uint16_t value, tRioStatusCode* status){}

		tDIO::tOutputEnable readOutputEnable(tRioStatusCode* status){
			return minerva::roborio_state.digital_system.enabled_outputs;
		}

		uint16_t readOutputEnable_Headers(tRioStatusCode* status){}
		uint8_t readOutputEnable_SPIPort(tRioStatusCode* status){}
		uint8_t readOutputEnable_Reserved(tRioStatusCode* status){}
		uint16_t readOutputEnable_MXP(tRioStatusCode* status){}

		void writePWMOutputSelect(uint8_t bitfield_index, uint8_t value, tRioStatusCode* status){
			//TODO
		}

		uint8_t readPWMOutputSelect(uint8_t bitfield_index, tRioStatusCode* status){
			//TODO
		}

		void writePulse(tDIO::tPulse value, tRioStatusCode* status){
			//TODO
		}

		void writePulse_Headers(uint16_t value, tRioStatusCode* status){}
		void writePulse_SPIPort(uint8_t value, tRioStatusCode* status){}
		void writePulse_Reserved(uint8_t value, tRioStatusCode* status){}
		void writePulse_MXP(uint16_t value, tRioStatusCode* status){}

		tDIO::tPulse readPulse(tRioStatusCode* status){
			//TODO
		}

		uint16_t readPulse_Headers(tRioStatusCode* status){}
		uint8_t readPulse_SPIPort(tRioStatusCode* status){}
		uint8_t readPulse_Reserved(tRioStatusCode* status){}
		uint16_t readPulse_MXP(tRioStatusCode* status){}

		tDIO::tDI readDI(tRioStatusCode* status){
			//TODO
		}

		uint16_t readDI_Headers(tRioStatusCode* status){}
		uint8_t readDI_SPIPort(tRioStatusCode* status){}
		uint8_t readDI_Reserved(tRioStatusCode* status){}
		uint16_t readDI_MXP(tRioStatusCode* status){}

		void writeEnableMXPSpecialFunction(uint16_t value, tRioStatusCode* status){
			//TODO
		}

		uint16_t readEnableMXPSpecialFunction(tRioStatusCode* status){
			//TODO
		}

		void writeFilterSelectMXP(uint8_t bitfield_index, uint8_t value, tRioStatusCode* status){
			//TODO
		}

		uint8_t readFilterSelectMXP(uint8_t bitfield_index, tRioStatusCode* status){
			//TODO
		}

		void writePulseLength(uint8_t value, tRioStatusCode* status){
			//TODO
		}

		uint8_t readPulseLength(tRioStatusCode* status){
			//TODO
		}

		void writePWMPeriodPower(uint16_t value, tRioStatusCode* status){
			//TODO
		}

		uint16_t readPWMPeriodPower(tRioStatusCode* status){
			//TODO
		}

		void writeFilterPeriodMXP(uint8_t reg_index, uint32_t value, tRioStatusCode* status){
			//TODO
		}

		uint32_t readFilterPeriodMXP(uint8_t reg_index, tRioStatusCode* status){
			//TODO
		}

		void writeFilterPeriodHdr(uint8_t reg_index, uint32_t value, tRioStatusCode* status){
			//TODO
		}

		uint32_t readFilterPeriodHdr(uint8_t reg_index, tRioStatusCode* status){
			//TODO
		}

	}
}

#ifdef DIGITAL_IO_TEST

int main(){
}

#endif

