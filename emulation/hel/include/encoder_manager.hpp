#ifndef _ENCODER_MANAGER_HPP_
#define _ENCODER_MANAGER_HPP_

#include <cstdint>
#include <string>

namespace hel{

    struct EncoderManager{
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
        Type getType()const;
        uint8_t getIndex()const;
        uint8_t getAChannel()const;
        void setAChannel(uint8_t);
        PortType getAType()const;
        void setAType(PortType);
        uint8_t getBChannel()const;
        void setBChannel(uint8_t);
        PortType getBType()const;
        void setBType(PortType);
        void setTicks(int32_t);
        int32_t getTicks()const;
        void update(); //TODO add reset capability

        std::string serialize()const;

        static EncoderManager deserialize(std::string);

        std::string toString()const;

        EncoderManager();
        EncoderManager(uint8_t,PortType,uint8_t,PortType);
    };

    std::string to_string(EncoderManager::Type);
    std::string to_string(EncoderManager::PortType);
}

#endif
