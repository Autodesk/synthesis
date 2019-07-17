#include "roborio_manager.hpp"
#include "util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    tPWM::tConfig PWMSystem::getConfig()const noexcept{
        return config;
    }

    void PWMSystem::setConfig(tPWM::tConfig value)noexcept{
        config = value;
    }

    bool PWMSystem::getHdrZeroLatch(uint8_t index)const{
        return hdr[index].zero_latch;
    }

    void PWMSystem::setHdrZeroLatch(uint8_t index, bool value){
        hdr[index].zero_latch = value;
    }

    bool PWMSystem::getMXPZeroLatch(uint8_t index)const{
        return mxp[index].zero_latch;
    }

    void PWMSystem::setMXPZeroLatch(uint8_t index, uint32_t value){
        mxp[index].zero_latch = value;
    }

    uint32_t PWMSystem::getHdrPeriodScale(uint8_t index)const{
        return hdr[index].period_scale;
    }

    void PWMSystem::setHdrPeriodScale(uint8_t index, uint32_t value){
        hdr[index].period_scale = value;
    }

    uint32_t PWMSystem::getMXPPeriodScale(uint8_t index)const{
        return mxp[index].period_scale;
    }

    void PWMSystem::setMXPPeriodScale(uint8_t index, uint32_t value){
        mxp[index].period_scale = value;
    }

    uint32_t PWMSystem::getHdrPulseWidth(uint8_t index)const{
        return hdr[index].pulse_width;
    }

    void PWMSystem::setHdrPulseWidth(uint8_t index, uint32_t value){
        hdr[index].pulse_width = value;
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

    uint32_t PWMSystem::getMXPPulseWidth(uint8_t index)const{
        return mxp[index].pulse_width;
    }

    void PWMSystem::setMXPPulseWidth(uint8_t index, uint32_t value){
        mxp[index].pulse_width = value;
        auto instance = SendDataManager::getInstance();
        instance.first->updateShallow();
        instance.second.unlock();
    }

    double PWMSystem::getPercentOutput(uint32_t pulse_width)noexcept{
        if (pulse_width == 0) {
            return 0.0;
        } else if (pulse_width > pwm_pulse_width::MAX) {
            return 1.0;
        } else if (pulse_width < pwm_pulse_width::MIN) {
            return -1.0;
        } else if (pulse_width > pwm_pulse_width::DEADBAND_MAX) {
            return static_cast<double>((int32_t) pulse_width - pwm_pulse_width::DEADBAND_MAX) / static_cast<double>(pwm_pulse_width::POSITIVE_SCALE_FACTOR);
        } else if (pulse_width < pwm_pulse_width::DEADBAND_MIN) {
            return static_cast<double>((int32_t) pulse_width - pwm_pulse_width::DEADBAND_MIN) / static_cast<double>(pwm_pulse_width::NEGATIVE_SCALE_FACTOR);
        }
        return 0.0;
    }

    PWMSystem::PWM::PWM()noexcept: zero_latch(false), period_scale(0), pulse_width(0){}
    PWMSystem::PWM::PWM(const PWM& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(zero_latch);
        COPY(period_scale);
        COPY(pulse_width);
#undef COPY
    }

    PWMSystem::PWMSystem()noexcept:hdr({}),mxp({}){}
    PWMSystem::PWMSystem(const PWMSystem& source)noexcept:PWMSystem(){
#define COPY(NAME) NAME = source.NAME
        COPY(hdr);
        COPY(mxp);
#undef COPY
    }

    struct PWMManager: public tPWM{
        tSystemInterface* getSystemInterface(){
            return new SystemInterface();
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

        void writeZeroLatch(uint8_t bitfield_index, bool value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            if(bitfield_index < tPWM::kNumHdrRegisters){
                instance.first->pwm_system.setHdrZeroLatch(bitfield_index, value);
            } else {
                instance.first->pwm_system.setMXPZeroLatch(bitfield_index - tPWM::kNumHdrRegisters, value);
            }
            instance.second.unlock();
        }

        bool readZeroLatch(uint8_t bitfield_index, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            if(bitfield_index < tPWM::kNumHdrRegisters){
                instance.second.unlock();
                return instance.first->pwm_system.getHdrZeroLatch(bitfield_index);
            } else {
                instance.second.unlock();
                return instance.first->pwm_system.getMXPZeroLatch(bitfield_index - tPWM::kNumHdrRegisters);
            }
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

            uint8_t DO_index = (reg_index < 4) ? reg_index : reg_index + 4; //Digital MXP 0-3 line up between PWM and digital ports, but the others are offset by 4

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
