#include "roborio_manager.hpp"
#include "system_interface.hpp"
#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAnalogTrigger.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
	struct AnalogTriggerManager: public tAnalogTrigger{
	private:
		uint8_t index;
	public:
		AnalogTriggerManager(uint8_t i){
			index = i;
		}

		~AnalogTriggerManager(){}

		tSystemInterface* getSystemInterface(){
			return new SystemInterface();
		}

		uint8_t getSystemIndex(){
			return index;
		}

		void writeSourceSelect(tSourceSelect /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}

		void writeSourceSelect_Channel(uint8_t /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}

		void writeSourceSelect_Averaged(bool /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}

		void writeSourceSelect_Filter(bool /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}

		void writeSourceSelect_FloatingRollover(bool /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}

		void writeSourceSelect_RolloverLimit(int16_t /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}

		void writeSourceSelect_DutyCycle(bool /*value*/, tRioStatusCode* /*status*/) {
			// TODO
		}

		tSourceSelect readSourceSelect(tRioStatusCode* /*status*/){
			return {}; // TODO
		}

		uint8_t readSourceSelect_Channel(tRioStatusCode* /*status*/){
			return 0; // TODO
		}

		bool readSourceSelect_Averaged(tRioStatusCode* /*status*/){
			return false; // TODO
		}

		bool readSourceSelect_Filter(tRioStatusCode* /*status*/){
			return false; // TODO
		}

		bool readSourceSelect_FloatingRollover(tRioStatusCode* /*status*/){
			return false; // TODO
		}

		int16_t readSourceSelect_RolloverLimit(tRioStatusCode* /*status*/){
			return 0; // TODO
		}

		bool readSourceSelect_DutyCycle(tRioStatusCode* /*status*/) {
			return false;	
		}

		void writeUpperLimit(int32_t /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}

		int32_t readUpperLimit(tRioStatusCode* /*status*/){
			return 0; // TODO
		}

		void writeLowerLimit(int32_t /*value*/, tRioStatusCode* /*status*/){
			// TODO
		}

		int32_t readLowerLimit(tRioStatusCode* /*status*/){
			return 0; // TODO
		}

		tOutput readOutput(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
			return {}; // TODO
		}

		bool readOutput_InHysteresis(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
			return false; // TODO
		}

		bool readOutput_OverLimit(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
			return false; // TODO
		}

		bool readOutput_Rising(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
			return false; // TODO
		}

		bool readOutput_Falling(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){
			return false; // TODO
		}
	};
}

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		tAnalogTrigger* tAnalogTrigger::create(uint8_t sys_index, tRioStatusCode* /*status*/){
			return new hel::AnalogTriggerManager(sys_index);
		}
	}
}
