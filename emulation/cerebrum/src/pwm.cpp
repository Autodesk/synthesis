#include "roborio.h"

#include "athena/DigitalInternal.h"

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		uint32_t tPWM::readCycleStartTime(tRioStatusCode* status){
			return 0;
		}
		
		void tPWM::writeConfig(tPWM::tConfig value, tRioStatusCode* status){
			cerebrum::roborio_state.pwm_system.config = value;
		}

		void tPWM::writeConfig_Period(uint16_t value, tRioStatusCode* status){
			cerebrum::roborio_state.pwm_system.config.Period = value;
		}
	
		void tPWM::writeConfig_MinHigh(uint16_t value, tRioStatusCode* status){
			cerebrum::roborio_state.pwm_system.config.MinHigh = value;
		}

		tPWM::tConfig tPWM::readConfig(tRioStatusCode* status){
			return cerebrum::roborio_state.pwm_system.config;
		}
		
		uint16_t tPWM::readConfig_Period(tRioStatusCode* status){
			return cerebrum::roborio_state.pwm_system.config.Period;
		}
		
		uint16_t tPWM::readConfig_MinHigh(tRioStatusCode* status){
			return cerebrum::roborio_state.pwm_system.config.MinHigh;
		}

		uint32_t readCycleStartTimeUpper(tRioStatusCode* status){
			return 0;
		}

		uint16_t readLoopTiming(tRioStatusCode* status){
			return hal::kExpectedLoopTiming;
		}

		void tPWM::writePeriodScaleMXP(uint8_t bitfield_index, uint8_t value, tRioStatusCode* status){
			cerebrum::roborio_state.pwm_system.mxp_period_scale[bitfield_index] = value;
		}

		uint8_t tPWM::readPeriodScaleMXP(uint8_t bitfield_index, tRioStatusCode* status){
			return cerebrum::roborio_state.pwm_system.hdr_period_scale[bitfield_index];
		}

		void tPWM::writePeriodScaleHdr(uint8_t bitfield_index, uint8_t value, tRioStatusCode* status){
			cerebrum::roborio_state.pwm_system.hdr_period_scale[bitfield_index] = value;
		}
		
		uint8_t tPWM::readPeriodScaleHdr(uint8_t bitfield_index, tRioStatusCode* status){
			return cerebrum::roborio_state.pwm_system.hdr_period_scale[bitfield_index];
		}

		void tPWM::writeZeroLatch(uint8_t bitfield_index, bool value, tRioStatusCode* status){
			//TODO
		}
	
		bool tPWM::readZeroLatch(uint8_t bitfield_index, tRioStatusCode* status){
			//TODO
		}

		void tPWM::writeHdr(uint8_t reg_index, uint16_t value, tRioStatusCode* status){
			cerebrum::roborio_state.pwm_system.hdr_values[reg_index] = value;
		}
		
		uint16_t tPWM::readHdr(uint8_t reg_index, tRioStatusCode* status){
			return cerebrum::roborio_state.pwm_system.hdr_values[reg_index];
		}

		void tPWM::writeMXP(uint8_t reg_index, uint16_t value, tRioStatusCode* status){
			cerebrum::roborio_state.pwm_system.mxp_values[reg_index] = value;
		}
		
		uint16_t tPWM::readMXP(uint8_t reg_index, tRioStatusCode* status){
			return cerebrum::roborio_state.pwm_system.mxp_values[reg_index];
		}
	}
}

#ifdef PWM_TEST

int main(){
}

#endif
