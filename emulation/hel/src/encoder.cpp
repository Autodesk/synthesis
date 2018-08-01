#include "roborio_manager.hpp"

#include "json_util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    std::string to_string(Encoder::Type type){
        switch(type){
        case Encoder::Type::UNKNOWN:
            return "UNKNOWN";
        case Encoder::Type::FPGA_ENCODER:
            return "FPGA_ENCODER";
        case Encoder::Type::COUNTER:
            return "COUNTER";
        default:
            throw UnhandledEnumConstantException("hel::Encoder::Type");
        }
    }
    std::string to_string(Encoder::PortType type){
        switch(type){
        case Encoder::PortType::DI:
            return "DI";
        case Encoder::PortType::AI:
            return "AI";
        default:
            throw UnhandledEnumConstantException("hel::Encoder::PortType");
        }
    }

    Encoder::PortType s_to_encoder_port_type(std::string s){
        switch(hasher(s.c_str())){
        case hasher("DI"):
            return Encoder::PortType::DI;
        case hasher("AI"):
            return Encoder::PortType::AI;
        default:
            throw UnhandledCase();
        }
    }

    uint8_t getChannel(uint8_t channel, bool module,Encoder::PortType type){
        if(module){
            switch(type){
            case Encoder::PortType::AI:
                channel += AnalogInputs::NUM_ANALOG_INPUTS_HDRS;
                break;
            case Encoder::PortType::DI:
                channel += DigitalSystem::NUM_DIGITAL_HEADERS;
                break;
            default:
                throw UnhandledEnumConstantException("hel::Encoder::PortType");
            }
        }
        return channel;
    }

    bool Encoder::checkDevice(uint8_t a, bool a_module, bool a_analog, uint8_t b, bool b_module, bool b_analog)const{
        if(
            (a_analog && a_type != PortType::AI) ||
            (b_analog && b_type != PortType::AI)
        ){
            return false;
        }
        a = getChannel(a, a_module, a_type);
        b = getChannel(b, b_module, b_type);
        if(a != a_channel || b != b_channel){
            return false;
        }
        return true;
    }

    void Encoder::findDevice(){
        auto instance = RoboRIOManager::getInstance();
        for(unsigned i = 0; i < instance.first->fpga_encoders.size(); i++){
            tEncoder::tConfig config = instance.first->fpga_encoders[i].getConfig();
            if(checkDevice(config.ASource_Channel,config.ASource_Module,config.ASource_AnalogTrigger,config.BSource_Channel,config.BSource_Module,config.BSource_AnalogTrigger)){
                type = Type::FPGA_ENCODER;
                index = i;
                instance.second.unlock();
                return;
            }
        }
        for(unsigned i = 0; i < instance.first->counters.size(); i++){
            tCounter::tConfig config = instance.first->counters[i].getConfig();
            if(checkDevice(config.UpSource_Channel,config.UpSource_Module,config.UpSource_AnalogTrigger,config.DownSource_Channel,config.DownSource_Module,config.DownSource_AnalogTrigger)){
                type = Type::COUNTER;
                index = i;
                instance.second.unlock();
                return;
            }
        }
        //TODO error, matching encoder not found
        type = Type::UNKNOWN;
        index = 0;
        instance.second.unlock();
    }

    void Encoder::setTicks(int32_t t){
        ticks = t;
    }

    void Encoder::update(){
        findDevice();
        auto instance = RoboRIOManager::getInstance();
        switch(type){
        case Type::UNKNOWN:
            instance.second.unlock();
            throw InputConfigurationException("Encoder with a channel on " + hel::to_string(a_type) + " port " + std::to_string(a_channel) + " and b channel on " + hel::to_string(b_type) + " port " + std::to_string(b_channel));
        case Type::FPGA_ENCODER:
        {
            tEncoder::tOutput output;
            output.Value = ticks;
            output.Direction = ticks < 0;
            instance.first->fpga_encoders[index].setOutput(output);
            break;
        }
        case Type::COUNTER:
        {
            tCounter::tOutput output;
            output.Value = ticks;
            output.Direction = ticks < 0;
            instance.first->counters[index].setOutput(output);
            break;
        }
        default:
            throw UnhandledEnumConstantException("hel::Encoder::Type");
        }
        instance.second.unlock();
    }

    std::string Encoder::serialize()const{
        std::string s = "{";
        s += "\"a_channel\":" + std::to_string(a_channel) + ", ";
        s += "\"a_type\":" + quote(hel::to_string(a_type)) + ", ";
        s += "\"b_channel\":" + std::to_string(b_channel) + ", ";
        s += "\"b_type\":" + quote(hel::to_string(b_type)) + ", ";
        s += "\"ticks\":" + std::to_string(ticks);
        s += "}";
        return s;
    }

    Encoder Encoder::deserialize(std::string s){
        Encoder a;
        a.a_channel = std::stoi(hel::pullValue("\"a_channel\"",s));
        a.b_channel = std::stoi(hel::pullValue("\"b_channel\"",s));
        a.a_type = hel::s_to_encoder_port_type(unquote(hel::pullValue("\"a_type\"",s)));
        a.b_type = hel::s_to_encoder_port_type(unquote(hel::pullValue("\"b_type\"",s)));
        a.ticks = std::stoi(hel::pullValue("\"ticks\"",s));
        return a;
    }

    std::string Encoder::toString()const{
        std::string s = "(";
        s += "type:" + hel::to_string(type) + ", ";
        s += "index:" + std::to_string(index) + ", ";
        s += "a_channel:" + std::to_string(a_channel) + ", ";
        s += "a_type:" + hel::to_string(a_type) + ", ";
        s += "b_channel:" + std::to_string(b_channel) + ", ";
        s += "b_type:" + hel::to_string(b_type) + ", ";
        s += "ticks:" + std::to_string(ticks);
        s += "}";
        return s;
    }

    Encoder::Encoder():Encoder(0,PortType::DI,0,PortType::DI){}
    Encoder::Encoder(uint8_t a,PortType a_t,uint8_t b,PortType b_t):type(Type::UNKNOWN),index(0),a_channel(a),a_type(a_t),b_channel(b),b_type(b_t),ticks(0){}
}
