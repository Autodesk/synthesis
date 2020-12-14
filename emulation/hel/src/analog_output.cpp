#include "roborio_manager.hpp"
#include "robot_outputs.hpp"
#include "system_interface.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    uint16_t AnalogOutputs::getMXPOutput(uint8_t index)const{
        return mxp_outputs[index];
    }

    void AnalogOutputs::setMXPOutput(uint8_t index, uint16_t value){
        mxp_outputs[index] = value;
        auto instance = RobotOutputsManager::getInstance();
        instance.first->updateDeep();
        instance.second.unlock();
    }

    AnalogOutputs::AnalogOutputs()noexcept:mxp_outputs(0){}
    AnalogOutputs::AnalogOutputs(const AnalogOutputs& source)noexcept:AnalogOutputs(){
#define COPY(NAME) NAME = source.NAME
        COPY(mxp_outputs);
#undef COPY
    }

    struct AnalogOutputManager: public tAO{
        tSystemInterface* getSystemInterface(){
            return new SystemInterface();
        }

        void writeMXP(uint8_t reg_index, uint16_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->analog_outputs.setMXPOutput(reg_index, value);
            instance.second.unlock();
        }

        uint16_t readMXP(uint8_t reg_index, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->analog_outputs.getMXPOutput(reg_index);
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tAO* tAO::create(tRioStatusCode* /*status*/){
            return new hel::AnalogOutputManager();
        }
    }
}
