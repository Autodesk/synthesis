#include "roborio.h"

#include "athena/DigitalInternal.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;	
	
struct PWMManager: public tPWM{
	tSystemInterface* getSystemInterface(){
		return nullptr;
	}

	uint32_t readCycleStartTime(tRioStatusCode* /*status*/){
		return 0;
	}
	
	void writeConfig(tPWM::tConfig value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.pwm_system.config = value;
	}

	void writeConfig_Period(uint16_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.pwm_system.config.Period = value;
	}
	
	void writeConfig_MinHigh(uint16_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.pwm_system.config.MinHigh = value;
	}

	tPWM::tConfig readConfig(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.pwm_system.config;
	}
	
	uint16_t readConfig_Period(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.pwm_system.config.Period;
	}
	
	uint16_t readConfig_MinHigh(tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.pwm_system.config.MinHigh;
	}

	uint32_t readCycleStartTimeUpper(tRioStatusCode* /*status*/){
		return 0;
	}

	uint16_t readLoopTiming(tRioStatusCode* /*status*/){
		return hal::kExpectedLoopTiming;
	}

	void writePeriodScaleMXP(uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.pwm_system.mxp_period_scale[bitfield_index] = value;
	}

	uint8_t readPeriodScaleMXP(uint8_t bitfield_index, tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.pwm_system.hdr_period_scale[bitfield_index];
	}

	void writePeriodScaleHdr(uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.pwm_system.hdr_period_scale[bitfield_index] = value;
	}
	
	uint8_t readPeriodScaleHdr(uint8_t bitfield_index, tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.pwm_system.hdr_period_scale[bitfield_index];
	}

	void writeZeroLatch(uint8_t bitfield_index, bool value, tRioStatusCode* /*status*/){
		//TODO
	}
	
	bool readZeroLatch(uint8_t bitfield_index, tRioStatusCode* /*status*/){
		//TODO
	}

	void writeHdr(uint8_t reg_index, uint16_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.pwm_system.hdr_values[reg_index] = value;
	}
	
	uint16_t readHdr(uint8_t reg_index, tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.pwm_system.hdr_values[reg_index];
	}

	void writeMXP(uint8_t reg_index, uint16_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.pwm_system.mxp_values[reg_index] = value;
	}
	
	uint16_t readMXP(uint8_t reg_index, tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.pwm_system.mxp_values[reg_index];
	}
};

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		tPWM* tPWM::create(tRioStatusCode* /*status*/){
			return new PWMManager();
		}
	}
}	
#ifdef PWM_TEST

int main(){
	//tPWM* a = tPWM::create(nullptr);
}

#endif
