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

    public:
        bool checkDevice(uint8_t, bool, bool, uint8_t, bool, bool)const noexcept;
        void updateDevice();
        Type getType()const noexcept;
        uint8_t getIndex()const noexcept;
        uint8_t getAChannel()const noexcept;
        void setAChannel(uint8_t)noexcept;
        PortType getAType()const noexcept;
        void setAType(PortType)noexcept;
        uint8_t getBChannel()const noexcept;
        void setBChannel(uint8_t)noexcept;
        PortType getBType()const noexcept;
        void setBType(PortType)noexcept;
        void setTicks(int32_t)noexcept;
        int32_t getTicks()const noexcept;
        void update();

        std::string serialize()const;

        static EncoderManager deserialize(std::string);

        std::string toString()const;

        EncoderManager()noexcept;
        EncoderManager(const EncoderManager&)noexcept;
        EncoderManager(uint8_t,PortType,uint8_t,PortType)noexcept;
    };

    std::string to_string(EncoderManager::Type);
    std::string to_string(EncoderManager::PortType);
}

#endif
