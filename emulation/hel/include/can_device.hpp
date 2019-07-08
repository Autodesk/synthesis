#ifndef _CAN_DEVICE_HPP_
#define _CAN_DEVICE_HPP_

#include <string>
#include <cstdint>
#include <vector>

namespace hel{

    /**
     * \brief Interprets the CAN message IDs created by HAL
     */

    struct CANMessageID{

        enum class Type{
            TALON_SRX,
            VICTOR_SPX,
            SPARK_MAX,
            PCM,
            PDP,
            UNKNOWN
        };

        enum class Manufacturer: int32_t{
            BROADCAST = 0,
            NI = 1,
            LM = 2,
            DEKA = 3,
            CTRE = 4,
            REV = 5,
            MS = 7,
            TEAM_USE = 8,
         };

        /**
         * \brief The maximum CAN bus address allowed on the RoboRIO
         * Valid addresses are 0-62
         */

        static constexpr uint8_t MAX_CAN_BUS_ADDRESS = 62;

    private:
        Type type;

        Manufacturer manufacturer;

        int32_t api_id;

        uint8_t id;

    public:
        static CANMessageID parse(uint32_t);

        static uint32_t generate(Type, Manufacturer, int32_t, uint8_t)noexcept;

      std::string toString()const;

        Type getType()const noexcept;

        Manufacturer getManufacturer()const noexcept;

        int32_t getAPIID()const noexcept;

        uint8_t getID()const noexcept;
    };

    std::string asString(CANMessageID::Manufacturer);

    std::string asString(CANMessageID::Type);

    CANMessageID::Type s_to_can_device_type(std::string);

    /**
     * \brief Models a CAN device on the CAN bus
     */

    struct CANDevice{
    private:

        /**
         * \brief The type of CAN device
         */

        CANMessageID::Type type;

        /**
         * \brief The CAN device ID
         */

        uint8_t id;

    public:

        virtual std::string toString()const = 0;

        CANMessageID::Type getType()const noexcept;

        uint8_t getID()const noexcept;

        virtual void parseCANPacket(const int32_t&, const std::vector<uint8_t>&) = 0;

        virtual std::vector<uint8_t> generateCANPacket(const int32_t&)const = 0;

        CANDevice()noexcept;

        CANDevice(const CANDevice&)noexcept;

        CANDevice(CANMessageID)noexcept;
    };
}

#endif
