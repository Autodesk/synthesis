#include "roborio.h"
#include "util.h"

#include "athena/DigitalInternal.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;    

namespace hel{
    tPWM::tConfig RoboRIO::PWMSystem::getConfig()const{
    	return config;
    }

    void RoboRIO::PWMSystem::setConfig(tPWM::tConfig value){
    	config = value;
    }

    uint32_t RoboRIO::PWMSystem::getHdrPeriodScale(uint8_t index)const{
    	return hdr[index].period_scale;
    }

    void RoboRIO::PWMSystem::setHdrPeriodScale(uint8_t index, uint32_t value){
    	hdr[index].period_scale = value;
    }

    uint32_t RoboRIO::PWMSystem::getMXPPeriodScale(uint8_t index)const{
    	return mxp[index].period_scale;
    }

    void RoboRIO::PWMSystem::setMXPPeriodScale(uint8_t index, uint32_t value){
    	mxp[index].period_scale = value;
    }

    uint32_t RoboRIO::PWMSystem::getHdrDutyCycle(uint8_t index)const{
    	return hdr[index].duty_cycle;
    }

    void RoboRIO::PWMSystem::setHdrDutyCycle(uint8_t index, uint32_t value){
    	hdr[index].duty_cycle = value;
    }

    uint32_t RoboRIO::PWMSystem::getMXPDutyCycle(uint8_t index)const{
    	return mxp[index].duty_cycle;
    }

    void RoboRIO::PWMSystem::setMXPDutyCycle(uint8_t index, uint32_t value){
    	mxp[index].duty_cycle = value;
    }


    struct PWMManager: public tPWM{
        tSystemInterface* getSystemInterface(){
            return nullptr;
        }

        uint32_t readCycleStartTime(tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation
        }

        void writeConfig(tPWM::tConfig value, tRioStatusCode* /*status*/){
            RoboRIOManager::getInstance()->pwm_system.setConfig(value);
        }

        void writeConfig_Period(uint16_t value, tRioStatusCode* /*status*/){
            tPWM::tConfig config = RoboRIOManager::getInstance()->pwm_system.getConfig();
            config.Period = value;
            RoboRIOManager::getInstance()->pwm_system.setConfig(config);
        }

        void writeConfig_MinHigh(uint16_t value, tRioStatusCode* /*status*/){
            tPWM::tConfig config = RoboRIOManager::getInstance()->pwm_system.getConfig();
            config.MinHigh = value;
            RoboRIOManager::getInstance()->pwm_system.setConfig(config);
        }

        tPWM::tConfig readConfig(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->pwm_system.getConfig();
        }

        uint16_t readConfig_Period(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->pwm_system.getConfig().Period;
        }

        uint16_t readConfig_MinHigh(tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->pwm_system.getConfig().MinHigh;
        }

        uint32_t readCycleStartTimeUpper(tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation
        }    

        uint16_t readLoopTiming(tRioStatusCode* /*status*/){
            return hal::kExpectedLoopTiming;
        }

        void writePeriodScaleMXP(uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
            RoboRIOManager::getInstance()->pwm_system.setMXPPeriodScale(bitfield_index, value);
        }

        uint8_t readPeriodScaleMXP(uint8_t bitfield_index, tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->pwm_system.getMXPPeriodScale(bitfield_index);
        }

        void writePeriodScaleHdr(uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
            RoboRIOManager::getInstance()->pwm_system.setHdrPeriodScale(bitfield_index, value);
        }

        uint8_t readPeriodScaleHdr(uint8_t bitfield_index, tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->pwm_system.getHdrPeriodScale(bitfield_index);
        }

        void writeZeroLatch(uint8_t bitfield_index, bool value, tRioStatusCode* /*status*/){
            //TODO
        }

        bool readZeroLatch(uint8_t bitfield_index, tRioStatusCode* /*status*/){
            return false;
            //TODO
        }

        void writeHdr(uint8_t reg_index, uint16_t value, tRioStatusCode* /*status*/){
            RoboRIOManager::getInstance()->pwm_system.setHdrDutyCycle(reg_index, value);
        }

        uint16_t readHdr(uint8_t reg_index, tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->pwm_system.getHdrDutyCycle(reg_index);
        }

        void writeMXP(uint8_t reg_index, uint16_t value, tRioStatusCode* /*status*/){
            uint8_t DO_index = [&]{
    			if(reg_index < 4){ //First four MXP PWM channels line up with DO, but the next six are offset by four
    				return reg_index;
    			}
    			reg_index += 4; 
    			return reg_index;
    		}();
    		if( 
    			checkBitHigh(hel::RoboRIOManager::getInstance()->digital_system.getEnabledOutputs().MXP, DO_index) && //Allow MXP output if pin is output-enabled
    			checkBitHigh(hel::RoboRIOManager::getInstance()->digital_system.getMXPSpecialFunctionsEnabled(), DO_index) //Allow MXP outout if DO is using special function
    		){
    			RoboRIOManager::getInstance()->pwm_system.setMXPDutyCycle(reg_index, value);
    		}
            //TODO error handling
        }

        uint16_t readMXP(uint8_t reg_index, tRioStatusCode* /*status*/){
            return RoboRIOManager::getInstance()->pwm_system.getMXPDutyCycle(reg_index);
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
    	tPWM* tPWM::create(tRioStatusCode* /*status*/){
    		return new hel::PWMManager();
    	}
    }
}
