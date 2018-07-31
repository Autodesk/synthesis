#ifndef _ENCODER_HPP_
#define _ENCODER_HPP_

#include <cstdint>

namespace hel{

    struct Encoder{ //TODO implement
        enum class PortType{
            DI,
            AI
        };

        enum class Type{
            FPGA_ENCODER,
            COUNTER
        };

    private:
        Type type;

        uint8_t a_channel;

        PortType a_type;

        uint8_t b_channel;

        PortType b_type;

        int32_t ticks;

        void findDevice();

    public:
        Encoder(uint8_t,PortType,uint8_t,PortType);
    };
}

#endif
