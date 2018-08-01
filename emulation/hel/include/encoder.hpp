#ifndef _ENCODER_HPP_
#define _ENCODER_HPP_

#include <cstdint>
#include <string>

namespace hel{

    struct Encoder{ //TODO implement
        enum class PortType{
            DI,
            AI
        };

        enum class Type{
            UNKNOWN,
            FPGA_ENCODER,
            COUNTER
        };

    private:
        Type type;

        uint8_t index;

        uint8_t a_channel;

        PortType a_type;

        uint8_t b_channel;

        PortType b_type;

        int32_t ticks;

        bool checkDevice(uint8_t, bool, bool, uint8_t, bool, bool)const;

        void findDevice();

    public:
        void setTicks(int32_t);

        void update();

        std::string serialize()const;

        static Encoder deserialize(std::string);

        std::string toString()const;

        Encoder();
        Encoder(uint8_t,PortType,uint8_t,PortType);
    };

    std::string to_string(Encoder::Type);
    std::string to_string(Encoder::PortType);
}

#endif
