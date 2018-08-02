#ifndef _CAN_DEVICE_HPP_
#define _CAN_DEVICE_HPP_

#include <string>
#include <cstdint>

namespace hel{

    /**
     * \struct CANMotorController
     * \brief Models a CAN device on the CAN bus
     */

    struct CANDevice{

        enum class Type{VICTOR_SPX,TALON_SRX,PCM,PDP,UNKNOWN};

        /**
         * \var static constexpr uint8_t MAX_CAN_BUS_ADDRESS
         * \brief The maximum CAN bus address allowed on the RoboRIO
         * Valid addresses are 0-62, but using 0 is not recommended
         */

        static constexpr uint8_t MAX_CAN_BUS_ADDRESS = 62;

    private:
        enum IDMask: uint32_t{
            DEVICE_ID = 0b01111111,
            DEVICE_TYPE = 0b1111000001000000000000000000,
            TALON_SRX_TYPE = 0x02040000,
            VICTOR_SPX_TYPE = 0x01040000,
            PCM_TYPE = 0x09041000,
            PDP_TYPE = 0x08041000
        };

    public:
        static uint8_t pullDeviceID(uint32_t)noexcept;
        static Type pullDeviceType(uint32_t)noexcept;
    };

    std::string to_string(CANDevice::Type);

    CANDevice::Type s_to_can_device_type(std::string);
}

#endif
