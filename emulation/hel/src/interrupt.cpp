#include "roborio_manager.hpp"
#include "system_interface.hpp"
#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tInterrupt.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
	struct Interrupt: public tInterrupt{
	private:
		uint8_t index;

	public:
		Interrupt(uint8_t i){
			index = i;
		}

		~Interrupt(){}

		tSystemInterface* getSystemInterface(){
			return new SystemInterface();
		}

		uint8_t getSystemIndex(){
			return index;
		}

		uint32_t readFallingTimeStamp(tRioStatusCode* /*status*/){
			return 0; // TODO
		}

		void writeConfig(tConfig /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}
		void writeConfig_Source_Channel(unsigned char /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}
		void writeConfig_Source_Module(unsigned char /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}
		void writeConfig_Source_AnalogTrigger(bool /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}
		void writeConfig_RisingEdge(bool /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}
		void writeConfig_FallingEdge(bool /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}
		void writeConfig_WaitForAck(bool /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}

		tInterrupt::tConfig readConfig(tRioStatusCode* /*status*/){
			return {}; // TODO
		}

		uint8_t readConfig_Source_Channel(tRioStatusCode* /*status*/){
			return 0; // TODO
		}

		uint8_t readConfig_Source_Module(tRioStatusCode* /*status*/){
			return 0; // TODO
		}

		bool readConfig_Source_AnalogTrigger(tRioStatusCode* /*status*/){
			return false; // TODO
		}

		bool readConfig_RisingEdge(tRioStatusCode* /*status*/){
			return false; // TODO
		}

		bool readConfig_FallingEdge(tRioStatusCode* /*status*/){
			return false; // TODO
		}

		bool readConfig_WaitForAck(tRioStatusCode* /*status*/){
			return false; // TODO
		}

		uint32_t readRisingTimeStamp(tRioStatusCode* /*status*/){
			return 0; // TODO
		}
	};

}

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		tInterrupt* tInterrupt::create(uint8_t sys_index, tRioStatusCode* /*status*/){
			return new hel::Interrupt(sys_index);
		}
	}
}
