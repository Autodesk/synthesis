#include "tInterruptImpl.h"
#include "NiFpgaState.h"
#include "tGlobalImpl.h"

namespace nFPGA {

	tInterrupt_Impl::tInterrupt_Impl(NiFpgaState *state, unsigned char sys_index) {
		this->state = state;
		this->sys_index = sys_index;
		this->config.value = 0;
		// TODO actually implement these
	}

	tInterrupt_Impl::~tInterrupt_Impl() {
		if (this->state->interrupt[this->sys_index] == this) {
			this->state->interrupt[this->sys_index] = NULL;
		}
	}

	tSystemInterface *tInterrupt_Impl::getSystemInterface() {
		return state;
	}

	unsigned char tInterrupt_Impl::getSystemIndex() {
		return sys_index;
	}

	unsigned int tInterrupt_Impl::readTimeStamp(tRioStatusCode *status) {
		return this->state->getGlobal()->readLocalTime(status);
	}

#pragma region writeConfig*
	void tInterrupt_Impl::writeConfig(tConfig value, tRioStatusCode *status) { 
		*status = NiFpga_Status_Success;
		config = value;
	}
	void tInterrupt_Impl::writeConfig_Source_Channel(unsigned char value, tRioStatusCode *status) { 
		config.Source_Channel = value;
		*status = NiFpga_Status_Success;
	}
	void tInterrupt_Impl::writeConfig_Source_Module(unsigned char value, tRioStatusCode *status) { 
		config.Source_Module = value;
		*status = NiFpga_Status_Success;
	}
	void tInterrupt_Impl::writeConfig_Source_AnalogTrigger(bool value, tRioStatusCode *status) { 
		config.Source_AnalogTrigger = value;
		*status = NiFpga_Status_Success;
	}
	void tInterrupt_Impl::writeConfig_RisingEdge(bool value, tRioStatusCode *status) { 
		config.RisingEdge = value;
		*status = NiFpga_Status_Success;
	}
	void tInterrupt_Impl::writeConfig_FallingEdge(bool value, tRioStatusCode *status) { 
		config.FallingEdge = value;
		*status = NiFpga_Status_Success;
	}
	void tInterrupt_Impl::writeConfig_WaitForAck(bool value, tRioStatusCode *status) { 
		config.WaitForAck = value;
		*status = NiFpga_Status_Success;
	}
	tInterrupt_Impl::tConfig tInterrupt_Impl::readConfig(tRioStatusCode *status) { 
		*status = NiFpga_Status_Success;
		return config;
	}
#pragma endregion

#pragma region readConfig*
	unsigned char tInterrupt_Impl::readConfig_Source_Channel(tRioStatusCode *status) { 
		*status = NiFpga_Status_Success;
		return config.Source_Channel;
	}
	unsigned char tInterrupt_Impl::readConfig_Source_Module(tRioStatusCode *status) { 
		*status = NiFpga_Status_Success;
		return config.Source_Module;
	}
	bool tInterrupt_Impl::readConfig_Source_AnalogTrigger(tRioStatusCode *status) { 
		*status = NiFpga_Status_Success;
		return config.Source_AnalogTrigger;
	}
	bool tInterrupt_Impl::readConfig_RisingEdge(tRioStatusCode *status) { 
		*status = NiFpga_Status_Success;
		return config.RisingEdge;
	}
	bool tInterrupt_Impl::readConfig_FallingEdge(tRioStatusCode *status) { 
		*status = NiFpga_Status_Success;
		return config.FallingEdge;
	}
	bool tInterrupt_Impl::readConfig_WaitForAck(tRioStatusCode *status) { 
		*status = NiFpga_Status_Success;
		return config.WaitForAck;
	}
#pragma endregion
}