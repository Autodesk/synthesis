#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

struct AnalogOutputManager: public tAO{
	tSystemInterface* getSystemInterface(){
		return nullptr;
	}

	void writeMXP(uint8_t reg_index, uint16_t value, tRioStatusCode* /*status*/){
		cerebrum::roborio_state.analog_outputs[reg_index].mxp_value = value;
	}

	uint16_t readMXP(uint8_t reg_index, tRioStatusCode* /*status*/){
		return cerebrum::roborio_state.analog_outputs[reg_index].mxp_value;
	}
};

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		tAO* tAO::create(tRioStatusCode* /*status*/){
			return new AnalogOutputManager();
		}
	}
}

#ifdef ANALOG_OUTPUT_TEST

int main(){
	//tAO* a = tAO::create(nullptr);
}

#endif
