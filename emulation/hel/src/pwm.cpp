#include "roborio.hpp"
#include "util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tPWM::tConfig PWMSystem::getConfig()const{
        return config;
    }

    void PWMSystem::setConfig(tPWM::tConfig value){
        config = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    uint32_t PWMSystem::getHdrPeriodScale(uint8_t index)const{
        return hdr[index].period_scale;
    }

    void PWMSystem::setHdrPeriodScale(uint8_t index, uint32_t value){
        hdr[index].period_scale = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    uint32_t PWMSystem::getMXPPeriodScale(uint8_t index)const{
        return mxp[index].period_scale;
    }

    void PWMSystem::setMXPPeriodScale(uint8_t index, uint32_t value){
        mxp[index].period_scale = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    uint32_t PWMSystem::getHdrPulseWidth(uint8_t index)const{
        return hdr[index].pulse_width;
    }

    void PWMSystem::setHdrPulseWidth(uint8_t index, uint32_t value){
        hdr[index].pulse_width = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    uint32_t PWMSystem::getMXPPulseWidth(uint8_t index)const{
        return mxp[index].pulse_width;
    }

    void PWMSystem::setMXPPulseWidth(uint8_t index, uint32_t value){
        mxp[index].pulse_width = value;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    double PWMSystem::getSpeed(uint32_t pulse_width) {
        // All of these values were calculated based off of the WPILib defaults and the math used to calculate their respective fields
        const int32_t max = 1499;
        const int32_t center = 999;
        const int32_t min = 499;

        const int32_t deadband_max = center + 1;
        const int32_t deadband_min = center - 1;

        const int32_t positive_scale_factor = max - deadband_max;
        const int32_t negative_scale_factor = deadband_min - min;

        if (pulse_width == 0) {
            return 0.0;
        } else if (pulse_width > max) {
            return 1.0;
        } else if (pulse_width < min) {
            return -1.0;
        } else if (pulse_width > deadband_max) {
            return static_cast<double>((int32_t) pulse_width - deadband_max) / static_cast<double>(positive_scale_factor);
        } else if (pulse_width < deadband_min) {
            return static_cast<double>((int32_t) pulse_width - deadband_min) / static_cast<double>(negative_scale_factor);
        } else {
            return 0.0;
        }
    }

    PWMSystem::PWM::PWM():period_scale(0), pulse_width(0){}

    PWMSystem::PWMSystem():hdr(),mxp(){}

    struct PWMManager: public tPWM{
        tSystemInterface* getSystemInterface(){
            return nullptr;
        }

        uint32_t readCycleStartTime(tRioStatusCode* /*status*/){
            return 0;//unnecessary for emulation
        }

        void writeConfig(tPWM::tConfig value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->pwm_system.setConfig(value);
            instance.second.unlock();
        }

        void writeConfig_Period(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tPWM::tConfig config = instance.first->pwm_system.getConfig();
            config.Period = value;
            instance.first->pwm_system.setConfig(config);
            instance.second.unlock();
        }

        void writeConfig_MinHigh(uint16_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            tPWM::tConfig config = instance.first->pwm_system.getConfig();
            config.MinHigh = value;
            instance.first->pwm_system.setConfig(config);
            instance.second.unlock();
        }

        tPWM::tConfig readConfig(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->pwm_system.getConfig();
        }

        uint16_t readConfig_Period(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->pwm_system.getConfig().Period;
        }

        uint16_t readConfig_MinHigh(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->pwm_system.getConfig().MinHigh;
        }

        uint32_t readCycleStartTimeUpper(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return 0;//unnecessary for emulation
        }

        uint16_t readLoopTiming(tRioStatusCode* /*status*/){
            return PWMSystem::EXPECTED_LOOP_TIMING;
        }

        void writePeriodScaleMXP(uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->pwm_system.setMXPPeriodScale(bitfield_index, value);
            instance.second.unlock();
        }

        uint8_t readPeriodScaleMXP(uint8_t bitfield_index, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->pwm_system.getMXPPeriodScale(bitfield_index);
        }

        void writePeriodScaleHdr(uint8_t bitfield_index, uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->pwm_system.setHdrPeriodScale(bitfield_index, value);
            instance.second.unlock();
        }

        uint8_t readPeriodScaleHdr(uint8_t bitfield_index, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->pwm_system.getHdrPeriodScale(bitfield_index);
        }

        void writeZeroLatch(uint8_t /*bitfield_index*/, bool /*value*/, tRioStatusCode* /*status*/){} //unnecessary for emulation

        bool readZeroLatch(uint8_t /*bitfield_index*/, tRioStatusCode* /*status*/){ //unnecessary for emulation
            return false;
        }

        void writeHdr(uint8_t reg_index, uint16_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.first->pwm_system.setHdrPulseWidth(reg_index, value);
            instance.second.unlock();
        }

        uint16_t readHdr(uint8_t reg_index, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->pwm_system.getHdrPulseWidth(reg_index);
        }

        void writeMXP(uint8_t reg_index, uint16_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();

            if(value == 0){ //allow disabling PWM even when output isn't configured for PWM
                instance.first->pwm_system.setMXPPulseWidth(reg_index, value);
                instance.second.unlock();
                return;
            }

            uint8_t DO_index = [&]{
                                   if(reg_index < 4){
                                       return reg_index;
                                   }
                                   uint8_t i = reg_index + 4;
                                   return i;
                               }();

            if(checkBitHigh(instance.first->digital_system.getMXPSpecialFunctionsEnabled(), DO_index)){ //Allow MXP outout if DO is using special function
                instance.first->pwm_system.setMXPPulseWidth(reg_index, value);
                instance.second.unlock();
            } else {
                instance.second.unlock();
                throw DigitalSystem::DIOConfigurationException(DigitalSystem::DIOConfigurationException::Config::DO, DigitalSystem::DIOConfigurationException::Config::MXP_SPECIAL_FUNCTION, reg_index + DigitalSystem::NUM_DIGITAL_HEADERS);
            }
        }

        uint16_t readMXP(uint8_t reg_index, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            instance.second.unlock();
            return instance.first->pwm_system.getMXPPulseWidth(reg_index);
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
