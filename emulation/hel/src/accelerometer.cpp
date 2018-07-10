#include "roborio.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    RoboRIO::Accelerometer:: ControlMode RoboRIO::Accelerometer::getControlMode()const{
        return control_mode;
    }

    void RoboRIO::Accelerometer::setControlMode(RoboRIO::Accelerometer::ControlMode mode){
        control_mode = mode;
    }

    uint8_t RoboRIO::Accelerometer::getCommTargetReg()const{
        return comm_target_reg;
    }

    void RoboRIO::Accelerometer::setCommTargetReg(uint8_t reg){
        comm_target_reg = reg;
    }

    bool RoboRIO::Accelerometer::getActive()const{
        return active;
    }

    void RoboRIO::Accelerometer::setActive(bool a){
        active = a;
    }

    uint8_t RoboRIO::Accelerometer::getRange()const{
        return range;
    }

    void RoboRIO::Accelerometer::setRange(uint8_t r){
        range = r;
    }

    float RoboRIO::Accelerometer::getXAccel()const{
        return x_accel;
    }

    void RoboRIO::Accelerometer::setXAccel(bool accel){
        x_accel = accel;
    }

    float RoboRIO::Accelerometer::getYAccel()const{
        return y_accel;
    }

    void RoboRIO::Accelerometer::setYAccel(bool accel){
        y_accel = accel;
    }

    float RoboRIO::Accelerometer::getZAccel()const{
        return z_accel;
    }

    void RoboRIO::Accelerometer::setZAccel(bool accel){
       z_accel = accel; 
    }

    float RoboRIO::Accelerometer::convertAccel(std::pair<uint8_t, uint8_t> accel){
        uint16_t raw = (accel.first << 4) | (accel.second >> 4);
        raw <<= 4; //correctly convert the integer to 2's compliment if negative
        raw >>= 4;

        switch(range) {
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

    std::pair<uint8_t, uint8_t> RoboRIO::Accelerometer::convertAccel(float accel){
        accel = [&]{
            switch(range) {
                case 0: //2G
                    return accel * 1024.0f;
                case 1: //4G
                    return accel * 512.0f;
                case 2: //8G
                    return accel * 256.0f;
                default:
                    return 0.0f;
            }
        }();

        int16_t raw = (int16_t)accel;

        uint8_t first = raw >> 4;
        uint8_t second = raw << 4;
        return {first, second};
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
            return nullptr;
        }

        uint8_t readSTAT(tRioStatusCode* /*status*/){
            return 0; //since functions are essentially completed instantly, status is always 0, or done
        }

        void writeDATO(uint8_t value, tRioStatusCode* /*status*/){
            switch(RoboRIOManager::getInstance()->accelerometer.getControlMode()){
                case RoboRIO::Accelerometer::ControlMode::SET_COMM_TARGET:
                    RoboRIOManager::getInstance()->accelerometer.setCommTargetReg(value);
                    return;
                case RoboRIO::Accelerometer::ControlMode::SET_DATA:
                    switch(RoboRIOManager::getInstance()->accelerometer.getCommTargetReg()){
                        case RoboRIO::Accelerometer::Register::kReg_CtrlReg1:
                            RoboRIOManager::getInstance()->accelerometer.setActive(value & 1u); //Gets first bit in value
                            return;
                        case RoboRIO::Accelerometer::Register::kReg_XYZDataCfg:
                            RoboRIOManager::getInstance()->accelerometer.setRange(value & 3u); //Gets first two bits in value
                            return;
                        default:
                           return; //TODO error handling 
                    }
                    return;
                default:
                    return; //TODO error handling
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
            RoboRIO::Accelerometer::ControlMode control_mode = [&]{
                if(value == (CONTROL_START | CONTROL_TX_RX)){
                    return RoboRIO::Accelerometer::ControlMode::SET_COMM_TARGET;
                }
                return RoboRIO::Accelerometer::ControlMode::SET_DATA;
            }();
            RoboRIOManager::getInstance()->accelerometer.setControlMode(control_mode);
        }

        uint8_t readCNTL(tRioStatusCode* /*status*/){ //unnecessary for emulation
            return 0;
        }

        uint8_t readDATI(tRioStatusCode* /*status*/){
            switch(RoboRIOManager::getInstance()->accelerometer.getCommTargetReg()){
                case RoboRIO::Accelerometer::Register::kReg_WhoAmI:
                    return ID;
                case RoboRIO::Accelerometer::Register::kReg_CtrlReg1:
                    return RoboRIOManager::getInstance()->accelerometer.getActive();
                case RoboRIO::Accelerometer::Register::kReg_XYZDataCfg:
                    return RoboRIOManager::getInstance()->accelerometer.getRange();
                case RoboRIO::Accelerometer::Register::kReg_OutXMSB:
                    return RoboRIOManager::getInstance()->accelerometer.convertAccel(RoboRIOManager::getInstance()->accelerometer.getXAccel()).first;
                case RoboRIO::Accelerometer::Register::kReg_OutXLSB:
                    return RoboRIOManager::getInstance()->accelerometer.convertAccel(RoboRIOManager::getInstance()->accelerometer.getXAccel()).second;
                case RoboRIO::Accelerometer::Register::kReg_OutYMSB:
                    return RoboRIOManager::getInstance()->accelerometer.convertAccel(RoboRIOManager::getInstance()->accelerometer.getYAccel()).first;
                case RoboRIO::Accelerometer::Register::kReg_OutYLSB:
                    return RoboRIOManager::getInstance()->accelerometer.convertAccel(RoboRIOManager::getInstance()->accelerometer.getYAccel()).second;
                case RoboRIO::Accelerometer::Register::kReg_OutZMSB:
                    return RoboRIOManager::getInstance()->accelerometer.convertAccel(RoboRIOManager::getInstance()->accelerometer.getZAccel()).first;
                case RoboRIO::Accelerometer::Register::kReg_OutZLSB:
                    return RoboRIOManager::getInstance()->accelerometer.convertAccel(RoboRIOManager::getInstance()->accelerometer.getZAccel()).second;
            }
            return 0; //TODO error handling
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
