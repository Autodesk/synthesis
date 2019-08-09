#include "roborio_manager.hpp"
#include "system_interface.hpp"

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAccel.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    Accelerometer:: ControlMode Accelerometer::getControlMode()const noexcept{
        return control_mode;
    }

    void Accelerometer::setControlMode(Accelerometer::ControlMode mode)noexcept{
        control_mode = mode;
    }

    uint8_t Accelerometer::getCommTargetReg()const noexcept{
        return comm_target_reg;
    }

    void Accelerometer::setCommTargetReg(uint8_t reg)noexcept{
        comm_target_reg = reg;
    }

    bool Accelerometer::getActive()const noexcept{
        return active;
    }

    void Accelerometer::setActive(bool a)noexcept{
        active = a;
    }

    uint8_t Accelerometer::getRange()const noexcept{
        return range;
    }

    void Accelerometer::setRange(uint8_t r)noexcept{
        range = r;
    }

    float Accelerometer::getXAccel()const noexcept{
        return x_accel;
    }

    void Accelerometer::setXAccel(float accel)noexcept{
        x_accel = accel;
    }

    float Accelerometer::getYAccel()const noexcept{
        return y_accel;
    }

    void Accelerometer::setYAccel(float accel)noexcept{
        y_accel = accel;
    }

    float Accelerometer::getZAccel()const noexcept{
        return z_accel;
    }

    void Accelerometer::setZAccel(float accel)noexcept{
       z_accel = accel;
    }

    float Accelerometer::convertAccel(std::pair<uint8_t, uint8_t> accel)noexcept{
        uint16_t raw = (accel.first << 4) | (accel.second >> 4);
        raw <<= 4; //correctly convert the integer to 2's compliment if negative
        raw >>= 4;

        switch(range) { //correctly scale raw to accel using range as done by HAL
            case 0: //2G
                return raw / 1024.0f;
            case 1: //4G
                return raw / 512.0f;
            case 2: //8G
                return raw / 256.0f;
            default:
                return 0.0f;
        }
    }

    std::pair<uint8_t, uint8_t> Accelerometer::convertAccel(float accel)noexcept{
        switch(range) { //correctly scale accel to raw using range as expected by HAL
        case 0: //2G
            accel *= 1024.0f;
            break;
        case 1: //4G
            accel *= 512.0f;
            break;
        case 2: //8G
            accel *= 256.0f;
            break;
        default:
            accel = 0.0f;
            break;
        }
        int16_t raw = (int16_t)accel;

        uint8_t first = raw >> 4; //split raw into two bytes as expected by HAL
        uint8_t second = raw << 4;
        return {first, second};
    }

    Accelerometer::Accelerometer()noexcept:control_mode(ControlMode::SET_COMM_TARGET),comm_target_reg(0),active(false),range(0),x_accel(0.0),y_accel(0.0),z_accel(0.0){}

    Accelerometer::Accelerometer(const Accelerometer& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(control_mode);
        COPY(comm_target_reg);
        COPY(active);
        COPY(range);
        COPY(x_accel);
        COPY(y_accel);
        COPY(z_accel);
#undef COPY
    }

    struct AccelerometerManager: public tAccel{
    private:
        static constexpr uint8_t ID = 0x2a;

        static constexpr uint8_t SEND_ADDRESS = (0x1c << 1) | 0;
        static constexpr uint8_t RECEIVE_ADDRESS = (0x1c << 1) | 1;

        static constexpr uint8_t CONTROL_TX_RX = 1;
        static constexpr uint8_t CONTROL_START = 2;
        static constexpr uint8_t CONTROL_STOP = 4;
    public:
        tSystemInterface* getSystemInterface(){
            return new SystemInterface();
        }

        uint8_t readSTAT(tRioStatusCode* /*status*/){
            return 0; //since functions are essentially completed instantly, status is always 0, or done
        }

        void writeDATO(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            switch(instance.first->accelerometer.getControlMode()){
                case Accelerometer::ControlMode::SET_COMM_TARGET:
                    instance.first->accelerometer.setCommTargetReg(value);
                    instance.second.unlock();
                    return;
                case Accelerometer::ControlMode::SET_DATA:
                    switch(instance.first->accelerometer.getCommTargetReg()){
                        case Accelerometer::Register::kReg_CtrlReg1:
                            instance.first->accelerometer.setActive(value & 1u); //Gets first bit in value
                            instance.second.unlock();
                            return;
                        case Accelerometer::Register::kReg_XYZDataCfg:
                            instance.first->accelerometer.setRange(value & 3u); //Gets first two bits in value
                            instance.second.unlock();
                            return;
                        default:
                            instance.second.unlock();
                            throw UnhandledEnumConstantException("hel::Accelerometer::Register");
                    }
                    instance.second.unlock();
                    return;
                default:
                    instance.second.unlock();
                    throw UnhandledEnumConstantException("hel::Accelerometer::ControlMode");
            }
        }

        uint8_t readDATO(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        void writeCNTR(uint8_t /*value*/, tRioStatusCode* /*status*/){} //unnecessary for emulation

        uint8_t readCNTR(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        void writeCNFG(uint8_t /*value*/, tRioStatusCode* /*status*/){} //unnecessary for emulation

        uint8_t readCNFG(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        void writeCNTL(uint8_t value, tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();
            Accelerometer::ControlMode control_mode = [&]{
                if(value == (CONTROL_START | CONTROL_TX_RX)){//HAL sets value to this when setting up write to comm target
                    return Accelerometer::ControlMode::SET_COMM_TARGET;
                }
                return Accelerometer::ControlMode::SET_DATA;
            }();
            instance.first->accelerometer.setControlMode(control_mode);
            instance.second.unlock();
        }

        uint8_t readCNTL(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint8_t readDATI(tRioStatusCode* /*status*/){
            auto instance = RoboRIOManager::getInstance();

            switch(instance.first->accelerometer.getCommTargetReg()){
            case Accelerometer::Register::kReg_WhoAmI:
                instance.second.unlock();
                return ID;
            case Accelerometer::Register::kReg_CtrlReg1:
                instance.second.unlock();
                return instance.first->accelerometer.getActive();
            case Accelerometer::Register::kReg_XYZDataCfg:
                instance.second.unlock();
                return instance.first->accelerometer.getRange();
            case Accelerometer::Register::kReg_OutXMSB:
                instance.second.unlock();
                return instance.first->accelerometer.convertAccel(instance.first->accelerometer.getXAccel()).first;
            case Accelerometer::Register::kReg_OutXLSB:
                instance.second.unlock();
                return instance.first->accelerometer.convertAccel(instance.first->accelerometer.getXAccel()).second;
            case Accelerometer::Register::kReg_OutYMSB:
                instance.second.unlock();
                return instance.first->accelerometer.convertAccel(instance.first->accelerometer.getYAccel()).first;
            case Accelerometer::Register::kReg_OutYLSB:
                instance.second.unlock();
                return instance.first->accelerometer.convertAccel(instance.first->accelerometer.getYAccel()).second;
            case Accelerometer::Register::kReg_OutZMSB:
                instance.second.unlock();
                return instance.first->accelerometer.convertAccel(instance.first->accelerometer.getZAccel()).first;
            case Accelerometer::Register::kReg_OutZLSB:
                instance.second.unlock();
                return instance.first->accelerometer.convertAccel(instance.first->accelerometer.getZAccel()).second;
            default:
                instance.second.unlock();
                throw UnhandledEnumConstantException("hel::Accelerometer::Register");
            }
        }

        void strobeGO(tRioStatusCode* /*status*/){} //unnecessary for emulation

        void writeADDR(uint8_t /*value*/, tRioStatusCode* /*status*/){} //unnecessary for emulation

        uint8_t readADDR(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }
    };
}

namespace nFPGA{
    namespace nRoboRIO_FPGANamespace{
        tAccel* tAccel::create(tRioStatusCode* /*status*/){
            return new hel::AccelerometerManager();
        }
    }
}
