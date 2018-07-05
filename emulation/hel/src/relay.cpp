#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{

	tRelay::tValue RoboRIO::RelaySystem::getValue()const{
		return value;
	}

	void RoboRIO::RelaySystem::setValue(tRelay::tValue v){
		value = v;
	}

	struct RelayManager: public tRelay{
		tSystemInterface* getSystemInterface(){
			return nullptr;
		}
		
		void writeValue(tValue value, tRioStatusCode *status){
			hel::RoboRIOManager::getInstance()->relay_system.setValue(value);
		}

		void writeValue_Forward(uint8_t value, tRioStatusCode *status){
			tRelay::tValue v = hel::RoboRIOManager::getInstance()->relay_system.getValue();
			v.Forward = value;
			hel::RoboRIOManager::getInstance()->relay_system.setValue(v);
		}

		void writeValue_Reverse(uint8_t value, tRioStatusCode *status){
			tRelay::tValue v = hel::RoboRIOManager::getInstance()->relay_system.getValue();
			v.Reverse = value;
			hel::RoboRIOManager::getInstance()->relay_system.setValue(v);
		}

		tValue readValue(tRioStatusCode *status){
			return hel::RoboRIOManager::getInstance()->relay_system.getValue();
		}

		uint8_t readValue_Forward(tRioStatusCode *status){
			return hel::RoboRIOManager::getInstance()->relay_system.getValue().Forward;
		}

		uint8_t readValue_Reverse(tRioStatusCode *status){
			return hel::RoboRIOManager::getInstance()->relay_system.getValue().Reverse;
		}
	};
}

namespace nFPGA{
	namespace nRoboRIO_FPGANamespace{
		tRelay* tRelay::create(tRioStatusCode* /*status*/){
			return new hel::RelayManager();
		}
	}
}
