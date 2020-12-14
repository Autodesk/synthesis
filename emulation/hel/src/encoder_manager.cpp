#include "roborio_manager.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    std::string asString(EncoderManager::Type type){
        switch(type){
        case EncoderManager::Type::UNKNOWN:
            return "UNKNOWN";
        case EncoderManager::Type::FPGA_ENCODER:
            return "FPGA_ENCODER";
        case EncoderManager::Type::COUNTER:
            return "COUNTER";
        default:
            throw UnhandledEnumConstantException("hel::EncoderManager::Type");
        }
    }
    std::string asString(EncoderManager::PortType port_type){
        switch(port_type){
        case EncoderManager::PortType::DI:
            return "DI";
        case EncoderManager::PortType::AI:
            return "AI";
        default:
            throw UnhandledEnumConstantException("hel::EncoderManager::PortType");
        }
    }

    EncoderManager::PortType s_to_encoder_port_type(std::string input){
        switch(hasher(input.c_str())){
        case hasher("DI"):
            return EncoderManager::PortType::DI;
        case hasher("AI"):
            return EncoderManager::PortType::AI;
        default:
            throw UnhandledCase();
        }
    }

    uint8_t getChannel(uint8_t channel, bool module,EncoderManager::PortType type){
        if(module){ //HAL calls this module, but it appears to be simple if it's on the MXP or not
            switch(type){
            case EncoderManager::PortType::AI:
                return channel + AnalogInputs::NUM_ANALOG_INPUTS_HDRS;
            case EncoderManager::PortType::DI:
                return channel + DigitalSystem::NUM_DIGITAL_HEADERS;
            default:
                throw UnhandledEnumConstantException("hel::EncoderManager::PortType");
            }
        }
        return channel;
    }

    bool EncoderManager::checkDevice(uint8_t a, bool a_module, bool a_analog, uint8_t b, bool b_module, bool b_analog)const noexcept{
        if(
            (a_analog && a_type != PortType::AI) ||
            (b_analog && b_type != PortType::AI)
        ){
            return false;
        }
        if(getChannel(a, a_module, a_type) != a_channel || getChannel(b, b_module, b_type) != b_channel){
            return false;
        }
        return true;
    }

    void EncoderManager::updateDevice(){
        auto instance = RoboRIOManager::getInstance();
        for(unsigned i = 0; i < instance.first->fpga_encoders.size(); i++){ //check if FPGA encoder
            tEncoder::tConfig config = instance.first->fpga_encoders[i].getConfig();
            if(checkDevice(config.ASource_Channel,config.ASource_Module,config.ASource_AnalogTrigger,config.BSource_Channel,config.BSource_Module,config.BSource_AnalogTrigger)){
                type = Type::FPGA_ENCODER;
                index = i;
                instance.second.unlock();
                return;
            }
        }
        for(unsigned i = 0; i < instance.first->counters.size(); i++){ //check if counter
            tCounter::tConfig config = instance.first->counters[i].getConfig();
            if(checkDevice(config.UpSource_Channel,config.UpSource_Module,config.UpSource_AnalogTrigger,config.DownSource_Channel,config.DownSource_Module,config.DownSource_AnalogTrigger)){
                type = Type::COUNTER;
                index = i;
                instance.second.unlock();
                return;
            }
        }
        type = Type::UNKNOWN;
        index = 0;
        instance.second.unlock();
    }

    EncoderManager::Type EncoderManager::getType()const noexcept{
        return type;
    }

    uint8_t EncoderManager::getIndex()const noexcept{
        return index;    }

    uint8_t EncoderManager::getAChannel()const noexcept{
        return a_channel;
    }

    void EncoderManager::setAChannel(uint8_t a)noexcept{
        a_channel = a;
    }

    EncoderManager::PortType EncoderManager::getAType()const noexcept{
        return a_type;
    }

    void EncoderManager::setAType(PortType a_t)noexcept{
        a_type = a_t;
    }

    uint8_t EncoderManager::getBChannel()const noexcept{
        return b_channel;
    }

    void EncoderManager::setBChannel(uint8_t b)noexcept{
        b_channel = b;
    }

    EncoderManager::PortType EncoderManager::getBType()const noexcept{
        return b_type;
    }

    void EncoderManager::setBType(PortType b_t)noexcept{
        b_type = b_t;
    }

    void EncoderManager::setTicks(int32_t t)noexcept{
        ticks = t;
    }

    int32_t EncoderManager::getTicks()const noexcept{
        return ticks;
    }

    void EncoderManager::update(){
        updateDevice();
        auto instance = RoboRIOManager::getInstance();
        switch(type){
        case Type::UNKNOWN:
            instance.second.unlock();
            warn("No matching input found in user code for input configured in robot model (EncoderManager with a channel on " + asString(a_type) + " port " + std::to_string((unsigned)a_channel) + " and b channel on " + asString(b_type) + " port " + std::to_string((unsigned)b_channel) + ")");
            return;
        case Type::FPGA_ENCODER:
        {
            tEncoder::tOutput output;
            output.Value = getTicks();
            output.Direction = ticks < 0;
            instance.first->fpga_encoders[index].setRawOutput(output);
            break;
        }
        case Type::COUNTER:
        {
            tCounter::tOutput output;
            output.Value = getTicks();
            output.Direction = ticks < 0;
            instance.first->counters[index].setRawOutput(output);
            break;
        }
        default:
            throw UnhandledEnumConstantException("hel::EncoderManager::Type");
        }
        instance.second.unlock();
    }

    std::string EncoderManager::toString()const{
        std::string s = "(";
        s += "type:" + asString(type) + ", ";
        s += "index:" + std::to_string(index) + ", ";
        s += "a_channel:" + std::to_string(a_channel) + ", ";
        s += "a_type:" + asString(a_type) + ", ";
        s += "b_channel:" + std::to_string(b_channel) + ", ";
        s += "b_type:" + asString(b_type) + ", ";
        s += "ticks:" + std::to_string(ticks);
        s += ")";
        return s;
    }

    EncoderManager::EncoderManager()noexcept:EncoderManager(0,PortType::DI,0,PortType::DI){}
    EncoderManager::EncoderManager(const EncoderManager& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(type);
        COPY(index);
        COPY(a_channel);
        COPY(a_type);
        COPY(b_channel);
        COPY(b_type);
        COPY(ticks);
#undef COPY
    }
    EncoderManager::EncoderManager(uint8_t a,PortType a_t,uint8_t b,PortType b_t)noexcept:type(Type::UNKNOWN),index(0),a_channel(a),a_type(a_t),b_channel(b),b_type(b_t),ticks(0){}
}
