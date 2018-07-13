#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;
 
namespace hel{
    tAccumulator::tOutput RoboRIO::Accumulator::getOutput()const{
        return output;
    }
    
    void RoboRIO::Accumulator::setOutput(tAccumulator::tOutput out){
        output = out;
    }

    int32_t RoboRIO::Accumulator::getCenter()const{
        return center;
    }

    void RoboRIO::Accumulator::setCenter(int32_t c){
        center = c;
    }

    int32_t RoboRIO::Accumulator::getDeadband()const{
        return deadband;
    }

    void RoboRIO::Accumulator::setDeadband(int32_t d){
        deadband = d;
    }

    struct AccumulatorManager: public tAccumulator{
    private:
        uint8_t index;

    public:
        tSystemInterface* getSystemInterface(){ //unnecessary for emulation
            return nullptr;
        }
 
        uint8_t getSystemIndex(){
            return index;
        }
 
        tOutput readOutput(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->accumulators[index].getOutput();
        }
 
        signed long long readOutput_Value(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->accumulators[index].getOutput().Value;
        }
 
        uint32_t readOutput_Count(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->accumulators[index].getOutput().Count;
        }
 
        void writeCenter(int32_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->accumulators[index].setCenter(value);
        }
 
        int32_t readCenter(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->accumulators[index].getCenter();
        }
 
        void writeDeadband(int32_t value, tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->accumulators[index].setDeadband(value);
        }
 
        int32_t readDeadband(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->accumulators[index].getDeadband();
        }
 
        void strobeReset(tRioStatusCode* /*status*/){
            auto instance = hel::RoboRIOManager::getInstance();
            tOutput output;
            instance.second.unlock();
            return instance.first->accumulators[index].setOutput(output);
        }

        AccumulatorManager(uint8_t i):index(i){}
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tAccumulator* tAccumulator::create(uint8_t sys_index, tRioStatusCode* /*status*/){
            return new hel::AccumulatorManager(sys_index);
        }
    }
}

