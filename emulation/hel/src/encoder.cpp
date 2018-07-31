#include "roborio_manager.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    void Encoder::findDevice(){
        auto instance = RoboRIOManager::getInstance();
        for(FPGAEncoder fpga_encoder: instance.first->fpga_encoders){
            tEncoder::tConfig config = fpga_encoder.getConfig();
            if(config.ASource_AnalogTrigger && a_type != PortType::AI){
                continue;
            }
            if(config.BSource_AnalogTrigger && b_type != PortType::AI){
                continue;
            }
            uint8_t a =
                [&]{
                    uint8_t channel = config.ASource_Channel;
                    if(config.ASource_Module){
                        switch(a_type){
                        case PortType::AI:
                            channel += AnalogInputs::NUM_ANALOG_INPUTS_HDRS;
                            break;
                        case PortType::DI:
                            channel += DigitalSystem::NUM_DIGITAL_HEADERS;
                            break;
                        default:
                            throw UnhandledEnumConstantException("hel::Encoder::PortType");
                        }
                    }
                    return channel;
                }();
            //TODO
        }
        instance.second.unlock();
    }

    Encoder::Encoder(uint8_t a,PortType a_t,uint8_t b,PortType b_t):type(Type::FPGA_ENCODER),a_channel(a),a_type(a_t),b_channel(b),b_type(b_t),ticks(0){
        findDevice();
    }
}
