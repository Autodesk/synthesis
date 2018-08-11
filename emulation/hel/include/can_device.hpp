#ifndef _CAN_DEVICE_HPP_
#define _CAN_DEVICE_HPP_

#include <string>
#include <cstdint>

namespace hel{

    /**
     * \brief Models a CAN device on the CAN bus
     */

    struct CANDevice{

        /**
         * \enum Type
         * \brief The various CAN devices Synthesis can recognize
         */

        enum class Type{VICTOR_SPX,TALON_SRX,PCM,PDP,UNKNOWN};

        /**
         * \var static constexpr uint8_t MAX_CAN_BUS_ADDRESS
         * \brief The maximum CAN bus address allowed on the RoboRIO
         * Valid addresses are 0-62
         */

        static constexpr uint8_t MAX_CAN_BUS_ADDRESS = 62;

    private:
        /**
         * \enum IDMask
         * \brief Bit fields to compare CAN message IDs against for identification
         *
         * Masks can be used to determine the device type and capture its ID
         */

        enum IDMask: uint32_t{
            DEVICE_ID = 0b01111111,
            DEVICE_TYPE = 0b1111000001000000000000000000,
            TALON_SRX_TYPE = 0x02040000,
            VICTOR_SPX_TYPE = 0x01040000,
            PCM_TYPE = 0x09041000,
            PDP_TYPE = 0x08041000
        };

    public:
        /**
         * \fn static uint8_t pullDeviceID(uint32_t messageID)noexcept
         * \brief Captures the target CAN device ID from a CAN message ID
         * \param messageID The CAN message ID to parse
         * \return A byte representing the CAN device ID
         */

        static uint8_t pullDeviceID(uint32_t)noexcept;

        /**
         * \fn static uint8_t pullDeviceType(uint32_t messageID)noexcept
         * \brief Captures the target CAN device type from a CAN message ID
         * \param messageID The CAN message ID to parse
         * \return The type of CAN device
         */

        static Type pullDeviceType(uint32_t)noexcept;
    };

    /**
     * \fn std::string as_string(CANDevice::Type type)
     * \brief Converts a CANDevice::Type to a string
     * \param type The CANDevice::Type to convert
     * \return A string representation of the CANDevice::Type
     */

    std::string as_string(CANDevice::Type);

    /**
     * \fn CANDevice::Type s_to_can_device_type(std::string input)
     * \brief Converts a string to a CANDevice::Type
     * \param input The string to parse
     * \return The parsed CANDevice::Type
     */

    CANDevice::Type s_to_can_device_type(std::string);
}

#endif
