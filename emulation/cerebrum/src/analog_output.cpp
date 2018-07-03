#include "roborio.h"

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		void tAO::writeMXP(uint8_t reg_index, uint16_t value, tRioStatusCode* /*status*/){
			cerebrum::roborio_state.analog_outputs[reg_index].mxp_value = value;
		}

		uint16_t tAO::readMXP(uint8_t reg_index, tRioStatusCode* /*status*/){
			return cerebrum::roborio_state.analog_outputs[reg_index].mxp_value;
		}
	}
}

#ifdef ANALOG_OUTPUT_TEST

int main(){
}

#endif
